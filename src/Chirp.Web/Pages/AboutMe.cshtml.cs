using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;
using Chirp.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
 

  namespace Chirp.Web.Pages
  {
      [Authorize]
      public class AboutMeModel : PageModel
      {
          private readonly UserManager<ApplicationAuthor> _userManager;
          private readonly SignInManager<ApplicationAuthor> _signInManager;
          private readonly IAuthorRepository _authorRepository;
          private readonly ICheepRepository _cheepRepository;
          private readonly ILogger<AboutMeModel> _logger;

          public AboutMeModel(
              UserManager<ApplicationAuthor> userManager,
              SignInManager<ApplicationAuthor> signInManager,
              IAuthorRepository authorRepository,
              ICheepRepository cheepRepository,
              ILogger<AboutMeModel> logger)
          {
              _userManager = userManager;
              _signInManager = signInManager;
              _authorRepository = authorRepository;
              _cheepRepository = cheepRepository;
              _logger = logger;
          }

          public string UserName { get; set; }
          public string Email { get; set; }
          public List<Author> Following { get; set; }
          public List<CheepDTO> UserCheeps { get; set; }
          public bool RequirePassword { get; set; }

          [BindProperty]
          public InputModel Input { get; set; }

          public class InputModel
          {
              [Required]
              [DataType(DataType.Password)]
              public string Password { get; set; }
          }

          public async Task<IActionResult> OnGetAsync()
          {
              var user = await _userManager.GetUserAsync(User);
              if (user == null)
              {
                  return NotFound();
              }

              await LoadUserDataAsync(user);
              return Page();
          }

          public async Task<IActionResult> OnPostDeleteAsync()
          {
              var user = await _userManager.GetUserAsync(User);
              if (user == null)
              {
                  return NotFound();
              }

              RequirePassword = await _userManager.HasPasswordAsync(user);
              if (RequirePassword)
              {
                  if (Input == null || string.IsNullOrEmpty(Input.Password) ||
                      !await _userManager.CheckPasswordAsync(user, Input.Password))
                  {
                      await LoadUserDataAsync(user);
                      ModelState.AddModelError(string.Empty, "Incorrect password.");
                      return Page();
                  }
              }

              try
              {
                  await _authorRepository.DeleteAuthorByEmailAsync(user.Email);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error deleting author data");
                  throw;
              }

              var result = await _userManager.DeleteAsync(user);
              if (!result.Succeeded)
              {
                  throw new InvalidOperationException("Unexpected error occurred deleting user.");
              }

              await _signInManager.SignOutAsync();
              _logger.LogInformation("User deleted themselves.");

              return Redirect("~/");
          }

          private async Task LoadUserDataAsync(ApplicationAuthor user)
          {
              UserName = user.UserName;
              Email = user.Email;
              RequirePassword = await _userManager.HasPasswordAsync(user);

              try
              {
                  var authorId = await _authorRepository.getAuthorByEmailAsync(Email);
                  var author = await _authorRepository.GetAuthorWithFollowingAsync(authorId);
                  Following = author?.Following?.Select(f => f.Followed).ToList() ?? new List<Author>();

                  UserCheeps = await _cheepRepository.ReadCheepsAsync(author: UserName, page: 0, pageSize:
                      int.MaxValue);
              }
              catch
              {
                  Following = new List<Author>();
                  UserCheeps = new List<CheepDTO>();
              }
          }
      }
  }
