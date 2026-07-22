// ServiceRegistration.cs
namespace RealEstateApp.Core.Application;

using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.Mappings;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<GeneralProfile>();
        });

        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyTypeService, PropertyTypeService>();
        services.AddScoped<ISaleTypeService, SaleTypeService>();
        services.AddScoped<IImprovementService, ImprovementService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFavoritePropertyService, FavoritePropertyService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IOfferService, OfferService>();

        return services;
    }
}