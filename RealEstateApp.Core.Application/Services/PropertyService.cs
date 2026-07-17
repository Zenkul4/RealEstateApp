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
    private readonly IGenericRepositoryAsync<Improvement> _improvementRepository;
    private readonly IMapper _mapper;

    public PropertyService(
        IGenericRepositoryAsync<Property> propertyRepository,
        IGenericRepositoryAsync<Improvement> improvementRepository,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _improvementRepository = improvementRepository;
        _mapper = mapper;
    }

    public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
    {
        var property = _mapper.Map<Property>(vm);
        
        property.Improvements = new List<Improvement>();
        foreach (var improvementId in vm.Improvements)
        {
            var improvement = await _improvementRepository.GetByIdAsync(improvementId);
            if (improvement != null)
            {
                property.Improvements.Add(improvement);
            }
        }

        await _propertyRepository.AddAsync(property);
        return _mapper.Map<SavePropertyViewModel>(property);
    }

    public async Task Update(SavePropertyViewModel vm, int id)
    {
        var property = await _propertyRepository.GetByIdWithIncludeAsync(id, p => p.Improvements);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        _mapper.Map(vm, property);

        property.Improvements.Clear();
        foreach (var improvementId in vm.Improvements)
        {
            var improvement = await _improvementRepository.GetByIdAsync(improvementId);
            if (improvement != null)
            {
                property.Improvements.Add(improvement);
            }
        }

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
        var property = await _propertyRepository.GetByIdWithIncludeAsync(id, p => p.Improvements);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        var vm = _mapper.Map<SavePropertyViewModel>(property);
        vm.Improvements = property.Improvements.Select(i => i.Id).ToList();
        return vm;
    }

    public async Task<List<PropertyViewModel>> GetAllWithInclude()
    {
        var properties = await _propertyRepository.GetAllWithIncludeAsync(p => p.PropertyType, p => p.SaleType);
        return _mapper.Map<List<PropertyViewModel>>(properties);
    }
}