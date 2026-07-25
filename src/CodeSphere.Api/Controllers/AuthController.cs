using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CodeSphere.Api.DTOs;
using CodeSphere.Core.Common;
using CodeSphere.Core.Entities;
using CodeSphere.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CodeSphere.Api.Controllers;

/// <summary>
/// Registers new accounts and issues short-lived JWT bearer tokens for the
/// same Identity accounts used by the Razor Pages front-end, so
/// external/API-only clients can authenticate against the shared CodeSphere
/// user store.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IUserService _userService;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<int>> roleManager,
        IUserService userService,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userService = userService;
        _config = config;
    }

    /// <summary>POST /api/auth/register — create a new account and return a JWT access token for it.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // Same validator the Razor Pages Register form uses — one rule,
        // enforced identically regardless of which front-end is used.
        if (!ImageUrlValidator.IsValid(request.ProfileImageURL, out var imageError))
        {
            ModelState.AddModelError(nameof(request.ProfileImageURL), imageError!);
            return ValidationProblem(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        // Every registered account may write articles — there is no separate
        // "Author" role to grant here, only the baseline "Reader" role.
        const string defaultRole = "Reader";
        if (!await _roleManager.RoleExistsAsync(defaultRole))
            await _roleManager.CreateAsync(new IdentityRole<int>(defaultRole));
        await _userManager.AddToRoleAsync(user, defaultRole);

        await _userService.UpsertProfileAsync(
            user.Id, request.FullName, request.Bio, request.Country, request.WebsiteURL, request.ProfileImageURL);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expires) = GenerateJwt(user, roles);

        var response = new TokenResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Roles = roles.ToList()
        };

        return CreatedAtAction("GetById", "Users", new { id = user.Id }, response);
    }

    /// <summary>POST /api/auth/login — exchange email/password for a JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expires) = GenerateJwt(user, roles);

        return Ok(new TokenResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    private (string token, DateTime expires) GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "120");
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
