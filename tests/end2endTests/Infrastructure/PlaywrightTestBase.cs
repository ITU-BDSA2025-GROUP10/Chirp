using Microsoft. Playwright;
using Microsoft.Extensions. Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore. Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Chirp.Core.Models;

public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright = null! ;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected string BaseUrl = null!;
    protected string TestEmail = null!;
    protected string TestPassword = null!;
    
    private WebApplicationFactory<Program>? _factory;
    private SqliteConnection? _connection;

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        BaseUrl = config["BaseUrl"]!;
        TestEmail = config["TestUser:Email"]!;
        TestPassword = config["TestUser:Password"]!;

       
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ChatDBContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    
                    services.AddDbContext<ChatDBContext>(options =>
                    {
                        options.UseSqlite(_connection);
                    });

                    
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices. GetRequiredService<ChatDBContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<Author>>();

                    
                    db.Database.EnsureCreated();

                  
                    var testUser = new Author
                    {
                        UserName = TestEmail,
                        Email = TestEmail,
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(testUser, TestPassword).Wait();
                });

                builder.UseUrls(BaseUrl);
                builder.UseEnvironment("Development");
            });

        
        _ = _factory.Server; 

        
        await Task.Delay(1000);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true });

        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (Context != null) await Context.CloseAsync();
        if (Browser != null) await Browser.CloseAsync();
        Playwright?.Dispose();
        
        _factory?.Dispose();
        
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
