using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu usuario o correo electrónico.")]
    [Display(Name = "Usuario o correo")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener mi sesión abierta")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
