// ViewModels/SaleType/SaveSaleTypeViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.SaleType;

public class SaveSaleTypeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es requerida.")]
    public string Description { get; set; } = null!;
}