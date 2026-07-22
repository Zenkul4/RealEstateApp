using System;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.ViewModels.Offer;

public class OfferViewModel
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = null!;
    public decimal Amount { get; set; }
    public OfferStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime Created { get; set; }
}
