using Microsoft.Playwright;

public static class LoginHelper
{
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
        
        // Click the login button - NO SPACES in the selector!
        await page. ClickAsync("button[type='submit']");
        
        // Wait for successful login
        await page.WaitForSelectorAsync("text=my timeline");
    }
}
