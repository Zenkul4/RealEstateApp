
namespace RealEstateApp.Core.Application.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using RealEstateApp.Core.Domain.Entities;

public class ImprovementService : IImprovementService
{
    private readonly IGenericRepositoryAsync<Improvement> _improvementRepository;
    private readonly IMapper _mapper;

    public ImprovementService(
        IGenericRepositoryAsync<Improvement> improvementRepository,
        IMapper mapper)
    {
        _improvementRepository = improvementRepository;
        _mapper = mapper;
    }

    public async Task<SaveImprovementViewModel> Add(SaveImprovementViewModel vm)
    {
        var improvement = _mapper.Map<Improvement>(vm);
        await _improvementRepository.AddAsync(improvement);
        return _mapper.Map<SaveImprovementViewModel>(improvement);
    }

    public async Task Update(SaveImprovementViewModel vm, int id)
    {
        var improvement = await _improvementRepository.GetByIdAsync(id);
        if (improvement == null)
            throw new KeyNotFoundException($"Improvement with id {id} not found");

        _mapper.Map(vm, improvement);
        await _improvementRepository.UpdateAsync(improvement);
    }

    public async Task Delete(int id)
    {
        var improvement = await _improvementRepository.GetByIdAsync(id);
        if (improvement == null)
            throw new KeyNotFoundException($"Improvement with id {id} not found");

        await _improvementRepository.DeleteAsync(improvement);
    }

    public async Task<SaveImprovementViewModel> GetByIdSaveViewModel(int id)
    {
        var improvement = await _improvementRepository.GetByIdAsync(id);
        if (improvement == null)
            throw new KeyNotFoundException($"Improvement with id {id} not found");

        return _mapper.Map<SaveImprovementViewModel>(improvement);
    }

    public async Task<List<ImprovementViewModel>> GetAllWithInclude()
    {
        var improvements = await _improvementRepository.GetAllAsync();
        return _mapper.Map<List<ImprovementViewModel>>(improvements);
    }
}