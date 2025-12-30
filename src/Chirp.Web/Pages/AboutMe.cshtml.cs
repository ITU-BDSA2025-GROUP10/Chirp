using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;
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
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;
        private readonly IAuthorRepository _authorRepository;
        private readonly ICheepRepository _cheepRepository;
        private readonly ILogger<AboutMeModel> _logger;

        public AboutMeModel(
            UserManager<Author> userManager,
            SignInManager<Author> signInManager,
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


        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<Author> Following { get; set; } = new();
        public List<CheepDTO> UserCheeps { get; set; } = new();
        public bool RequirePassword { get; set; }

        [BindProperty]
        public InputModel? Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
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
                if (Input == null ||
                    string.IsNullOrWhiteSpace(Input.Password) ||
                    !await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    await LoadUserDataAsync(user);
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogError("User email is null when attempting deletion.");
                return BadRequest();
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

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await LoadUserDataAsync(user);

            var zipBytes = GenerateDataArchive();

            var fileName = $"chirp-data-{UserName}-{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
            return File(zipBytes, "application/zip", fileName);
        }

        private byte[] GenerateDataArchive()
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var profileEntry = archive.CreateEntry("personal_info.csv");
                using (var entryStream = profileEntry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.WriteLine("Field,Value");
                    writer.WriteLine($"Username,\"{UserName}\"");
                    writer.WriteLine($"Email,\"{Email}\"");
                }

                var followingEntry = archive.CreateEntry("following.csv");
                using (var entryStream = followingEntry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.WriteLine("Username");
                    foreach (var followed in Following)
                    {
                        writer.WriteLine($"\"{followed.UserName}\"");
                    }
                }

                var cheepsEntry = archive.CreateEntry("cheeps.csv");
                using (var entryStream = cheepsEntry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.WriteLine("Timestamp,Text");
                    foreach (var cheep in UserCheeps)
                    {
                        writer.WriteLine($"\"{cheep.Timestamp}\",\"{cheep.Text?.Replace("\"", "\"\"")}\"");
                    }
                }
            }

            return memoryStream.ToArray();
        }

        private async Task LoadUserDataAsync(Author user)
        {
            UserName = user.UserName;
            Email = user.Email;
            RequirePassword = await _userManager.HasPasswordAsync(user);

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(UserName))
            {
                Following.Clear();
                UserCheeps.Clear();
                return;
            }

            try
            {
                var authorId = await _authorRepository.getAuthorByEmailAsync(Email);
                var author = await _authorRepository.GetAuthorWithFollowingAsync(authorId);

                Following = author?.Following?.Select(f => f.Followed).ToList()
                            ?? new List<Author>();

                UserCheeps = await _cheepRepository.ReadCheepsAsync(
                    author: UserName,
                    page: 0,
                    pageSize: int.MaxValue);
            }
            catch
            {
                Following.Clear();
                UserCheeps.Clear();
            }
        }
    }
}
