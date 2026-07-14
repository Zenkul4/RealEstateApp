using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities;

public class Property : AuditableBaseEntity
{
    public string Code { get; set; } = null!;
    public int PropertyTypeId { get; set; }
    public int SaleTypeId { get; set; }
    public decimal Price { get; set; }
    public int Rooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal Size { get; set; } 
    public string Description { get; set; } = null!;
    public PropertyStatus Status { get; set; }
    public string AgentId { get; set; } = null!; 

    public PropertyType PropertyType { get; set; } = null!;
    public SaleType SaleType { get; set; } = null!;
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Improvement> Improvements { get; set; } = new List<Improvement>();
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<FavoriteProperty> Favorites { get; set; } = new List<FavoriteProperty>();
}