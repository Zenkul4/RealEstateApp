using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities;

public class FavoriteProperty : AuditableBaseEntity
{
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = null!; 

    public Property Property { get; set; } = null!;
}