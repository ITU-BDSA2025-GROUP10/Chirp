using Chirp.Infrastructure;
using Chirp.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IChatService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public UserTimelineModel(IChatService service)
    {
        _service = service;
    }

    public ActionResult OnGet(string author)
    {
        Cheeps = _service.GetCheepsFromAuthor(author);
        return Page();
    }
}
