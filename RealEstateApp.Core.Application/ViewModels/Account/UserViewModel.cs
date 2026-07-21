// ViewModels/Account/UserViewModel.cs
namespace RealEstateApp.Core.Application.ViewModels.Account;

public class UserViewModel
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public int ActivePropertiesCount { get; set; }
}