using Microsoft.Playwright;

public static class LoginHelper
{
    public static async Task LoginAsync(
        IPage page,
        string baseUrl,
        string email,
        string password)
    {
        await page.GotoAsync($"{baseUrl}/Identity/Account/Login");

        // Wait for login form
        await page.WaitForSelectorAsync("text=Log in");

        // Fill credentials
        await page.FillAsync("input[name='Input.Email']", email);
        await page.FillAsync("input[name='Input.Password']", password);

        // Click the login button by TEXT (not type/id)
        await page.ClickAsync("button:has-text('Log in')");

        // 🔑 THIS is the key: wait for navigation menu change
        await page.WaitForSelectorAsync("text=my timeline");
    }
}
