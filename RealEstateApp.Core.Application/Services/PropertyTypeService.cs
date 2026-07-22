namespace RealEstateApp.Core.Application.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using RealEstateApp.Core.Domain.Entities;

public class PropertyTypeService : IPropertyTypeService
{   
    private readonly IGenericRepositoryAsync<PropertyType> _propertyTypeRepository;
    private readonly IMapper _mapper;

    public PropertyTypeService(
        IGenericRepositoryAsync<PropertyType> propertyTypeRepository,
        IMapper mapper)
    {
        _propertyTypeRepository = propertyTypeRepository;
        _mapper = mapper;
    }

    public async Task<SavePropertyTypeViewModel> Add(SavePropertyTypeViewModel vm)
    {
        var propertyType = _mapper.Map<PropertyType>(vm);
        await _propertyTypeRepository.AddAsync(propertyType);
        return _mapper.Map<SavePropertyTypeViewModel>(propertyType);
    }

    public async Task Update(SavePropertyTypeViewModel vm, int id)
    {
        var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
        if (propertyType == null)
            throw new KeyNotFoundException($"PropertyType with id {id} not found");

        _mapper.Map(vm, propertyType);
        await _propertyTypeRepository.UpdateAsync(propertyType);
    }

    public async Task Delete(int id)
    {
        var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
        if (propertyType == null)
            throw new KeyNotFoundException($"PropertyType with id {id} not found");

        await _propertyTypeRepository.DeleteAsync(propertyType);
    }

    public async Task<SavePropertyTypeViewModel> GetByIdSaveViewModel(int id)
    {
        var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
        if (propertyType == null)
            throw new KeyNotFoundException($"PropertyType with id {id} not found");

        return _mapper.Map<SavePropertyTypeViewModel>(propertyType);
    }

    public async Task<List<PropertyTypeViewModel>> GetAllWithInclude()
    {
        var propertyTypes = await _propertyTypeRepository.GetAllAsync();
        return _mapper.Map<List<PropertyTypeViewModel>>(propertyTypes);
    }
}