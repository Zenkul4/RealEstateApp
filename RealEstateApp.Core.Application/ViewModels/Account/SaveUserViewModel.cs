using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Account;

public class SaveUserViewModel
{
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato de email no es válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El nombre completo es requerido.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "El rol es requerido.")]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }
}