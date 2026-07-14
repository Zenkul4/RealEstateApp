using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities;

public class Message : AuditableBaseEntity
{
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = null!;
    public string AgentId { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string Content { get; set; } = null!;

    public Property Property { get; set; } = null!;
}