using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Chirp.Core.Models;
using Chirp.Infrastructure;


namespace IntegrationTests;

public class CheepServiceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CheepServiceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Baseline test to see that integration test setup works
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
}
