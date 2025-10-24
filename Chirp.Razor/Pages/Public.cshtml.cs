using Chirp.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Chirp.Razor.Models;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IChatService _service;
    private readonly IConfiguration _configuration;
    public List<CheepViewModel> Cheeps { get; set; }

    // paging metadata exposed to the view
    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public bool HasPreviousPage { get; private set; }
    public bool HasNextPage { get; private set; }

    public PublicModel(IChatService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    // Use pageIndex (1-based) to fetch pages. The ChatService expects a zero-based page index.
    public ActionResult OnGet(int? pageIndex)
    {
        var page = pageIndex ?? 1;
        PageIndex = page;

        // Read page size from configuration with a sensible default
        PageSize = _configuration?.GetValue<int>("PageSize", 32) ?? 32;

        // Request one extra item so we can detect whether a next page exists.
        var items = _service.GetCheeps(page - 1, PageSize + 1);

        // If we received more than pageSize items, there is a next page.
        HasNextPage = items.Count > PageSize;
        HasPreviousPage = page > 1;

        // Keep only the requested pageSize items for display.
        Cheeps = items.Take(PageSize).ToList();

        return Page();
    }
}
