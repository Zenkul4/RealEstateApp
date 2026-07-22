using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Core.Application.ViewModels.Agent;

public class SaveAgentProfileViewModel
{
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Phone { get; set; } = null!;

    public string? PhotoUrl { get; set; }

    public IFormFile? Photo { get; set; }
}
