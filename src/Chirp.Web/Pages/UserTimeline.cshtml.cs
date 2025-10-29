using Chirp.Infrastructure;
using Chirp.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class AuthorTimelineModel : PageModel
{
    private readonly IChatService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public AuthorTimelineModel(IChatService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        Cheeps = _service.GetCheepsFromAuthor(author);
        return Page();
    }
}
