using Xunit;

public class LogoutTests :  PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Logout()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        
        await Page. ClickAsync("button:has-text('logout')");
        
        await Page.WaitForSelectorAsync("text=login");

        // Verify we're logged out
        Assert.Contains("login", await Page.ContentAsync());
        
        // Verify we don't see authenticated navigation
        var content = await Page.ContentAsync();
        Assert.DoesNotContain("my timeline", content);
    }
}
