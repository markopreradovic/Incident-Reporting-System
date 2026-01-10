using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/auth")] // Fiksna ruta – Ocelot je podešen na /api/auth/{catchAll}
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    private const string SecretKey = "tvoj-super-tajni-kljuc-od-barem-32-karaktera!!";
    private const string Issuer = "IncidentSystem";
    private const string Audience = "IncidentSystem";

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Korisničko ime i lozinka su obavezni.");

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Pogrešno korisničko ime ili lozinka.");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            user = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                roles
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Korisničko ime i lozinka su obavezni.");

        var user = new AppUser
        {
            UserName = model.Username.Trim(),
            Email = model.Email?.Trim(),
            FullName = model.FullName?.Trim() ?? ""
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var role = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role.Trim();
            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { message = "Korisnik uspješno kreiran.", role });
        }

        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();

        var result = new List<object>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FullName,
                Roles = roles
            });
        }

        return Ok(result);
    }
}

public class LoginModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Role { get; set; } // "User" ili "Moderator" ili "Administrator"
}