using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IOfferService
{
    Task<List<OfferViewModel>> GetOffersByClientAndPropertyAsync(string clientId, int propertyId);
    Task<bool> HasPendingOfferAsync(string clientId, int propertyId);
    Task<bool> HasAcceptedOfferAsync(int propertyId);
    Task AddAsync(SaveOfferViewModel vm);
    Task<List<OfferViewModel>> GetOffersByPropertyAsync(int propertyId);
    Task AcceptOfferAsync(int offerId, string agentId);
    Task RejectOfferAsync(int offerId, string agentId);
}
