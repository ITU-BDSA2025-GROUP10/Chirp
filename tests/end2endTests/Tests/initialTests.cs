using Xunit;

public class initialTests : PlaywrightTestBase
{
    [Fact]
    public async Task App_Is_Reachable()
    {
        await Page.GotoAsync(BaseUrl);

        var title = await Page.TitleAsync();
        Assert.False(string.IsNullOrEmpty(title));
    }
}
