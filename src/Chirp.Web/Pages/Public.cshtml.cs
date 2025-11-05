using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IConfiguration _configuration;
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

    
    public PublicModel(ICheepService service, IConfiguration configuration)
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
