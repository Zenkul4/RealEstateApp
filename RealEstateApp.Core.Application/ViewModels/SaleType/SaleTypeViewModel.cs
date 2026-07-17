// ViewModels/SaleType/SaleTypeViewModel.cs
namespace RealEstateApp.Core.Application.ViewModels.SaleType;

public class SaleTypeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}