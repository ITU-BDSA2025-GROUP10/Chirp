using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Chirp.Core.Models;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Service;


namespace IntegrationTests;

public class CheepServiceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CheepServiceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Helper method to clear the database before each test
    private async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

        // clear all data
        db.Followings.RemoveRange(db.Followings);
        db.Cheeps.RemoveRange(db.Cheeps);
        db.Authors.RemoveRange(db.Authors);
        await db.SaveChangesAsync();
    }
    
    // This test ensures that a user can visit our homepage without the app crashing
    [Fact]
    public async Task Homepage_ReturnsSuccessStatusCode()
    {
        // Arrange: Create an HttpClient
        var client = _factory.CreateClient();
        
        // Act: Make a request
        var response = await client.GetAsync("/");
        
        // Assert: Verify response
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
  public async Task GetCheeps()
  {
      // ARRANGE: Create a scenario with multiple authors and following relationships
      using (var scope = _factory.Services.CreateScope())
      {
          var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

          // Create three authors
          var alice = new Author { Name = "alice", Email = "alice@test.com" };
          var bob = new Author { Name = "bob", Email = "bob@test.com" };
          var charlie = new Author { Name = "charlie", Email = "charlie@test.com" };
          db.Authors.AddRange(alice, bob, charlie);
          await db.SaveChangesAsync();

          // Alice follows Bob (but NOT Charlie)
          var following = new Following
          {
              FollowerId = alice.AuthorId,
              FollowedId = bob.AuthorId
          };
          db.Followings.Add(following);
          await db.SaveChangesAsync();

          // Both Bob and Charlie create cheeps
          var bobCheep = new Cheep
          {
              Text = "Cheep from Bob",
              AuthorId = bob.AuthorId,
              TimeStamp = DateTime.UtcNow.AddMinutes(-1) // Older
          };
          var charlieCheep = new Cheep
          {
              Text = "Cheep from Charlie",
              AuthorId = charlie.AuthorId,
              TimeStamp = DateTime.UtcNow // Newer
          };
          db.Cheeps.AddRange(bobCheep, charlieCheep);
          await db.SaveChangesAsync();
      }

      // ACT: Request the public timeline (this internally calls GetFollowedIdsAsync())
      var client = _factory.CreateClient();
      var response = await client.GetAsync("/");

      // ASSERT: Verify the response and check isFollowed logic
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      var content = await response.Content.ReadAsStringAsync();

      // Both cheeps should appear in the database
      content.Should().Contain("Cheep from Bob");
      content.Should().Contain("Cheep from Charlie");
  }

    [Fact]
    public async Task AuthorTimeline_ShowOnlyAuthorsCheeps()
    {
        // ARRANGE: We create two authors with different cheeps
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
            
            var alice = new Author{ Name = "alice", Email = "alice@test.com" };
            var bob = new Author { Name = "bob", Email = "bob@test.com" };
            db.Authors.AddRange(alice, bob);
            await db.SaveChangesAsync();

            var aliceCheep = new Cheep
            {
                Text = "Cheep from Alice",
                AuthorId = alice.AuthorId,
                TimeStamp = DateTime.UtcNow
            };
            var bobCheep = new Cheep
            {
                Text = "Cheep from Bob",
                AuthorId = bob.AuthorId,
                TimeStamp = DateTime.UtcNow
            };
            db.Cheeps.AddRange(aliceCheep, bobCheep);
            await db.SaveChangesAsync();
        }
        
        // ACT: Request Alice's timeline
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/alice");
        
        // ASSERT: Only Alice's cheeps should appear
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Cheep from Alice");
        content.Should().NotContain("Cheep from Bob");
    }

	[Fact]
	public async Task UserFollowFunction()
	{
		// ARRANGE: set up data
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
			var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

		// Create authors
			var alice = new Author { Name = "alice", Email = "alice@test.com"};
			var bob = new Author { Name = "bob", Email = "bob@test.com"};
			var charlie = new Author { Name = "charlie", Email = "charlie@test.com"};
			db.Authors.AddRange(alice, bob, charlie);
			await db.SaveChangesAsync();

		// bob follows charlie
			await authorRepo.CreateFollowingAsync(bob.AuthorId, charlie.AuthorId);

		// create cheeps for both
			var bobCheep = new Cheep{ AuthorId = bob.AuthorId, Text = "Bob's test cheep", TimeStamp = DateTime.UtcNow };
			var charlieCheep = new Cheep{ AuthorId = charlie.AuthorId, Text = "Charlie's test cheep", TimeStamp = DateTime.UtcNow };
			db.Cheeps.AddRange(bobCheep, charlieCheep);
			await db.SaveChangesAsync();
		}
		
		// ACT: make sure Alice follows Bob and Charlie
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
			var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

			var alice = db.Authors.First(a => a.Name == "alice");
			var bob = db.Authors.First(a => a.Name == "bob");
			var charlie = db.Authors.First(a => a.Name == "charlie");

			await authorRepo.CreateFollowingAsync(alice.AuthorId, bob.AuthorId);
			await authorRepo.CreateFollowingAsync(alice.AuthorId, charlie.AuthorId);
		}

		// ASSERT: verify Alice follows both
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

			var alice = db.Authors.First(a => a.Name == "alice");
			var bob = db.Authors.First(a => a.Name == "bob");
			var charlie = db.Authors.First(a => a.Name == "charlie");

			var followings = db.Followings.Where(f => f.FollowerId == alice.AuthorId).ToList();

			followings.Should().HaveCount(2);
			followings.Should().Contain(f => f.FollowedId == bob.AuthorId);
			followings.Should().Contain(f => f.FollowedId == charlie.AuthorId);
		}
	}

	[Fact]
	public async Task FollowingTimeline_IntegratesMultipleRepositoriesAndService()
	{
		// Clear database to ensure test isolation
		await ClearDatabaseAsync();

		// ARRANGE: Set up a scenario with multiple authors and following relationships
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
			var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

			// Create three authors
			var alice = new Author { Name = "alice", Email = "alice@test.com" };
			var bob = new Author { Name = "bob", Email = "bob@test.com" };
			var charlie = new Author { Name = "charlie", Email = "charlie@test.com" };
			db.Authors.AddRange(alice, bob, charlie);
			await db.SaveChangesAsync();

			// Alice follows Bob, but NOT Charlie
			await authorRepo.CreateFollowingAsync(alice.AuthorId, bob.AuthorId);

			// All three create cheeps at different times
			var aliceCheep = new Cheep
			{
				AuthorId = alice.AuthorId,
				Text = "Alice's cheep",
				TimeStamp = DateTime.UtcNow.AddMinutes(-2)
			};
			var bobCheep = new Cheep
			{
				AuthorId = bob.AuthorId,
				Text = "Bob's cheep",
				TimeStamp = DateTime.UtcNow.AddMinutes(-1)
			};
			var charlieCheep = new Cheep
			{
				AuthorId = charlie.AuthorId,
				Text = "Charlie's cheep",
				TimeStamp = DateTime.UtcNow
			};
			db.Cheeps.AddRange(aliceCheep, bobCheep, charlieCheep);
			await db.SaveChangesAsync();
		}

		// ACT: Use CheepService to get Alice's following timeline
		// This integrates AuthorRepository (to get followed IDs) + CheepRepository (to get cheeps) + CheepService
		using (var scope = _factory.Services.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<ICheepService>();

			var timeline = service.GetCheepsFromFollowing("alice");

			// ASSERT: Alice should ONLY see Bob's cheeps (because she follows him)
			// GetCheepsFromFollowing only returns followed authors' cheeps, NOT your own
			timeline.Should().NotBeNull();
			timeline.Should().HaveCount(1);
			timeline.Should().Contain(c => c.Cheep == "Bob's cheep");
			timeline.Should().NotContain(c => c.Cheep == "Alice's cheep"); // Own cheeps NOT included
			timeline.Should().NotContain(c => c.Cheep == "Charlie's cheep"); // Not following Charlie

			// Verify the Author property and isFollowed flag
			timeline.First().Author.Should().Be("bob");
			timeline.First().isFollowed.Should().BeTrue(); // Bob is followed by Alice
		}
	}

    [Fact]
    public async Task UserUnFollowFunction()
    {
        // Clear database to ensure test isolation
        await ClearDatabaseAsync();

        // ARRANGE: Create authors and set up following relationships
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
            var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

            // Create three authors
            var alice = new Author { Name = "alice", Email = "alice@test.com" };
            var bob = new Author { Name = "bob", Email = "bob@test.com" };
            var charlie = new Author { Name = "charlie", Email = "charlie@test.com" };
            db.Authors.AddRange(alice, bob, charlie);
            await db.SaveChangesAsync();

            // Alice follows both Bob and Charlie
            await authorRepo.CreateFollowingAsync(alice.AuthorId, bob.AuthorId);
            await authorRepo.CreateFollowingAsync(alice.AuthorId, charlie.AuthorId);

            // Create cheeps for both Bob and Charlie
            var bobCheep = new Cheep { AuthorId = bob.AuthorId, Text = "Bob's cheep", TimeStamp = DateTime.UtcNow };
            var charlieCheep = new Cheep { AuthorId = charlie.AuthorId, Text = "Charlie's cheep", TimeStamp = DateTime.UtcNow };
            db.Cheeps.AddRange(bobCheep, charlieCheep);
            await db.SaveChangesAsync();
        }

        // ACT: Alice unfollows Charlie
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
            var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

            var alice = db.Authors.First(a => a.Name == "alice");
            var charlie = db.Authors.First(a => a.Name == "charlie");

            await authorRepo.DeleteFollowingAsync(alice.AuthorId, charlie.AuthorId);
        }

        // ASSERT: Verify Alice still follows Bob but no longer follows Charlie
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

            var alice = db.Authors.First(a => a.Name == "alice");
            var bob = db.Authors.First(a => a.Name == "bob");
            var charlie = db.Authors.First(a => a.Name == "charlie");

            var followings = db.Followings.Where(f => f.FollowerId == alice.AuthorId).ToList();

            // Alice should only follow Bob now
            followings.Should().HaveCount(1);
            followings.Should().Contain(f => f.FollowedId == bob.AuthorId);
            followings.Should().NotContain(f => f.FollowedId == charlie.AuthorId);
        }
    }

	[Fact]
	public async Task CheepService_CreateAndRead_IntegratesRepositoryAndDatabase()
	{
		// Clear database to ensure test isolation
		await ClearDatabaseAsync();

		// ARRANGE: Create authors directly in the database
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

			var alice = new Author { Name = "alice", Email = "alice@test.com" };
			var bob = new Author { Name = "bob", Email = "bob@test.com" };
			db.Authors.AddRange(alice, bob);
			await db.SaveChangesAsync();
		}

		// ACT: Use CheepService to create cheeps
		using (var scope = _factory.Services.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<ICheepService>();

			await service.CreateCheepAsync("alice", "Alice's first cheep");
			await service.CreateCheepAsync("bob", "Bob's first cheep");
			await service.CreateCheepAsync("alice", "Alice's second cheep");
		}

		// ASSERT: Read cheeps through service and verify integration
		using (var scope = _factory.Services.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<ICheepService>();

			// Test GetCheeps (public timeline)
			var allCheeps = service.GetCheeps(page: 0, pageSize: 32);
			allCheeps.Should().NotBeNull();
			allCheeps.Should().HaveCount(3);
			allCheeps.Should().Contain(c => c.Author == "alice" && c.Cheep == "Alice's first cheep");
			allCheeps.Should().Contain(c => c.Author == "bob" && c.Cheep == "Bob's first cheep");
			allCheeps.Should().Contain(c => c.Author == "alice" && c.Cheep == "Alice's second cheep");

			// Test GetCheepsFromAuthor (author timeline)
			var aliceCheeps = service.GetCheepsFromAuthor("alice", page: 0, pageSize: 32);
			aliceCheeps.Should().NotBeNull();
			aliceCheeps.Should().HaveCount(2);
			aliceCheeps.Should().OnlyContain(c => c.Author == "alice");
			aliceCheeps.Should().Contain(c => c.Cheep == "Alice's first cheep");
			aliceCheeps.Should().Contain(c => c.Cheep == "Alice's second cheep");

			var bobCheeps = service.GetCheepsFromAuthor("bob", page: 0, pageSize: 32);
			bobCheeps.Should().NotBeNull();
			bobCheeps.Should().HaveCount(1);
			bobCheeps.Should().OnlyContain(c => c.Author == "bob");
			bobCheeps.First().Cheep.Should().Be("Bob's first cheep");
		}
	}

	[Fact]
	public async Task Pagination_LimitsPageSizeTo32Cheeps()
	{
		// Clear database to ensure test isolation
		await ClearDatabaseAsync();

		// ARRANGE: Create an author and many cheeps (more than 32)
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
			var service = scope.ServiceProvider.GetRequiredService<ICheepService>();

			var alice = new Author { Name = "alice", Email = "alice@test.com" };
			db.Authors.Add(alice);
			await db.SaveChangesAsync();

			// Create 50 cheeps (more than one page of 32)
			for (int i = 1; i <= 50; i++)
			{
				await service.CreateCheepAsync("alice", $"Cheep number {i}");
				// Small delay to ensure different timestamps
				await Task.Delay(1);
			}
		}

		// ACT & ASSERT: Test pagination with default page size of 32
		using (var scope = _factory.Services.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<ICheepService>();

			// Page 0 should have exactly 32 cheeps (the page limit)
			var page0 = service.GetCheeps(page: 0);
			page0.Should().NotBeNull();
			page0.Should().HaveCount(32);
			page0.Should().OnlyContain(c => c.Author == "alice");

			// Page 1 should have the remaining 18 cheeps
			var page1 = service.GetCheeps(page: 1);
			page1.Should().NotBeNull();
			page1.Should().HaveCount(18);
			page1.Should().OnlyContain(c => c.Author == "alice");

			// Verify pages don't overlap
			var page0Ids = page0.Select(c => c.Id).ToList();
			var page1Ids = page1.Select(c => c.Id).ToList();
			page0Ids.Should().NotIntersectWith(page1Ids);

			// Verify pagination with author timeline also respects 32 limit
			var alicePage0 = service.GetCheepsFromAuthor("alice", page: 0);
			alicePage0.Should().HaveCount(32);
			alicePage0.Should().OnlyContain(c => c.Author == "alice");
		}
	}

	[Fact]
	public async Task CascadeDelete_RemovesAuthorAndRelatedDataCorrectly()
	{
		// Clear database to ensure test isolation
		await ClearDatabaseAsync();

		int aliceId, bobId, charlieId;

		// ARRANGE: Create authors with cheeps and following relationships
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
			var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();

			// Create three authors
			var alice = new Author { Name = "alice", Email = "alice@test.com" };
			var bob = new Author { Name = "bob", Email = "bob@test.com" };
			var charlie = new Author { Name = "charlie", Email = "charlie@test.com" };
			db.Authors.AddRange(alice, bob, charlie);
			await db.SaveChangesAsync();

			aliceId = alice.AuthorId;
			bobId = bob.AuthorId;
			charlieId = charlie.AuthorId;

			// Create following relationships: Alice follows Bob, Bob follows Charlie
			await authorRepo.CreateFollowingAsync(aliceId, bobId);
			await authorRepo.CreateFollowingAsync(bobId, charlieId);

			// Each author creates cheeps
			var aliceCheep = new Cheep
			{
				AuthorId = aliceId,
				Text = "Alice's cheep",
				TimeStamp = DateTime.UtcNow
			};
			var bobCheep = new Cheep
			{
				AuthorId = bobId,
				Text = "Bob's cheep",
				TimeStamp = DateTime.UtcNow
			};
			var charlieCheep = new Cheep
			{
				AuthorId = charlieId,
				Text = "Charlie's cheep",
				TimeStamp = DateTime.UtcNow
			};
			db.Cheeps.AddRange(aliceCheep, bobCheep, charlieCheep);
			await db.SaveChangesAsync();
		}

		// ACT: Delete Bob (who has cheeps, follows Charlie, and is followed by Alice)
		using (var scope = _factory.Services.CreateScope())
		{
			var authorRepo = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
			await authorRepo.DeleteAuthorAsync(bobId);
		}

		// ASSERT: Verify cascade delete worked correctly
		using (var scope = _factory.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();

			// Bob should be deleted
			var bobExists = await db.Authors.AnyAsync(a => a.AuthorId == bobId);
			bobExists.Should().BeFalse();

			// Bob's cheeps should be deleted
			var bobCheepsExist = await db.Cheeps.AnyAsync(c => c.AuthorId == bobId);
			bobCheepsExist.Should().BeFalse();

			// Bob's following relationships should be deleted (Alice -> Bob and Bob -> Charlie)
			var aliceToBobFollowing = await db.Followings
				.AnyAsync(f => f.FollowerId == aliceId && f.FollowedId == bobId);
			aliceToBobFollowing.Should().BeFalse();

			var bobToCharlieFollowing = await db.Followings
				.AnyAsync(f => f.FollowerId == bobId && f.FollowedId == charlieId);
			bobToCharlieFollowing.Should().BeFalse();

			// Alice and Charlie should still exist with their cheeps
			var aliceExists = await db.Authors.AnyAsync(a => a.AuthorId == aliceId);
			aliceExists.Should().BeTrue();

			var charlieExists = await db.Authors.AnyAsync(a => a.AuthorId == charlieId);
			charlieExists.Should().BeTrue();

			var aliceCheepExists = await db.Cheeps.AnyAsync(c => c.AuthorId == aliceId);
			aliceCheepExists.Should().BeTrue();

			var charlieCheepExists = await db.Cheeps.AnyAsync(c => c.AuthorId == charlieId);
			charlieCheepExists.Should().BeTrue();

			// Total count should be 2 authors and 2 cheeps
			var authorCount = await db.Authors.CountAsync();
			authorCount.Should().Be(2);

			var cheepCount = await db.Cheeps.CountAsync();
			cheepCount.Should().Be(2);
		}
	}
}
