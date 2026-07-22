using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Admin;

public class SaveUserAdminViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string UserName { get; set; } = null!;

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmPassword { get; set; }

    public string? Role { get; set; }
}
