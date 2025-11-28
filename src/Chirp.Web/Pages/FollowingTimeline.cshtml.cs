using Chirp.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class FollowingTimelineModel : PageModel
{
    private readonly ICheepService _cheepService;
    private const int PageSize = 32;

    // Use the view model, not CheepDTO
    public List<CheepViewModel> Cheeps { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; }

    public FollowingTimelineModel(ICheepService cheepService)
    {
        _cheepService = cheepService;
    }

    // Service API is synchronous, so this is synchronous too
    public void OnGet()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            Cheeps = new List<CheepViewModel>();
            return;
        }

        Cheeps = _cheepService.GetCheepsFromFollowing(
            userName,
            PageIndex,
            PageSize);
    }
}
