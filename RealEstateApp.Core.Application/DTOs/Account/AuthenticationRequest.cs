using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Account;

public class AuthenticationRequest
{
    [Required(ErrorMessage = "El usuario o correo electrónico es requerido.")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}
