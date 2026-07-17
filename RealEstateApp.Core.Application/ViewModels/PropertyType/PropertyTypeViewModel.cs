// ViewModels/PropertyType/PropertyTypeViewModel.cs
namespace RealEstateApp.Core.Application.ViewModels.PropertyType;

public class PropertyTypeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}