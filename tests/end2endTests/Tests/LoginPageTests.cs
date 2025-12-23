using Xunit;

public class LoginPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task Login_Page_Loads()
    {
        await Page.GotoAsync($"{BaseUrl}/Identity/Account/Login");

        await Page.WaitForSelectorAsync("form");

        var heading = await Page.Locator("h1").TextContentAsync();
        Assert.Contains("Log", heading, StringComparison.OrdinalIgnoreCase);
    }
}
