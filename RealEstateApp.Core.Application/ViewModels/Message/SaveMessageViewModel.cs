using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Message;

public class SaveMessageViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la propiedad.")]
    public int PropertyId { get; set; }

    [Required(ErrorMessage = "El cliente es requerido.")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "El agente es requerido.")]
    public string AgentId { get; set; } = null!;

    [Required(ErrorMessage = "Debe escribir un mensaje antes de enviarlo.")]
    public string Content { get; set; } = null!;
}
