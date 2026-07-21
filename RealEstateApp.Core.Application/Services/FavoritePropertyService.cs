using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Properties;
using RealEstateApp.Core.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Services;

public class FavoritePropertyService : IFavoritePropertyService
{
    private readonly IGenericRepositoryAsync<FavoriteProperty> _favoritePropertyRepository;
    private readonly IGenericRepositoryAsync<Property> _propertyRepository;
    private readonly IMapper _mapper;

    public FavoritePropertyService(
        IGenericRepositoryAsync<FavoriteProperty> favoritePropertyRepository,
        IGenericRepositoryAsync<Property> propertyRepository,
        IMapper mapper)
    {
        _favoritePropertyRepository = favoritePropertyRepository;
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task AddAsync(int propertyId, string clientId)
    {
        var alreadyFavorite = await IsFavoriteAsync(propertyId, clientId);
        if (alreadyFavorite) return;

        var favorite = new FavoriteProperty
        {
            PropertyId = propertyId,
            ClientId = clientId
        };
        await _favoritePropertyRepository.AddAsync(favorite);
    }

    public async Task DeleteAsync(int propertyId, string clientId)
    {
        var favorites = await _favoritePropertyRepository.GetAllAsync();
        var favorite = favorites.FirstOrDefault(f => f.PropertyId == propertyId && f.ClientId == clientId);
        if (favorite != null)
        {
            await _favoritePropertyRepository.DeleteAsync(favorite);
        }
    }

    public async Task<List<PropertyViewModel>> GetFavoritesByClientAsync(string clientId)
    {
        var allFavorites = await _favoritePropertyRepository.GetAllWithIncludeAsync(p => p.Property);
        var clientFavorites = allFavorites.Where(f => f.ClientId == clientId).Select(f => f.Property).ToList();

        var properties = new List<Property>();
        foreach (var favProp in clientFavorites)
        {
            var propWithIncludes = await _propertyRepository.GetByIdWithIncludeAsync(favProp.Id, p => p.PropertyType, p => p.SaleType, p => p.Images);
            if (propWithIncludes != null)
            {
                properties.Add(propWithIncludes);
            }
        }

        var vms = _mapper.Map<List<PropertyViewModel>>(properties);
        for (int i = 0; i < properties.Count; i++)
        {
            vms[i].ImageUrls = properties[i].Images.Select(img => img.ImageUrl).ToList();
        }
        return vms;
    }

    public async Task<bool> IsFavoriteAsync(int propertyId, string clientId)
    {
        var favorites = await _favoritePropertyRepository.GetAllAsync();
        return favorites.Any(f => f.PropertyId == propertyId && f.ClientId == clientId);
    }
}
