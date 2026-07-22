using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Services;

public class OfferService : IOfferService
{
    private readonly IGenericRepositoryAsync<Offer> _offerRepository;
    private readonly IMapper _mapper;

    public OfferService(IGenericRepositoryAsync<Offer> offerRepository, IMapper mapper)
    {
        _offerRepository = offerRepository;
        _mapper = mapper;
    }

    public async Task<List<OfferViewModel>> GetOffersByClientAndPropertyAsync(string clientId, int propertyId)
    {
        var offers = await _offerRepository.GetAllAsync();
        var clientOffers = offers
            .Where(o => o.PropertyId == propertyId && o.ClientId == clientId)
            .OrderByDescending(o => o.Created)
            .ToList();

        return _mapper.Map<List<OfferViewModel>>(clientOffers);
    }

    public async Task<bool> HasPendingOfferAsync(string clientId, int propertyId)
    {
        var offers = await _offerRepository.GetAllAsync();
        return offers.Any(o => o.PropertyId == propertyId && o.ClientId == clientId && o.Status == OfferStatus.Pendiente);
    }

    public async Task<bool> HasAcceptedOfferAsync(int propertyId)
    {
        var offers = await _offerRepository.GetAllAsync();
        return offers.Any(o => o.PropertyId == propertyId && o.Status == OfferStatus.Aceptada);
    }

    public async Task AddAsync(SaveOfferViewModel vm)
    {
        var entity = _mapper.Map<Offer>(vm);
        entity.Status = OfferStatus.Pendiente;
        await _offerRepository.AddAsync(entity);
    }
}
