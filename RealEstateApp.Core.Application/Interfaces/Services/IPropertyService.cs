namespace RealEstateApp.Core.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Properties;

public interface IPropertyService
{
    Task<SavePropertyViewModel> Add(SavePropertyViewModel vm);
    Task Update(SavePropertyViewModel vm, int id);
    Task Delete(int id);
    Task<SavePropertyViewModel> GetByIdSaveViewModel(int id);
    Task<List<PropertyViewModel>> GetAllWithInclude();
    Task<PropertyViewModel> GetByIdWithInclude(int id);
}