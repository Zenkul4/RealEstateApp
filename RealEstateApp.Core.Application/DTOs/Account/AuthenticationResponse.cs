namespace RealEstateApp.Core.Application.DTOs.Account;

public class AuthenticationResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsVerified { get; set; }
    public bool HasError { get; set; }
    public string? Error { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
}
