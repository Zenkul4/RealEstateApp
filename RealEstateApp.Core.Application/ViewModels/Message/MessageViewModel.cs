using System;

namespace RealEstateApp.Core.Application.ViewModels.Message;

public class MessageViewModel
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = null!;
    public string AgentId { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime Created { get; set; }
    public bool IsFromClient { get; set; }
}
