namespace RealEstateApp.Core.Application.DTOs.Account;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
}
