using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Account;

public class RegisterRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string UserName { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    [Required] public string Cedula { get; set; } = string.Empty;
    public UserType UserType { get; set; }
}

public enum UserType
{
    Administrator,
    Client,
    Agent,
    Developer
}
