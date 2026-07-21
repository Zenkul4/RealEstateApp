using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.DTOs.Account;

namespace RealEstateApp.Core.Application.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es requerida.")]
    [StringLength(20, MinimumLength = 9, ErrorMessage = "La cédula no tiene un formato válido.")]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(50, MinimumLength = 4)]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [Phone(ErrorMessage = "Escribe un número de teléfono válido.")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "La contraseña y su confirmación no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona cómo deseas usar la plataforma.")]
    [Display(Name = "Tipo de cuenta")]
    public UserType UserType { get; set; } = UserType.Client;

    [Required(ErrorMessage = "Selecciona una foto de perfil.")]
    [Display(Name = "Foto de perfil")]
    public IFormFile? Photo { get; set; }
}
