namespace RealEstateApp.Core.Application.DTOs.Account;

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public UserType UserType { get; set; }
}

public enum UserType
{
    Administrator,
    Client,
    Agent,
    Developer
}