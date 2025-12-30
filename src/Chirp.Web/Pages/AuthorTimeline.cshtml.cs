using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Service;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class AuthorTimelineModel : PageModel
{
    private readonly ICheepService _service;
    private readonly ICommentRepository _commentRepository;
    public List<CheepViewModel> Cheeps { get; set; }


    [BindProperty] [Required] [StringLength(160)] public string Text {get ; set;} = string.Empty;


    public async Task<IActionResult> OnPost()
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage();
        }


        await _service.CreateCheepAsync(User.Identity.Name!, Text);

        return RedirectToPage();
    }

    public AuthorTimelineModel(ICheepService service, ICommentRepository commentRepository)
    {
        _service = service;
        _commentRepository = commentRepository;
    }

    public ActionResult OnGet(string author)
    {
        Cheeps = _service.GetCheepsFromAuthor(author);
        return Page();
    }

    public async Task<IActionResult> OnPostCommentAsync(int cheepId, string commentText, string author)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToPage(new { author });

        if (string.IsNullOrWhiteSpace(commentText))
            return RedirectToPage(new { author });

        var authorName = User.Identity.Name!;
        await _commentRepository.CreateCommentAsync(new Core.Models.CommentDTO
        {
            Text = commentText,
            AuthorName = authorName,
            CheepId = cheepId,
            TimeStamp = DateTime.UtcNow
        });

        return RedirectToPage(new { author });
    }

    public async Task<IActionResult> OnGetCommentsAsync(int cheepId)
    {
        var comments = await _commentRepository.GetCommentsByCheepIdAsync(cheepId);
        return new JsonResult(comments);
    }
}
