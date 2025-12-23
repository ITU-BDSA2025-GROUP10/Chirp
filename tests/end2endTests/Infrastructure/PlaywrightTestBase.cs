using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright = null!;
    protected IBrowser Browser = null!;
    protected IBrowserContext Context = null!;
    protected IPage Page = null!;
    protected string BaseUrl = null!;
    protected string TestEmail = null!;
    protected string TestPassword = null!;

    public async Task InitializeAsync()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        BaseUrl = config["BaseUrl"]!;
        TestEmail = config["TestUser:Email"]!;
        TestPassword = config["TestUser:Password"]!;

        // Create test user via HTTP endpoint before starting browser tests
        using var httpClient = new HttpClient();
        try
        {
            await httpClient.PostAsync($"{BaseUrl}/test/create-user", null);
        }
        catch
        {
            // Ignore errors - endpoint might not exist or app might not be running yet
            // The test will fail later with a clearer message if the app isn't running
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true });

        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}
