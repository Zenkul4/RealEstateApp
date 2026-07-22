using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Message;
using RealEstateApp.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Services;

public class MessageService : IMessageService
{
    private readonly IGenericRepositoryAsync<Message> _messageRepository;
    private readonly IMapper _mapper;

    public MessageService(IGenericRepositoryAsync<Message> messageRepository, IMapper mapper)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<List<MessageViewModel>> GetConversationAsync(int propertyId, string clientId, string agentId)
    {
        var messages = await _messageRepository.GetAllAsync();
        var conversation = messages
            .Where(m => m.PropertyId == propertyId && m.ClientId == clientId && m.AgentId == agentId)
            .OrderBy(m => m.Created)
            .ToList();

        var vms = _mapper.Map<List<MessageViewModel>>(conversation);
        for (int i = 0; i < conversation.Count; i++)
        {
            vms[i].IsFromClient = conversation[i].SenderId == clientId;
        }

        return vms;
    }

    public async Task AddAsync(SaveMessageViewModel vm, string senderId)
    {
        var entity = _mapper.Map<Message>(vm);
        entity.SenderId = senderId;
        await _messageRepository.AddAsync(entity);
    }
}
