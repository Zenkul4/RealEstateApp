// Services/UserService.cs
namespace RealEstateApp.Core.Application.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;

public class UserService : IUserService
{
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public UserService(
        IAccountService accountService,
        IMapper mapper)
    {
        _accountService = accountService;
        _mapper = mapper;
    }

    public async Task<List<UserViewModel>> GetAllAgentsAsync()
    {
        var agents = await _accountService.GetUsersByRoleAsync("Agente");
        if (agents.Count == 0)
        {
            agents = await _accountService.GetUsersByRoleAsync("Agent");
        }
        return _mapper.Map<List<UserViewModel>>(agents);
    }

    public async Task<List<UserViewModel>> GetUsersByRoleAsync(string role)
    {
        var users = await _accountService.GetUsersByRoleAsync(role);
        return _mapper.Map<List<UserViewModel>>(users);
    }

    public async Task<UserViewModel> GetByIdAsync(string id)
    {
        var user = await _accountService.GetUserByIdAsync(id);
        return _mapper.Map<UserViewModel>(user);
    }

    public async Task UpdateActiveStatusAsync(string id, bool isActive)
    {
        await _accountService.UpdateUserStatusAsync(id, isActive);
    }
}