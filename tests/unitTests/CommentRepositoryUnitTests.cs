using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Chirp.Core.Models;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Chirp.Tests;

namespace unitTests;

public class CommentRepositoryUnitTests : IAsyncLifetime
{
    private TestSqliteFactory<ChatDBContext> _fx = null!;

    public async Task InitializeAsync()
    {
        _fx = new TestSqliteFactory<ChatDBContext>(opts => new ChatDBContext(opts));
        await _fx.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    //  GET COMMENTS BY CHEEP ID


    [Fact]
    public async Task GetCommentsByCheepIdAsync_ReturnsOrderedComments_ForCheep()
    {
        int cheepId;

        // Seed
        await using (var seed = _fx.CreateContext())
        {
            var author = new Author { Name = "alice" };
            seed.Authors.Add(author);
            await seed.SaveChangesAsync();

            var cheep = new Cheep { Text = "hello", AuthorId = author.AuthorId };
            seed.Cheeps.Add(cheep);
            await seed.SaveChangesAsync();
            cheepId = cheep.CheepId;

            seed.Comments.AddRange(
                new Comment { Text = "first",  CheepId = cheepId, AuthorId = author.AuthorId, TimeStamp = DateTime.UtcNow.AddMinutes(-2) },
                new Comment { Text = "second", CheepId = cheepId, AuthorId = author.AuthorId, TimeStamp = DateTime.UtcNow.AddMinutes(-1) }
            );
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        var result = await repo.GetCommentsByCheepIdAsync(cheepId);

        result.Select(c => c.Text).Should().Equal("first", "second");
        result.All(c => c.AuthorName == "alice").Should().BeTrue();
    }

    [Fact]
    public async Task GetCommentsByCheepIdAsync_ReturnsEmpty_WhenNoComments()
    {
        int cheepId;

        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "bob" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();

            var c = new Cheep { Text = "none", AuthorId = a.AuthorId };
            seed.Cheeps.Add(c);
            await seed.SaveChangesAsync();
            cheepId = c.CheepId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        var result = await repo.GetCommentsByCheepIdAsync(cheepId);

        result.Should().BeEmpty();
    }

   
    //  GET COMMENT COUNT
   

    [Fact]
    public async Task GetCommentCountByCheepIdAsync_ReturnsCorrectCount()
    {
        int cheepId;

        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "alice" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();

            var cheep = new Cheep { Text = "test", AuthorId = a.AuthorId };
            seed.Cheeps.Add(cheep);
            await seed.SaveChangesAsync();
            cheepId = cheep.CheepId;

            seed.Comments.AddRange(
                new Comment { CheepId = cheepId, AuthorId = a.AuthorId, Text = "a" },
                new Comment { CheepId = cheepId, AuthorId = a.AuthorId, Text = "b" }
            );
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        var count = await repo.GetCommentCountByCheepIdAsync(cheepId);

        count.Should().Be(2);
    }

   
    //  CREATE COMMENT
   

    [Fact]
    public async Task CreateCommentAsync_CreatesAuthorIfMissing_AndSavesComment()
    {
        int cheepId;

        await using (var seed = _fx.CreateContext())
        {
            var existing = new Author { Name = "owner" };
            seed.Authors.Add(existing);
            await seed.SaveChangesAsync();

            var cheep = new Cheep { Text = "post", AuthorId = existing.AuthorId };
            seed.Cheeps.Add(cheep);
            await seed.SaveChangesAsync();

            cheepId = cheep.CheepId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        var dto = new CommentDTO
        {
            AuthorName = "newPerson@example.com",
            CheepId = cheepId,
            Text = "nice!"
        };

        await repo.CreateCommentAsync(dto);

        await using var verify = _fx.CreateContext();

        var savedAuthor = await verify.Authors.FirstOrDefaultAsync(a => a.Email == "newPerson@example.com");
        var savedComment = await verify.Comments.FirstOrDefaultAsync(c => c.CheepId == cheepId);

        savedAuthor.Should().NotBeNull();
        savedComment.Should().NotBeNull();

        savedComment!.Text.Should().Be("nice!");
        savedComment.AuthorId.Should().Be(savedAuthor!.AuthorId);
    }

    [Fact]
    public async Task CreateCommentAsync_UsesExistingAuthor_IfExists()
    {
        int cheepId;
        int existingAuthorId;

        // Seed
        await using (var seed = _fx.CreateContext())
        {
            var author = new Author { Name = "alice", Email = "alice@example.com" };
            seed.Authors.Add(author);
            await seed.SaveChangesAsync();
            existingAuthorId = author.AuthorId;

            var cheep = new Cheep { Text = "post", AuthorId = author.AuthorId };
            seed.Cheeps.Add(cheep);
            await seed.SaveChangesAsync();
            cheepId = cheep.CheepId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        var dto = new CommentDTO
        {
            AuthorName = "alice@example.com",
            CheepId = cheepId,
            Text = "hi"
        };

        await repo.CreateCommentAsync(dto);

        await using var verify = _fx.CreateContext();

        var savedComment = await verify.Comments.FirstOrDefaultAsync(c => c.CheepId == cheepId);

        savedComment.Should().NotBeNull();
        savedComment!.AuthorId.Should().Be(existingAuthorId);
    }

 
    //  DELETE COMMENT
 

    [Fact]
    public async Task DeleteCommentAsync_RemovesComment_WhenExists()
    {
        int commentId;

        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "alice" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();

            var cheep = new Cheep { Text = "post", AuthorId = a.AuthorId };
            seed.Cheeps.Add(cheep);
            await seed.SaveChangesAsync();

            var c = new Comment 
            {
                Text = "delete me",
                AuthorId = a.AuthorId,
                CheepId = cheep.CheepId,
                TimeStamp = DateTime.UtcNow
            };

            seed.Comments.Add(c);
            await seed.SaveChangesAsync();

            commentId = c.CommentId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        await repo.DeleteCommentAsync(commentId);

        await using var verify = _fx.CreateContext();
        var exists = await verify.Comments.AnyAsync(c => c.CommentId == commentId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCommentAsync_DoesNothing_WhenCommentMissing()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new CommentRepository(ctx);

        // Should NOT throw
        await repo.DeleteCommentAsync(999);

        await using var verify = _fx.CreateContext();
        var count = await verify.Comments.CountAsync();

        count.Should().Be(0);
    }
}
