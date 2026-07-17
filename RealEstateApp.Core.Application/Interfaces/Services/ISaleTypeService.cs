// Interfaces/ISaleTypeService.cs
namespace RealEstateApp.Core.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.SaleType;

public interface ISaleTypeService
{
    Task<SaveSaleTypeViewModel> Add(SaveSaleTypeViewModel vm);
    Task Update(SaveSaleTypeViewModel vm, int id);
    Task Delete(int id);
    Task<SaveSaleTypeViewModel> GetByIdSaveViewModel(int id);
    Task<List<SaleTypeViewModel>> GetAllWithInclude();
}