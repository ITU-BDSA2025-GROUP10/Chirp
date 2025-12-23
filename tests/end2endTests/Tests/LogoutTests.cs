using Xunit;

public class LogoutTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Logout()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        await Page.ClickAsync("button:has-text('logout')");

        // Wait for unauthenticated navigation
        await Page.WaitForSelectorAsync("text=login");

        Assert.Contains("login", await Page.ContentAsync());
    }
}
