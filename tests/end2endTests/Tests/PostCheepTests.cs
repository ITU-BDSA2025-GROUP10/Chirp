using Microsoft.Playwright;
using Xunit;

public class PostCheepTests : PlaywrightTestBase
{
    [Fact]
    public async Task User_Can_Post_Cheep_When_Logged_In()
    {
        // Register and login using the LoginHelper
        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl);

        // Navigate to the public timeline
        await Page.GotoAsync(BaseUrl);

        // Wait for the cheep input box to appear
        await Page.WaitForSelectorAsync("input[id='cheepText']");

        // Create a unique cheep message
        var cheepText = $"Test cheep at {DateTime.Now.Ticks}";

        // Fill in the cheep text
        await Page.FillAsync("input[id='cheepText']", cheepText);

        // Click the Share button
        await Page.ClickAsync("input[type='submit'][value='Share']");

        // Wait for the page to reload after posting
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the cheep appears on the timeline
        var content = await Page.ContentAsync();
        Assert.Contains(cheepText, content);
    }

    [Fact]
    public async Task User_Cannot_Post_Empty_Cheep()
    {
        // Register and login
        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl);

        // Navigate to the public timeline
        await Page.GotoAsync(BaseUrl);

        // Wait for the cheep input box to appear
        await Page.WaitForSelectorAsync("input[id='cheepText']");

        // Try to submit without filling in text
        await Page.ClickAsync("input[type='submit'][value='Share']");

        // The form should have validation preventing empty submission
        // Check if we're still on the same page with the form visible
        var isFormVisible = await Page.Locator("input[id='cheepText']").IsVisibleAsync();
        Assert.True(isFormVisible);
    }

    [Fact]
    public async Task User_Cannot_Post_Cheep_Exceeding_160_Characters()
    {
        // Register and login
        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl);

        // Navigate to the public timeline
        await Page.GotoAsync(BaseUrl);

        // Wait for the cheep input box to appear
        await Page.WaitForSelectorAsync("input[id='cheepText']");

        // Create a cheep that exceeds 160 characters
        var longCheepText = new string('a', 200);

        // Fill in the cheep text
        await Page.FillAsync("input[id='cheepText']", longCheepText);

        // Get the actual value in the input (should be truncated to 160 by maxlength attribute)
        var actualValue = await Page.InputValueAsync("input[id='cheepText']");

        // Verify the input was truncated to 160 characters
        Assert.True(actualValue.Length <= 160);
    }

    [Fact]
    public async Task Cheep_Form_Not_Visible_When_Not_Logged_In()
    {
        // Navigate to the public timeline without logging in
        await Page.GotoAsync(BaseUrl);

        // Wait for page to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The cheep input box should not be visible
        var isFormVisible = await Page.Locator("input[id='cheepText']").IsVisibleAsync();
        Assert.False(isFormVisible);
    }

    [Fact]
    public async Task User_Can_Post_Multiple_Cheeps()
    {
        // Register and login
        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl);

        // Navigate to the public timeline
        await Page.GotoAsync(BaseUrl);

        // Post first cheep
        var cheepText1 = $"First cheep at {DateTime.Now.Ticks}";
        await Page.WaitForSelectorAsync("input[id='cheepText']");
        await Page.FillAsync("input[id='cheepText']", cheepText1);
        await Page.ClickAsync("input[type='submit'][value='Share']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify first cheep appears
        var content1 = await Page.ContentAsync();
        Assert.Contains(cheepText1, content1);

        // Post second cheep
        await Task.Delay(100); // Small delay to ensure different timestamp
        var cheepText2 = $"Second cheep at {DateTime.Now.Ticks}";
        await Page.WaitForSelectorAsync("input[id='cheepText']");
        await Page.FillAsync("input[id='cheepText']", cheepText2);
        await Page.ClickAsync("input[type='submit'][value='Share']");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify both cheeps appear
        var content2 = await Page.ContentAsync();
        Assert.Contains(cheepText1, content2);
        Assert.Contains(cheepText2, content2);
    }

    [Fact]
    public async Task Character_Counter_Updates_As_User_Types()
    {
        // Register and login
        await LoginHelper.RegisterAndLoginAsync(Page, BaseUrl);

        // Navigate to the public timeline
        await Page.GotoAsync(BaseUrl);

        // Wait for the cheep input box and counter
        await Page.WaitForSelectorAsync("input[id='cheepText']");
        await Page.WaitForSelectorAsync("#charCount");

        // Initial counter should show 160
        var initialCount = await Page.Locator("#charCount").TextContentAsync();
        Assert.Equal("160", initialCount);

        // Type some text
        var testText = "Hello World";
        await Page.FillAsync("input[id='cheepText']", testText);

        // Wait a moment for the JavaScript to update
        await Task.Delay(500);

        // Counter should update to show remaining characters
        var updatedCount = await Page.Locator("#charCount").TextContentAsync();
        var expectedRemaining = 160 - testText.Length;
        Assert.Equal(expectedRemaining.ToString(), updatedCount);
    }
}
