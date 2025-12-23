using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure.Service;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IConfiguration _configuration;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICommentRepository _commentRepository;
    public List<CheepViewModel> Cheeps { get; set; }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public bool HasPreviousPage { get; private set; }
    public bool HasNextPage { get; private set; }
    [BindProperty] [Required] [StringLength(160)] public string Text {get ; set;} = string.Empty;


    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        if (!User.Identity?.IsAuthenticated ?? true)
        {
           // Console.WriteLine("User not authenticated");
            return RedirectToPage();
        }
        
        // Create the cheep
        await _service.CreateCheepAsync(User.Identity.Name!, Text);

        // Redirect back to the page (so the new cheep shows up)
        return RedirectToPage();
        
        
        
    }
    
    public PublicModel(ICheepService service, IConfiguration configuration, IAuthorRepository authorRepository, ICommentRepository commentRepository)
    {
        _service = service;
        _configuration = configuration;
        _authorRepository = authorRepository;
        _commentRepository = commentRepository;
    }
    
    public async Task<IActionResult> OnPostFollowAsync(string author, int? pageIndex)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage();

        var email = User.Identity!.Name!;
        int followerId;
        try
        {
            followerId = await _authorRepository.getAuthorByEmailAsync(email);
        }
        catch (KeyNotFoundException)
        {
            followerId = await _authorRepository.createAuthorAsync(email, email);
        }

        // follow logic here:

        var followedId = await _authorRepository.getAuthorByNameAsync(author);
        await _authorRepository.CreateFollowingAsync(followerId, followedId);

        // Redirect so OnGet runs and fills Cheeps
        return RedirectToPage(new { pageIndex = pageIndex ?? 1 });
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string author, int? pageIndex)
    {
        var email = User.Identity!.Name!;
        var followerId = await _authorRepository.getAuthorByEmailAsync(email);
        var followedId = await _authorRepository.getAuthorByNameAsync(author);

        await _authorRepository.DeleteFollowingAsync(followerId, followedId);

        return RedirectToPage(new { pageIndex = pageIndex ?? 0 });
    }

    public async Task<IActionResult> OnPostCommentAsync(int cheepId, string commentText, int? pageIndex)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage();

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

    public ActionResult OnGet(int? pageIndex)
    {
        var page = pageIndex ?? 1;
        PageIndex = page;

        PageSize = _configuration?.GetValue<int>("PageSize", 32) ?? 32;

        var currentUser = User.Identity?.Name;
        var items = _service.GetCheeps(currentUser, - 1, PageSize + 1);

        HasNextPage = items.Count > PageSize;
        HasPreviousPage = page > 1;

        Cheeps = items.Take(PageSize).ToList();

        return Page();
    }
}
