// Services/PropertyService.cs
namespace RealEstateApp.Core.Application.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.ViewModels.Properties;
using RealEstateApp.Core.Domain.Entities;

public class PropertyService : IPropertyService
{
    private readonly IGenericRepositoryAsync<Property> _propertyRepository;
    private readonly IMapper _mapper;

    public PropertyService(
        IGenericRepositoryAsync<Property> propertyRepository,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
    {
        var property = _mapper.Map<Property>(vm);
        await _propertyRepository.AddAsync(property);
        return _mapper.Map<SavePropertyViewModel>(property);
    }

    public async Task Update(SavePropertyViewModel vm, int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        _mapper.Map(vm, property);
        await _propertyRepository.UpdateAsync(property);
    }

    public async Task Delete(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        await _propertyRepository.DeleteAsync(property);
    }

    public async Task<SavePropertyViewModel> GetByIdSaveViewModel(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        return _mapper.Map<SavePropertyViewModel>(property);
    }

    public async Task<List<PropertyViewModel>> GetAllWithInclude()
    {
        var properties = await _propertyRepository.GetAllAsync();
        return _mapper.Map<List<PropertyViewModel>>(properties);
    }
}