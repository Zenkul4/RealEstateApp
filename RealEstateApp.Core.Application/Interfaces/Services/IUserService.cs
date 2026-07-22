// Interfaces/IUserService.cs
namespace RealEstateApp.Core.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Account;

public interface IUserService
{
    Task<List<UserViewModel>> GetAllAgentsAsync();
    Task<List<UserViewModel>> GetUsersByRoleAsync(string role);
    Task<UserViewModel> GetByIdAsync(string id);
    Task UpdateActiveStatusAsync(string id, bool isActive);
}