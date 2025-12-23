using Xunit;

public class ChirpTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Create_Chirp()
    {
        await LoginHelper.LoginAsync(Page, BaseUrl, TestEmail, TestPassword);

        var chirpText = "E2E test chirp " + DateTime.Now.Ticks;

        await Page.FillAsync("input[name='Text']", chirpText);
        await Page.ClickAsync("input[type='submit'][value='Share']");
        // Verify chirp appears
        await Page.WaitForSelectorAsync($"text={chirpText}");
        Assert.Contains(chirpText, await Page.ContentAsync());
    }
}
