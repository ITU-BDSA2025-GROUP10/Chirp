using Chirp.Infrastructure.Service;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class FollowingTimelineModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorRepository _authorRepository;
    private const int PageSize = 32;

    // Use the view model, not CheepDTO
    public List<CheepViewModel> Cheeps { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; }

    public FollowingTimelineModel(ICheepService cheepService, IAuthorRepository authorRepository)
    {
        _cheepService = cheepService;
        _authorRepository = authorRepository;
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
    
    public async Task<IActionResult> OnPostFollowAsync(string author, int? pageIndex)
    {
        var followerName = User.Identity!.Name!;
        var followerId = await _authorRepository.getAuthorByNameAsync(followerName);
        var followedId = await _authorRepository.getAuthorByNameAsync(author);

        await _authorRepository.CreateFollowingAsync(followerId, followedId);

        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string author, int? pageIndex)
    {
        var followerName = User.Identity!.Name!;
        var followerId = await _authorRepository.getAuthorByNameAsync(followerName);
        var followedId = await _authorRepository.getAuthorByNameAsync(author);

        await _authorRepository.DeleteFollowingAsync(followerId, followedId);

        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }
}
