using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Chirp.Core.Models;                   // Cheep, author, CheepDTO
using Chirp.Infrastructure;                 // ChatDBContext
using Chirp.Infrastructure.Repositories;    // CheepRepository

namespace Chirp.Tests;

// If you already have TestSqliteFactory<ChatDBContext> in your test project,
// this test class uses it directly. (It keeps a single open :memory: DB per class.)
public class CheepRepositoryUnitTests : IAsyncLifetime
{
    private TestSqliteFactory<ChatDBContext> _fx = null!;

    public async Task InitializeAsync()
    {
        _fx = new TestSqliteFactory<ChatDBContext>(opts => new ChatDBContext(opts));
        await _fx.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- CREATE ----------

    [Fact]
    public async Task CreateCheepAsync_CreatesUserIfMissing_AndReturnsId()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        var id = await repo.CreateCheepAsync(new CheepDTO
        {
            Author = "alice",
            Text = "hello world"
        });

        id.Should().BeGreaterThan(0);

        await using var verify = _fx.CreateContext();
        var saved = await verify.Cheeps
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.CheepId == id);

        saved.Should().NotBeNull();
        saved!.Text.Should().Be("hello world");
        saved.Author.Name.Should().Be("alice");
    }

    [Theory]
    [InlineData(null, "text", "Author is required.")]
    [InlineData("", "text", "Author is required.")]
    [InlineData("alice", null, "Text is required.")]
    [InlineData("alice", "", "Text is required.")]
    public async Task CreateCheepAsync_ValidatesInputs(string? author, string? text, string expectedMessage)
    {
        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        Func<Task> act = async () => await repo.CreateCheepAsync(new CheepDTO { Author = author!, Text = text! });
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage(expectedMessage + "*");
    }

    // ---------- READ ----------

    [Fact]
    public async Task ReadCheepsAsync_ReturnsDescendingAndFiltersByAuthor()
    {
        // seed
        await using (var seed = _fx.CreateContext())
        {
            var alice = new Author { Name = "alice" };
            var bob   = new Author { Name = "bob" };
            seed.Authors.AddRange(alice, bob);
            await seed.SaveChangesAsync();

            seed.Cheeps.AddRange(
                new Cheep { Text = "old",   TimeStamp = DateTime.UtcNow.AddMinutes(-3), AuthorId = alice.AuthorId },
                new Cheep { Text = "other", TimeStamp = DateTime.UtcNow.AddMinutes(-2), AuthorId = bob.AuthorId   },
                new Cheep { Text = "new",   TimeStamp = DateTime.UtcNow.AddMinutes(-1), AuthorId = alice.AuthorId }
            );
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        var result = await repo.ReadCheepsAsync(author: "alice", page: 0, pageSize: 32);

        result.Select(c => c.Text).Should().Equal("new", "old");             // order desc by timestamp
        result.Should().OnlyContain(c => c.Author == "alice");               // filter by author
        result.All(c => !string.IsNullOrWhiteSpace(c.Timestamp)).Should().BeTrue();

        // (Optional) Check timestamp format "MM/dd/yy H:mm:ss"
        var rx = new Regex(@"^\d{2}/\d{2}/\d{2} \d{1,2}:\d{2}:\d{2}$");
        result.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Timestamp) && rx.IsMatch(c.Timestamp!));    }

    [Fact]
    public async Task ReadCheepsAsync_AppliesPaging()
    {
        // seed 5 cheeps for alice with increasing timestamps
        await using (var seed = _fx.CreateContext())
        {
            var alice = new Author { Name = "alice" };
            seed.Authors.Add(alice);
            await seed.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                seed.Cheeps.Add(new Cheep {
                    Text = $"c{i}",
                    TimeStamp = DateTime.UtcNow.AddMinutes(-i),
                    AuthorId = alice.AuthorId
                });
            }
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        // pageSize 2: page 0 = newest two; page 1 = next two; page 2 = last one
        var p0 = await repo.ReadCheepsAsync("alice", page: 0, pageSize: 2);
        var p1 = await repo.ReadCheepsAsync("alice", page: 1, pageSize: 2);
        var p2 = await repo.ReadCheepsAsync("alice", page: 2, pageSize: 2);

        p0.Select(x => x.Text).Should().HaveCount(2);
        p1.Select(x => x.Text).Should().HaveCount(2);
        p2.Select(x => x.Text).Should().HaveCount(1);

        // Ensure no overlap and overall ordering (c0 newest)
        var combined = p0.Concat(p1).Concat(p2).Select(x => x.Text).ToList();
        combined.Should().Equal("c0","c1","c2","c3","c4");
    }

    // ---------- UPDATE ----------

    [Fact]
    public async Task UpdateCheepAsync_ChangesOnlyText_WhenCheepExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var u = new Author { Name = "alice" };
            seed.Authors.Add(u);
            await seed.SaveChangesAsync();

            var c = new Cheep { Text = "before", TimeStamp = DateTime.UtcNow.AddMinutes(-5), AuthorId = u.AuthorId };
            seed.Cheeps.Add(c);
            await seed.SaveChangesAsync();
            id = c.CheepId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        await repo.UpdateCheepAsync(new CheepDTO { Id = id, Text = "after" });

        await using var verify = _fx.CreateContext();
        var updated = await verify.Cheeps.AsNoTracking().FirstAsync(c => c.CheepId == id);
        updated.Text.Should().Be("after");
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task DeleteCheepAsync_RemovesRow_WhenCheepExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var u = new Author { Name = "alice" };
            seed.Authors.Add(u);
            await seed.SaveChangesAsync();

            var c = new Cheep { Text = "to delete", TimeStamp = DateTime.UtcNow, AuthorId = u.AuthorId };
            seed.Cheeps.Add(c);
            await seed.SaveChangesAsync();
            id = c.CheepId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        await repo.DeleteCheepAsync(id);

        await using var verify = _fx.CreateContext();
        var exists = await verify.Cheeps.AnyAsync(c => c.CheepId == id);
        exists.Should().BeFalse();
    }
}
