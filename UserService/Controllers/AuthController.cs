using Microsoft.AspNetCore.Authorization;
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
[Route("api/auth")]
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
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
            return BadRequest(new { message = "Korisničko ime je obavezno." });

        if (string.IsNullOrWhiteSpace(model.Password))
            return BadRequest(new { message = "Lozinka je obavezna." });

        if (string.IsNullOrWhiteSpace(model.Email))
            return BadRequest(new { message = "Email adresa je obavezna." });

        if (string.IsNullOrWhiteSpace(model.FullName))
            return BadRequest(new { message = "Puno ime i prezime je obavezno." });

        var existingUser = await _userManager.FindByNameAsync(model.Username.Trim());
        if (existingUser != null)
            return BadRequest(new { message = "Korisničko ime već postoji." });

        var existingEmail = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (existingEmail != null)
            return BadRequest(new { message = "Email adresa već postoji u sistemu." });

        var user = new AppUser
        {
            UserName = model.Username.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var roleToAssign = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role;

            if (roleToAssign == "User" || roleToAssign == "Moderator" || roleToAssign == "Admin") 
            {
                await _userManager.AddToRoleAsync(user, roleToAssign);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return Ok(new
            {
                message = "Korisnik uspješno kreiran.",
                userId = user.Id,
                userName = user.UserName,
                email = user.Email,
                fullName = user.FullName
            });
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { message = "Registracija nije uspjela.", errors });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();

        var result = new List<object>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var role = roles.FirstOrDefault() ?? "User";

            result.Add(new
            {
                u.Id,
                Username = u.UserName,
                u.Email,
                u.FullName,
                Role = role
            });
        }

        return Ok(result);
    }

    [HttpPut("users/{userId}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
            return BadRequest(new { message = "Uloga je obavezna." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Korisnik nije pronađen." });

        // Ne dozvoljavaj promjenu sopstvene uloge
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == userId)
            return BadRequest(new { message = "Ne možete promijeniti sopstvenu ulogu." });

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Ukloni sve trenutne uloge
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest(new { message = "Greška pri uklanjanju starih uloga." });
        }

        // Dodaj novu ulogu
        var addResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
            return BadRequest(new { message = "Greška pri dodavanju nove uloge." });

        return Ok(new { message = $"Uloga korisnika uspješno ažurirana na {request.Role}." });
    }

    [HttpDelete("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Korisnik nije pronađen." });

        // Ne dozvoljavaj brisanje samog sebe
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == userId)
            return BadRequest(new { message = "Ne možete obrisati sopstveni nalog." });

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Brisanje korisnika nije uspjelo.", errors });
        }

        return Ok(new { message = "Korisnik uspješno obrisan." });
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
    public string? Role { get; set; } // "User" ili "Moderator" ili "Admin" - default je "User"
}

public class UpdateRoleRequest
{
    public string Role { get; set; } = "";
}