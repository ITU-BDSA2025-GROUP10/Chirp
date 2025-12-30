using System.ComponentModel.DataAnnotations;
using Chirp.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Author> _signInManager;
        private readonly UserManager<Author> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<Author> userManager,
            SignInManager<Author> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }
            
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        /*
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // ... eksisterende validering
            if (ModelState.IsValid)
            {
                var user = CreateUser();
        
                // RET DISSE LINJER:
                await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
        
                var result = await _userManager.CreateAsync(user, Input.Password);
                // ... resten af koden
            }
        }
        */
        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Check if an Author already exists with this email or username
                var existingAuthorByEmail = await _userManager.FindByEmailAsync(Input.Email);
                var existingAuthorByUserName = await _userManager.FindByNameAsync(Input.UserName);

                // Check if username or email is already taken
                if (existingAuthorByUserName != null && existingAuthorByEmail != null && existingAuthorByUserName.Id != existingAuthorByEmail.Id)
                {
                    ModelState.AddModelError(string.Empty, "Username or email already exists.");
                    return Page();
                }

                var existingAuthor = existingAuthorByEmail ?? existingAuthorByUserName;

                if (existingAuthor != null)
                {
                    // Author exists (like Helge) - check if they already have a password
                    var hasPassword = await _userManager.HasPasswordAsync(existingAuthor);

                    if (hasPassword)
                    {
                        // Author already has a password - they should login instead
                        ModelState.AddModelError(string.Empty, "An account with this username or email already exists. Please log in.");
                        return Page();
                    }
                    else
                    {
                        // Author exists but has no password (from seed data) - add password and update username
                        existingAuthor.UserName = Input.UserName;
                        existingAuthor.Email = Input.Email;
                        existingAuthor.EmailConfirmed = true;

                        var updateResult = await _userManager.UpdateAsync(existingAuthor);
                        if (!updateResult.Succeeded)
                        {
                            foreach (var error in updateResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return Page();
                        }

                        var addPasswordResult = await _userManager.AddPasswordAsync(existingAuthor, Input.Password);

                        if (addPasswordResult.Succeeded)
                        {
                            _logger.LogInformation("User with existing email created a password and username.");

                            // Sign them in
                            await _signInManager.SignInAsync(existingAuthor, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            foreach (var error in addPasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return Page();
                        }
                    }
                }
                else
                {
                    // No existing Author - create new one with separate username and email
                    var user = new Author
                    {
                        UserName = Input.UserName,
                        Email = Input.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return Page();
        }
    }
}
