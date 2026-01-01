using System.Text.RegularExpressions; 
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


using Chirp.Core.Models;                 
using Chirp.Infrastructure;                
using Chirp.Infrastructure.Repositories;
using Chirp.Tests;

namespace unitTests;


public class CheepRepositoryUnitTests : IAsyncLifetime
{
    private TestSqliteFactory<ChatDBContext> _fx = null!;

    public async Task InitializeAsync()
    {
        _fx = new TestSqliteFactory<ChatDBContext>(opts => new ChatDBContext(opts));
        await _fx.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    //  CREATE

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
        saved.Author.UserName.Should().Be("alice");
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

    //  READ

    [Fact]
    public async Task ReadCheepsAsync_ReturnsDescendingAndFiltersByAuthor()
    {
        
        await using (var seed = _fx.CreateContext())
        {
            var alice = new Author { UserName = "alice" };
            var bob   = new Author { UserName = "bob" };
            seed.Authors.AddRange(alice, bob);
            await seed.SaveChangesAsync();

            seed.Cheeps.AddRange(
                new Cheep { Text = "old",   TimeStamp = DateTime.UtcNow.AddMinutes(-3), AuthorId = alice.Id },
                new Cheep { Text = "other", TimeStamp = DateTime.UtcNow.AddMinutes(-2), AuthorId = bob.Id   },
                new Cheep { Text = "new",   TimeStamp = DateTime.UtcNow.AddMinutes(-1), AuthorId = alice.Id }
            );
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);

        var result = await repo.ReadCheepsAsync(author: "alice", page: 0, pageSize: 32);

        result.Select(c => c.Text).Should().Equal("new", "old");          
        result.Should().OnlyContain(c => c.Author == "alice");              
        result.All(c => !string.IsNullOrWhiteSpace(c.Timestamp)).Should().BeTrue();
        
        var rx = new Regex(@"^\d{2}/\d{2}/\d{2} \d{1,2}:\d{2}:\d{2}$");
        result.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Timestamp) && rx.IsMatch(c.Timestamp!));    }

    [Fact]
    public async Task ReadCheepsAsync_AppliesPaging()
    {
        await using (var seed = _fx.CreateContext())
        {
            var alice = new Author { UserName = "alice" };
            seed.Authors.Add(alice);
            await seed.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                seed.Cheeps.Add(new Cheep {
                    Text = $"c{i}",
                    TimeStamp = DateTime.UtcNow.AddMinutes(-i),
                    AuthorId = alice.Id
                });
            }
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CheepRepository(ctx);
        
        var p0 = await repo.ReadCheepsAsync("alice", page: 0, pageSize: 2);
        var p1 = await repo.ReadCheepsAsync("alice", page: 1, pageSize: 2);
        var p2 = await repo.ReadCheepsAsync("alice", page: 2, pageSize: 2);

        p0.Select(x => x.Text).Should().HaveCount(2);
        p1.Select(x => x.Text).Should().HaveCount(2);
        p2.Select(x => x.Text).Should().HaveCount(1);

        
        var combined = p0.Concat(p1).Concat(p2).Select(x => x.Text).ToList();
        combined.Should().Equal("c0","c1","c2","c3","c4");
    }

    //  UPDATE 

    [Fact]
    public async Task UpdateCheepAsync_ChangesOnlyText_WhenCheepExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var u = new Author { UserName = "alice" };
            seed.Authors.Add(u);
            await seed.SaveChangesAsync();

            var c = new Cheep { Text = "before", TimeStamp = DateTime.UtcNow.AddMinutes(-5), AuthorId = u.Id };
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

    //  DELETE 

    [Fact]
    public async Task DeleteCheepAsync_RemovesRow_WhenCheepExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var u = new Author { UserName = "alice" };
            seed.Authors.Add(u);
            await seed.SaveChangesAsync();

            var c = new Cheep { Text = "to delete", TimeStamp = DateTime.UtcNow, AuthorId = u.Id };
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
