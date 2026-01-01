using Microsoft.Playwright;
using Xunit;

public class LoginPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task Login_Page_Loads()
    {
        await Page.GotoAsync($"{BaseUrl}/Account/Login");
        await Page.WaitForSelectorAsync("form");
        
        //assert that the loginbutton  is rendered
        var loginButtonText = await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).TextContentAsync();
        Assert.Contains("Log in", loginButtonText, StringComparison.OrdinalIgnoreCase);

    }
}
