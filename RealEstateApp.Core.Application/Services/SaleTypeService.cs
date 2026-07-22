namespace RealEstateApp.Core.Application.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using RealEstateApp.Core.Domain.Entities;

public class SaleTypeService : ISaleTypeService
{
    private readonly IGenericRepositoryAsync<SaleType> _saleTypeRepository;
    private readonly IMapper _mapper;

    public SaleTypeService(
        IGenericRepositoryAsync<SaleType> saleTypeRepository,
        IMapper mapper)
    {
        _saleTypeRepository = saleTypeRepository;
        _mapper = mapper;
    }

    public async Task<SaveSaleTypeViewModel> Add(SaveSaleTypeViewModel vm)
    {
        var saleType = _mapper.Map<SaleType>(vm);
        await _saleTypeRepository.AddAsync(saleType);
        return _mapper.Map<SaveSaleTypeViewModel>(saleType);
    }

    public async Task Update(SaveSaleTypeViewModel vm, int id)
    {
        var saleType = await _saleTypeRepository.GetByIdAsync(id);
        if (saleType == null)
            throw new KeyNotFoundException($"SaleType with id {id} not found");

        _mapper.Map(vm, saleType);
        await _saleTypeRepository.UpdateAsync(saleType);
    }

    public async Task Delete(int id)
    {
        var saleType = await _saleTypeRepository.GetByIdAsync(id);
        if (saleType == null)
            throw new KeyNotFoundException($"SaleType with id {id} not found");

        await _saleTypeRepository.DeleteAsync(saleType);
    }

    public async Task<SaveSaleTypeViewModel> GetByIdSaveViewModel(int id)
    {
        var saleType = await _saleTypeRepository.GetByIdAsync(id);
        if (saleType == null)
            throw new KeyNotFoundException($"SaleType with id {id} not found");

        return _mapper.Map<SaveSaleTypeViewModel>(saleType);
    }

    public async Task<List<SaleTypeViewModel>> GetAllWithInclude()
    {
        var saleTypes = await _saleTypeRepository.GetAllAsync();
        return _mapper.Map<List<SaleTypeViewModel>>(saleTypes);
    }
}