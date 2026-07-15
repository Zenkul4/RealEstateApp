namespace RealEstateApp.Core.Application.DTOs.Account;

public class AuthenticationRequest
{
    public string EmailOrUserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}