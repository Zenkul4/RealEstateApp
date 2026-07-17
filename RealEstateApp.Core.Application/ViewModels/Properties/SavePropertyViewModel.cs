// ViewModels/Properties/SavePropertyViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Properties;

public class SavePropertyViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El precio es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "El tamaño de la propiedad es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El tamaño debe ser mayor a cero.")]
    public decimal LandSize { get; set; }

    [Required(ErrorMessage = "La cantidad de habitaciones es requerida.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad de habitaciones no puede ser menor a cero.")]
    public int Rooms { get; set; }

    [Required(ErrorMessage = "La cantidad de baños es requerida.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad de baños no puede ser menor a cero.")]
    public int Bathrooms { get; set; }

    [Required(ErrorMessage = "La descripción es requerida.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "El tipo de propiedad es requerido.")]
    public int PropertyTypeId { get; set; }

    [Required(ErrorMessage = "El tipo de venta es requerido.")]
    public int SaleTypeId { get; set; }

    [Required(ErrorMessage = "El agente es requerido.")]
    public string AgentId { get; set; } = null!;

    [Required(ErrorMessage = "Debe seleccionar al menos una mejora.")]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos una mejora.")]
    public List<int> Improvements { get; set; } = new();
}