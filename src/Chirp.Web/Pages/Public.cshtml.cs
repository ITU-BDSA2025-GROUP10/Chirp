using Chirp.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly IChatService _service;
    private readonly IConfiguration _configuration;
    public List<CheepViewModel> Cheeps { get; set; }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public bool HasPreviousPage { get; private set; }
    public bool HasNextPage { get; private set; }

    public PublicModel(IChatService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }


    public ActionResult OnGet(int? pageIndex)
    {
        var page = pageIndex ?? 1;
        PageIndex = page;

        PageSize = _configuration?.GetValue<int>("PageSize", 32) ?? 32;

        var items = _service.GetCheeps(page - 1, PageSize + 1);

        HasNextPage = items.Count > PageSize;
        HasPreviousPage = page > 1;

        Cheeps = items.Take(PageSize).ToList();

        return Page();
    }
}
