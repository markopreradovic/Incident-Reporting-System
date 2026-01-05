using Microsoft.AspNetCore.Identity;

namespace UserService.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}