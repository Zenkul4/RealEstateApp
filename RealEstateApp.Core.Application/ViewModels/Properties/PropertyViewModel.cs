namespace RealEstateApp.Core.Application.ViewModels.Properties;

public class PropertyViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal LandSize { get; set; }
    public int Rooms { get; set; }
    public int Bathrooms { get; set; }
    public string Description { get; set; } = null!;
    public string AgentId { get; set; } = null!;
    public string AgentName { get; set; } = null!;
    public int PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = null!;
    public int SaleTypeId { get; set; }
    public string SaleTypeName { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public System.Collections.Generic.List<string> ImageUrls { get; set; } = new();
    public System.Collections.Generic.List<string> ImprovementNames { get; set; } = new();
}
