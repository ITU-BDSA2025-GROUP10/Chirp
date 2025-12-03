using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Chirp;
using Chirp.Core.Models;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace IntegrationTests;

    public class CheepTest : IClassFixture<Factory<Program>>
    {
        private readonly Factory<Program> _factory;

        public CheepTest(Factory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateCheepAsync_CreatesAuthorAndCheep()
        {
            // Arrange: get real DbContext + service from the test server
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            var db = services.GetRequiredService<ChatDBContext>();
            var cheepService = services.GetRequiredService<ICheepService>();

            const string authorName = "IntegrationTestUser";
            const string text = "Hello from CreateCheepAsync test";

            // Act
            await cheepService.CreateCheepAsync(authorName, text);

            // Assert: check DB state
            var cheep = await db.Cheeps
                .Include(c => c.Author)
                .SingleOrDefaultAsync(c => c.Text == text);

            Assert.NotNull(cheep);
            Assert.Equal(authorName, cheep!.Author.Name);
        }
    }


