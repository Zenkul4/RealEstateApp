// ViewModels/SaleType/SaveSaleTypeViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.SaleType;

public class SaveSaleTypeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
    public string Description { get; set; } = null!;
}