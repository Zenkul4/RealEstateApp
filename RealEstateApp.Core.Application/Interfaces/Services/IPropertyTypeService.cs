// Interfaces/IPropertyTypeService.cs
namespace RealEstateApp.Core.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

public interface IPropertyTypeService
{
    Task<SavePropertyTypeViewModel> Add(SavePropertyTypeViewModel vm);
    Task Update(SavePropertyTypeViewModel vm, int id);
    Task Delete(int id);
    Task<SavePropertyTypeViewModel> GetByIdSaveViewModel(int id);
    Task<List<PropertyTypeViewModel>> GetAllWithInclude();
}