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
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public PropertyService(
        IGenericRepositoryAsync<Property> propertyRepository,
        IGenericRepositoryAsync<Improvement> improvementRepository,
        IUserService userService,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _improvementRepository = improvementRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
    {
        var property = _mapper.Map<Property>(vm);
        
        // Generar código de propiedad de 6 dígitos único
        var random = new Random();
        string code;
        bool isUnique = false;
        do
        {
            code = random.Next(100000, 999999).ToString();
            var existingProperties = await _propertyRepository.GetAllAsync();
            isUnique = !existingProperties.Any(p => p.Code == code);
        } while (!isUnique);

        property.Code = code;

        property.Improvements = new List<Improvement>();
        foreach (var improvementId in vm.SelectedImprovements)
        {
            var improvement = await _improvementRepository.GetByIdAsync(improvementId);
            if (improvement != null)
            {
                property.Improvements.Add(improvement);
            }
        }

        // Map Image URLs
        property.Images = new List<PropertyImage>();
        if (!string.IsNullOrEmpty(vm.ImageOneUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageOneUrl });
        if (!string.IsNullOrEmpty(vm.ImageTwoUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageTwoUrl });
        if (!string.IsNullOrEmpty(vm.ImageThreeUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageThreeUrl });
        if (!string.IsNullOrEmpty(vm.ImageFourUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageFourUrl });

        await _propertyRepository.AddAsync(property);
        return _mapper.Map<SavePropertyViewModel>(property);
    }

    public async Task Update(SavePropertyViewModel vm, int id)
    {
        var property = await _propertyRepository.GetByIdWithIncludeAsync(id, p => p.Improvements, p => p.Images);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        _mapper.Map(vm, property);

        property.Improvements.Clear();
        foreach (var improvementId in vm.SelectedImprovements)
        {
            var improvement = await _improvementRepository.GetByIdAsync(improvementId);
            if (improvement != null)
            {
                property.Improvements.Add(improvement);
            }
        }

        property.Images.Clear();
        if (!string.IsNullOrEmpty(vm.ImageOneUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageOneUrl });
        if (!string.IsNullOrEmpty(vm.ImageTwoUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageTwoUrl });
        if (!string.IsNullOrEmpty(vm.ImageThreeUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageThreeUrl });
        if (!string.IsNullOrEmpty(vm.ImageFourUrl)) property.Images.Add(new PropertyImage { ImageUrl = vm.ImageFourUrl });

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
        var property = await _propertyRepository.GetByIdWithIncludeAsync(id, p => p.Improvements, p => p.Images);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        var vm = _mapper.Map<SavePropertyViewModel>(property);
        vm.SelectedImprovements = property.Improvements.Select(i => i.Id).ToList();

        // Load existing image URLs
        var imagesList = property.Images.ToList();
        if (imagesList.Count > 0) vm.ImageOneUrl = imagesList[0].ImageUrl;
        if (imagesList.Count > 1) vm.ImageTwoUrl = imagesList[1].ImageUrl;
        if (imagesList.Count > 2) vm.ImageThreeUrl = imagesList[2].ImageUrl;
        if (imagesList.Count > 3) vm.ImageFourUrl = imagesList[3].ImageUrl;

        return vm;
    }

    public async Task<List<PropertyViewModel>> GetAllWithInclude()
    {
        var properties = await _propertyRepository.GetAllWithIncludeAsync(p => p.PropertyType, p => p.SaleType, p => p.Images);
        var vms = _mapper.Map<List<PropertyViewModel>>(properties);
        for (int i = 0; i < properties.Count; i++)
        {
            vms[i].ImageUrls = properties[i].Images.Select(img => img.ImageUrl).ToList();
            try
            {
                var agent = await _userService.GetByIdAsync(properties[i].AgentId);
                if (agent != null)
                {
                    vms[i].AgentName = agent.FullName;
                }
            }
            catch {}
        }
        return vms;
    }

    public async Task<PropertyViewModel> GetByIdWithInclude(int id)
    {
        var property = await _propertyRepository.GetByIdWithIncludeAsync(id, p => p.PropertyType, p => p.SaleType, p => p.Images, p => p.Improvements);
        if (property == null)
            throw new KeyNotFoundException($"Property with id {id} not found");

        var vm = _mapper.Map<PropertyViewModel>(property);
        vm.ImageUrls = property.Images.Select(img => img.ImageUrl).ToList();
        vm.ImprovementNames = property.Improvements.Select(imp => imp.Name).ToList();

        try
        {
            var agent = await _userService.GetByIdAsync(property.AgentId);
            if (agent != null)
            {
                vm.AgentName = agent.FullName;
            }
        }
        catch {}

        return vm;
    }
}