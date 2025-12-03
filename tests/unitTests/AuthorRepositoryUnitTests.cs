using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Chirp.Core.Models;                  
using Chirp.Infrastructure;               
using Chirp.Infrastructure.Repositories;
using Chirp.Tests;

namespace unitTests;

public class AuthorRepositoryUnitTests : IAsyncLifetime
{
    private TestSqliteFactory<ChatDBContext> _fx = null!;

    public async Task InitializeAsync()
    {
        _fx = new TestSqliteFactory<ChatDBContext>(opts => new ChatDBContext(opts));
        await _fx.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    //  CREATING AUTHOR

    [Fact]
    public async Task CreateAuthorAsync_CreatesAuthor_AndReturnsId()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        var id = await repo.createAuthorAsync("alice", "alice@example.com");

        id.Should().BeGreaterThan(0);

        await using var verify = _fx.CreateContext();
        var saved = await verify.Authors.FirstOrDefaultAsync(a => a.AuthorId == id);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("alice");
        saved.Email.Should().Be("alice@example.com");
    }

    //  GET AUTHOR BY NAME

    [Fact]
    public async Task GetAuthorByNameAsync_ReturnsCorrectId_WhenExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "bob", Email = "b@b.com" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();
            id = a.AuthorId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        var result = await repo.getAuthorByNameAsync("bob");
        result.Should().Be(id);
    }

    [Fact]
    public async Task GetAuthorByNameAsync_Throws_WhenNotFound()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        Func<Task> act = async () => await repo.getAuthorByNameAsync("missing");

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Author with name 'missing' does not exist*");
    }

    //  GET AUTHOR BY EMAIL

    [Fact]
    public async Task GetAuthorByEmailAsync_ReturnsCorrectId_WhenExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "eva", Email = "eva@example.com" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();
            id = a.AuthorId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        var result = await repo.getAuthorByEmailAsync("eva@example.com");
        result.Should().Be(id);
    }

    [Fact]
    public async Task GetAuthorByEmailAsync_Throws_WhenNotFound()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        Func<Task> act = async () => await repo.getAuthorByEmailAsync("missing@example.com");

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Author with name 'missing@example.com' does not exist*");
    }

    //  DELETE AUTHOR

    [Fact]
    public async Task DeleteAuthorAsync_RemovesAuthor_WhenExists()
    {
        int id;
        await using (var seed = _fx.CreateContext())
        {
            var a = new Author { Name = "charlie", Email = "c@example.com" };
            seed.Authors.Add(a);
            await seed.SaveChangesAsync();
            id = a.AuthorId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        await repo.deleteAuthorAsync(id);

        await using var verify = _fx.CreateContext();
        var exists = await verify.Authors.AnyAsync(a => a.AuthorId == id);

        exists.Should().BeFalse();
    }

    //  CREATE FOLLOWING RELATIONSHIP

    [Fact]
    public async Task CreateFollowingAsync_CreatesRelation_WhenBothExist()
    {
        int followerId, followedId;

        await using (var seed = _fx.CreateContext())
        {
            var f = new Author { Name = "alice" };
            var t = new Author { Name = "bob" };
            seed.Authors.AddRange(f, t);
            await seed.SaveChangesAsync();
            followerId = f.AuthorId;
            followedId = t.AuthorId;
        }

        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        await repo.CreateFollowingAsync(followerId, followedId);

        await using var verify = _fx.CreateContext();
        var relation = await verify.Followings
            .FirstOrDefaultAsync(x => x.FollowerId == followerId && x.FollowedId == followedId);

        relation.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateFollowingAsync_Throws_WhenEitherAuthorMissing()
    {
        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        Func<Task> act = async () => await repo.CreateFollowingAsync(1000, 2000);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Follower or followed author does not exist*");
    }

    [Fact]
    public async Task CreateFollowingAsync_IgnoresDuplicateRelations()
    {
        int followerId, followedId;

        await using (var seed = _fx.CreateContext())
        {
            var f = new Author { Name = "alice" };
            var t = new Author { Name = "bob" };
            seed.Authors.AddRange(f, t);
            await seed.SaveChangesAsync();
            followerId = f.AuthorId;
            followedId = t.AuthorId;

            seed.Followings.Add(new Following { FollowerId = followerId, FollowedId = followedId });
            await seed.SaveChangesAsync();
        }

        await using var ctx = _fx.CreateContext();
        var repo = new AuthorRepository(ctx);

        // Should NOT throw
        await repo.CreateFollowingAsync(followerId, followedId);

        await using var verify = _fx.CreateContext();
        var count = await verify.Followings.CountAsync(f =>
            f.FollowerId == followerId && f.FollowedId == followedId);

        count.Should().Be(1);  // no duplicate added
    }
}
