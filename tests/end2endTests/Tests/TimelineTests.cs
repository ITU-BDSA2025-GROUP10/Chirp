using Xunit;

public class TimelineTests : PlaywrightTestBase
{
    [Fact]
    public async Task Timeline_Loads_After_Login()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        await Page.WaitForSelectorAsync("text=Public Timeline");
        
        Assert.Contains("Public Timeline", await Page.ContentAsync());
    }
}
