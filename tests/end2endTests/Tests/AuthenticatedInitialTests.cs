using Xunit;

public class AuthenticatedSmokeTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Login()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        // Assert authenticated navigation exists
        Assert.Contains("my timeline", await Page.ContentAsync());
        Assert.Contains("logout", await Page.ContentAsync());
    }
}
