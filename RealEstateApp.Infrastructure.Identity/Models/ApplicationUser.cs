using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Infrastructure.Identity.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Cedula { get; set; } = null!;

    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}