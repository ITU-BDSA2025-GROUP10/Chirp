using Chirp.Infrastructure.Service;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class FollowingTimelineModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICommentRepository _commentRepository;
    private const int PageSize = 32;

    public List<CheepViewModel> Cheeps { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; }

    public FollowingTimelineModel(ICheepService cheepService, IAuthorRepository authorRepository, ICommentRepository commentRepository)
    {
        _cheepService = cheepService;
        _authorRepository = authorRepository;
        _commentRepository = commentRepository;
    }
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
        var followerEmail = User.Identity!.Name!;
        var followerId = await _authorRepository.getAuthorByEmailAsync(followerEmail);
        var followedId = await _authorRepository.getAuthorByNameAsync(author);

        await _authorRepository.CreateFollowingAsync(followerId, followedId);

        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string author, int? pageIndex)
    {
        var followerEmail = User.Identity!.Name!;
        var followerId = await _authorRepository.getAuthorByEmailAsync(followerEmail);
        var followedId = await _authorRepository.getAuthorByNameAsync(author);

        await _authorRepository.DeleteFollowingAsync(followerId, followedId);

        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }

    public async Task<IActionResult> OnPostCommentAsync(int cheepId, string commentText, int? pageIndex)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage(new { pageIndex = pageIndex ?? 1 });

        if (string.IsNullOrWhiteSpace(commentText))
            return RedirectToPage(new { pageIndex = pageIndex ?? 1 });

        var authorName = User.Identity.Name!;
        await _commentRepository.CreateCommentAsync(new Core.Models.CommentDTO
        {
            Text = commentText,
            AuthorName = authorName,
            CheepId = cheepId,
            TimeStamp = DateTime.UtcNow
        });

        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }

    public async Task<IActionResult> OnGetCommentsAsync(int cheepId)
    {
        var comments = await _commentRepository.GetCommentsByCheepIdAsync(cheepId);
        return new JsonResult(comments);
    }
}
