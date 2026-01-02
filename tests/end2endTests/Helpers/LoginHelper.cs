using Microsoft.Playwright;

public static class LoginHelper
{
    /// <summary>
    /// Registers and logs in a new user. Returns the username.
    /// </summary>
    public static async Task<string> RegisterAndLoginAsync(
        IPage page,
        string baseUrl,
        string? username = null,
        string? email = null,
        string? password = null)
    {
        // Generate unique credentials if not provided
        username ??= $"testuser{DateTime.Now.Ticks}";
        email ??= $"testuser{DateTime.Now.Ticks}@test.com";
        password ??= "TestPassword123!";

        // Navigate to register page
        await page.GotoAsync($"{baseUrl}/Account/Register");
        await page.WaitForSelectorAsync("form");

        // Fill in registration form
        await page.FillAsync("input[name='Input.UserName']", username);
        await page.FillAsync("input[name='Input.Email']", email);
        await page.FillAsync("input[name='Input.Password']", password);
        await page.FillAsync("input[name='Input.ConfirmPassword']", password);

        // Submit the form
        await page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        // Wait for successful registration and auto-login
        await page.WaitForSelectorAsync("text=my timeline", new() { Timeout = 10000 });

        return username;
    }

    /// <summary>
    /// Logs in with an existing user.
    /// </summary>
    public static async Task LoginAsync(
        IPage page,
        string baseUrl,
        string username,
        string password)
    {
        await page.GotoAsync($"{baseUrl}/Account/Login");

        // Wait for login form
        await page.WaitForSelectorAsync("text=Log in");

        // Fill credentials
        await page.FillAsync("input[name='Input.UserName']", username);
        await page.FillAsync("input[name='Input.Password']", password);

        // Submit the form by pressing Enter on the password field
        await page.Locator("input[name='Input.Password']").PressAsync("Enter");

        // Wait for navigation to complete
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for the "my timeline" link to appear (indicates successful login)
        await page.WaitForSelectorAsync("text=my timeline", new() { Timeout = 10000 });
    }
}
