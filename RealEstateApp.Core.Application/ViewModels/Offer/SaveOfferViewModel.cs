using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Offer;

public class SaveOfferViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la propiedad.")]
    public int PropertyId { get; set; }

    [Required(ErrorMessage = "El cliente es requerido.")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "Debe ingresar el monto de la oferta.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la oferta debe ser un valor numérico mayor que cero.")]
    public decimal Amount { get; set; }
}
