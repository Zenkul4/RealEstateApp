using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Message;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IMessageService
{
    Task<List<MessageViewModel>> GetConversationAsync(int propertyId, string clientId, string agentId);
    Task AddAsync(SaveMessageViewModel vm, string senderId);
    Task<List<string>> GetClientIdsWhoMessagedPropertyAsync(int propertyId, string agentId);
}
