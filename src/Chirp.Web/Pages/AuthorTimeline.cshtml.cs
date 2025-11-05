using Chirp.Infrastructure;
using Chirp.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class AuthorTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; }
    
    
    [BindProperty] public string Text {get ; set;} = string.Empty;


    public async Task<IActionResult> OnPost()
    {
        // Check if user is logged in
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage();
        }

        // Check if text is not empty
        if (string.IsNullOrWhiteSpace(Text))
        {
            return RedirectToPage();
        }

        // Create the cheep
        await _service.CreateCheepAsync(User.Identity.Name!, Text);

        // Redirect back to the page (so the new cheep shows up)
        return RedirectToPage();
    }
    
    public AuthorTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        Cheeps = _service.GetCheepsFromAuthor(author);
        return Page();
    }
}
