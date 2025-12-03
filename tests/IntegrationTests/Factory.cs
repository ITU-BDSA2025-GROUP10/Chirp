using System.Data.Common;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

   public class Factory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remove existing DbContext registration
            // 👉 This is the bit I’d change:
            // You're removing IDbContextOptionsConfiguration<ChatDBContext>,
            // but normally the app has registered DbContextOptions<ChatDBContext>.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChatDBContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            // 2. Remove any existing DbConnection registration (optional but fine)
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor is not null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // 3. Register a *single* open SQLite connection
            services.AddSingleton<DbConnection>(_ =>
            {
                // If you want a pure in-memory DB for tests, use:
                // "DataSource=:memory:;Cache=Shared"
                var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
                connection.Open();
                return connection;
            });

            // 4. Register ChatDBContext to use that SQLite connection
            services.AddDbContext<ChatDBContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            // 5. Build provider and seed the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ChatDBContext>();

            // ⚠️ Recommended for tests: reset DB state
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // You can keep using your existing DbInitializer for now
            DbInitializer.SeedDatabase(db);
        });

        builder.UseEnvironment("Development"); // or "Testing" if you prefer
    }
}
