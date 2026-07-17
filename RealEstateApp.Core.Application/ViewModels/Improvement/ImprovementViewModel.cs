// ViewModels/Improvement/ImprovementViewModel.cs
namespace RealEstateApp.Core.Application.ViewModels.Improvement;

public class ImprovementViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}