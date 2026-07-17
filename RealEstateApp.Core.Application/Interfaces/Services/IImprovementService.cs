// Interfaces/IImprovementService.cs
namespace RealEstateApp.Core.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Improvement;

public interface IImprovementService
{
    Task<SaveImprovementViewModel> Add(SaveImprovementViewModel vm);
    Task Update(SaveImprovementViewModel vm, int id);
    Task Delete(int id);
    Task<SaveImprovementViewModel> GetByIdSaveViewModel(int id);
    Task<List<ImprovementViewModel>> GetAllWithInclude();
}