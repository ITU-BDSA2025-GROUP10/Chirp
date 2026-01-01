using Microsoft.Playwright;
using Xunit;

public class RegisterPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task Register_Page_Loads()
    {
        await Page. GotoAsync($"{BaseUrl}/Account/Register");
        await Page.WaitForSelectorAsync("form");

        var heading = await Page. Locator("h2").TextContentAsync();
        Assert.Contains("Create account", heading, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_Page_Has_Login_Link()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Register");
        
        var loginLink = await Page.Locator("a:has-text('Log in')").IsVisibleAsync();
        Assert.True(loginLink);
    }

    [Fact]
    public async Task User_Can_Register_New_Account()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Register");

        var uniqueUsername = $"testuser{DateTime.Now.Ticks}";
        var uniqueEmail = $"testuser{DateTime.Now.Ticks}@test.com";
        var password = "TestPassword123! ";

        await Page.FillAsync("input[name='Input.UserName']", uniqueUsername);
        await Page.FillAsync("input[name='Input.Email']", uniqueEmail);
        await Page.FillAsync("input[name='Input.Password']", password);
        await Page.FillAsync("input[name='Input.ConfirmPassword']", password);

        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        await Page.WaitForSelectorAsync("text=my timeline", new() { Timeout = 10000 });
        Assert.Contains("my timeline", await Page.ContentAsync());
    }
}
