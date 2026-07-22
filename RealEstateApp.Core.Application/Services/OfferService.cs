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
    private readonly IGenericRepositoryAsync<Property> _propertyRepository;
    private readonly IMapper _mapper;

    public OfferService(
        IGenericRepositoryAsync<Offer> offerRepository,
        IGenericRepositoryAsync<Property> propertyRepository,
        IMapper mapper)
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
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

    public async Task<List<OfferViewModel>> GetOffersByPropertyAsync(int propertyId)
    {
        var offers = await _offerRepository.GetAllAsync();
        var propertyOffers = offers
            .Where(o => o.PropertyId == propertyId)
            .OrderByDescending(o => o.Created)
            .ToList();

        return _mapper.Map<List<OfferViewModel>>(propertyOffers);
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

    public async Task AcceptOfferAsync(int offerId, string agentId)
    {
        var allOffers = await _offerRepository.GetAllAsync();
        var offer = allOffers.FirstOrDefault(o => o.Id == offerId);
        if (offer == null || offer.Status != OfferStatus.Pendiente)
        {
            throw new InvalidOperationException("La oferta no existe o ya ha sido procesada.");
        }

        var property = await _propertyRepository.GetByIdWithIncludeAsync(offer.PropertyId);
        if (property == null || property.AgentId != agentId)
        {
            throw new UnauthorizedAccessException("No tiene autorización para modificar ofertas de esta propiedad.");
        }

        if (property.Status == PropertyStatus.Vendida)
        {
            throw new InvalidOperationException("No se pueden aceptar ofertas en una propiedad que ya está vendida.");
        }

        // 1. Aceptar oferta actual
        offer.Status = OfferStatus.Aceptada;
        await _offerRepository.UpdateAsync(offer);

        // 2. Cambiar propiedad a Vendida
        property.Status = PropertyStatus.Vendida;
        await _propertyRepository.UpdateAsync(property);

        // 3. Rechazar todas las demás ofertas pendientes de esa propiedad
        var otherOffers = allOffers.Where(o => o.PropertyId == offer.PropertyId && o.Id != offerId && o.Status == OfferStatus.Pendiente).ToList();
        foreach (var other in otherOffers)
        {
            other.Status = OfferStatus.Rechazada;
            await _offerRepository.UpdateAsync(other);
        }
    }

    public async Task RejectOfferAsync(int offerId, string agentId)
    {
        var allOffers = await _offerRepository.GetAllAsync();
        var offer = allOffers.FirstOrDefault(o => o.Id == offerId);
        if (offer == null || offer.Status != OfferStatus.Pendiente)
        {
            throw new InvalidOperationException("La oferta no existe o ya ha sido procesada.");
        }

        var property = await _propertyRepository.GetByIdWithIncludeAsync(offer.PropertyId);
        if (property == null || property.AgentId != agentId)
        {
            throw new UnauthorizedAccessException("No tiene autorización para modificar ofertas de esta propiedad.");
        }

        offer.Status = OfferStatus.Rechazada;
        await _offerRepository.UpdateAsync(offer);
    }
}
