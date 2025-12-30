using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            services.RemoveAll<DbContextOptions<ChatDBContext>>();
            services.RemoveAll<ChatDBContext>();

            // Open in-memory SQLite connection
            _connection.Open();

            // Add in-memory DbContext (same as unit tests!)
            services.AddDbContext<ChatDBContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Create database schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChatDBContext>();
            db.Database.EnsureCreated();

            // Disable antiforgery token validation for testing
            services.AddMvc(options =>
            {
                options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }
        base.Dispose(disposing);
    }
}
