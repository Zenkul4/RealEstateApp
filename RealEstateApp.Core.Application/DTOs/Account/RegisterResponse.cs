namespace RealEstateApp.Core.Application.DTOs.Account;

public class RegisterResponse
{
    public bool HasError { get; set; }
    public string? Error { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmailConfirmationToken { get; set; } = string.Empty;
}
