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
        
        await page.ClickAsync("button:has-text('Log in')");
        
        await page.WaitForSelectorAsync("text=my timeline");
    }
}
