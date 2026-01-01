using System.Security.Claims;
using Chirp.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<Author> _signInManager;
    private readonly UserManager<Author> _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<Author> signInManager,
        UserManager<Author> userManager,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, props);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"External provider error: {remoteError}";
            return RedirectToPage("./Login", new { returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "External login failed (no info).";
            return RedirectToPage("./Login", new { returnUrl });
        }

        // Try sign in existing external login
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        // Create user on first GitHub login
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var githubName = info.Principal.FindFirstValue(ClaimTypes.Name) ??
                         info.Principal.FindFirstValue("urn:github:login") ??
                         info.Principal.Identity?.Name ??
                         "github-user";

        // If email is missing, you can either:
        // A) force user to add it (best), or
        // B) generate a dummy email (not recommended)
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "GitHub did not provide an email. Please create a normal account, or make your email public on GitHub.";
            return RedirectToPage("./Register", new { returnUrl });
        }

        // Ensure unique username
        var baseUserName = githubName.Replace(" ", "").ToLowerInvariant();
        var userName = baseUserName;
        var i = 0;
        while (await _userManager.FindByNameAsync(userName) != null)
        {
            i++;
            userName = $"{baseUserName}{i}";
        }

        var user = new Author
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" | ", createResult.Errors.Select(e => e.Description));
            return RedirectToPage("./Login", new { returnUrl });
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" | ", addLoginResult.Errors.Select(e => e.Description));
            return RedirectToPage("./Login", new { returnUrl });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}
