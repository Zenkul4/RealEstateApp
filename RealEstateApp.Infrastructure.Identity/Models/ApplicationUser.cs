using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Infrastructure.Identity.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Cedula { get; set; } = null!;

    // Es nullable a nivel de base de datos porque IdentityUser se instancia vacío 
    // antes de mapear los datos, aunque en el registro funcional sea requerido.
    public string? PhotoUrl { get; set; }
}