using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities;

public class Offer : AuditableBaseEntity
{
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = null!;
    public decimal Amount { get; set; }
    public OfferStatus Status { get; set; }

    public Property Property { get; set; } = null!;
}