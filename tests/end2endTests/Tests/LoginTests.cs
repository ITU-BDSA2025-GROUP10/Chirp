using Microsoft.Playwright;
using Xunit;

public class LoginTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Login_With_Valid_Credentials()
    {
        // First, register a new user
        var username = $"testuser{DateTime.Now.Ticks}";
        var email = $"testuser{DateTime.Now.Ticks}@test.com";
        var password = "TestPassword123!";

        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl, username, email, password);

        // Now logout
        await Page.ClickAsync("button:has-text('logout')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify we're logged out
        var loggedOut = await Page.Locator("a:has-text('login')").IsVisibleAsync();
        Assert.True(loggedOut);

        // Now login with the same credentials
        await LoginHelper.LoginAsync(Page, BaseUrl, username, password);

        // Verify we're logged in by checking for the presence of "my timeline"
        var content = await Page.ContentAsync();
        Assert.Contains("my timeline", content);
    }

    [Fact]
    public async Task User_Cannot_Login_With_Invalid_Credentials()
    {
        // Navigate to login page
        await Page.GotoAsync($"{BaseUrl}/Account/Login");

        // Wait for login form to load
        await Page.WaitForSelectorAsync("text=Log in");

        // Fill in invalid credentials
        await Page.FillAsync("input[name='Input.UserName']", "invalid@example.com");
        await Page.FillAsync("input[name='Input.Password']", "WrongPassword123!");

        // Submit the form - use specific class to avoid GitHub OAuth button
        await Page.ClickAsync("button.auth-primary");

        // Wait a bit for the response
        await Task.Delay(2000);

        // Should still be on login page or see an error
        var url = Page.Url;
        Assert.Contains("Login", url);
    }

    [Fact]
    public async Task Login_Page_Has_Required_Elements()
    {
        // Navigate to login page
        await Page.GotoAsync($"{BaseUrl}/Account/Login");

        // Wait for login form to load
        await Page.WaitForSelectorAsync("form");

        // Check for username input
        var usernameInput = await Page.Locator("input[name='Input.UserName']").IsVisibleAsync();
        Assert.True(usernameInput);

        // Check for password input
        var passwordInput = await Page.Locator("input[name='Input.Password']").IsVisibleAsync();
        Assert.True(passwordInput);

        // Check for submit button (specifically the login button, not the GitHub OAuth button)
        var submitButton = await Page.Locator("button.auth-primary").IsVisibleAsync();
        Assert.True(submitButton);
    }

    [Fact]
    public async Task User_Can_Navigate_To_Register_From_Login()
    {
        // Navigate to login page
        await Page.GotoAsync($"{BaseUrl}/Account/Login");

        // Wait for login form to load
        await Page.WaitForSelectorAsync("text=Log in");

        // Click the register link
        await Page.ClickAsync("text=Create one");

        // Should navigate to register page
        await Page.WaitForSelectorAsync("text=Create account", new() { Timeout = 5000 });

        var url = Page.Url;
        Assert.Contains("Register", url);
    }
}
