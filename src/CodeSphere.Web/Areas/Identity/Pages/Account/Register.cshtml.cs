using System.ComponentModel.DataAnnotations;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IUserService _userService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<int>> roleManager,
        IUserService userService,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = "/";

    public class InputModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // ---- UserProfiles fields, collected up front at sign-up ----

        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [Display(Name = "Website")]
        public string? WebsiteURL { get; set; }

        // Deliberately NOT decorated with [Url] alone — that attribute is too
        // permissive (it accepts things like "ftp://..." and doesn't guard
        // against "javascript:"/"data:" pseudo-schemes). The real check runs
        // server-side via ImageUrlValidator in OnPostAsync, which is the same
        // validator the API's registration endpoint uses, so both entry
        // points enforce an identical rule.
        [MaxLength(255)]
        [Display(Name = "Profile photo URL")]
        public string? ProfileImageURL { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
            return Page();

        if (!CodeSphere.Core.Common.ImageUrlValidator.IsValid(Input.ProfileImageURL, out var imageError))
        {
            ModelState.AddModelError(nameof(Input.ProfileImageURL), imageError!);
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            EmailConfirmed = true // RequireConfirmedAccount is false, so there's no verification step to wait on.
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} created a new account.", Input.Email);

            // Every registered account may write articles — there is no separate
            // "Author" role to grant here, only the baseline "Reader" role.
            const string defaultRole = "Reader";
            if (!await _roleManager.RoleExistsAsync(defaultRole))
                await _roleManager.CreateAsync(new IdentityRole<int>(defaultRole));
            await _userManager.AddToRoleAsync(user, defaultRole);

            // Always create a UserProfiles row — even if every optional field was
            // left blank — so every account has one consistently, and the profile
            // page never has to special-case "no profile yet".
            await _userService.UpsertProfileAsync(
                user.Id, Input.FullName, Input.Bio, Input.Country, Input.WebsiteURL, Input.ProfileImageURL);

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
