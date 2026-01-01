using Xunit;

public class ChirpTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Create_Chirp()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        
        await Page.GotoAsync(BaseUrl);

        var chirpText = "E2E test chirp " + DateTime.Now. Ticks;
        await Page. FillAsync("input[name='Text']", chirpText);
        await Page.ClickAsync("input[type='submit'][value='Share']");
        
        
        await Page. WaitForLoadStateAsync(Microsoft. Playwright.LoadState.NetworkIdle);
        await Page.WaitForSelectorAsync($"text={chirpText}");
        Assert.Contains(chirpText, await Page.ContentAsync());
    }

    [Fact]
    public async Task Unauthenticated_User_Cannot_Create_Chirp()
    {
        await Page.GotoAsync(BaseUrl);

        // Check that the chirp form is not visible for unauthenticated users
        var chirpFormVisible = await Page.Locator("input[name='Text']").IsVisibleAsync();
        Assert.False(chirpFormVisible);
    }
}
