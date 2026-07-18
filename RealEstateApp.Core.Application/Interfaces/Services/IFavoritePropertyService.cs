using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Properties;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IFavoritePropertyService
{
    Task AddAsync(int propertyId, string clientId);
    Task DeleteAsync(int propertyId, string clientId);
    Task<List<PropertyViewModel>> GetFavoritesByClientAsync(string clientId);
    Task<bool> IsFavoriteAsync(int propertyId, string clientId);
}
