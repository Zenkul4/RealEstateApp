using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities;

public class PropertyImage : AuditableBaseEntity
{
    public int PropertyId { get; set; }
    public string ImageUrl { get; set; } = null!;

    public Property Property { get; set; } = null!;
}