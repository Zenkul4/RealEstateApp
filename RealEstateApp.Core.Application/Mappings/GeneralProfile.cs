// Mappings/GeneralProfile.cs
using AutoMapper;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using RealEstateApp.Core.Application.ViewModels.Properties;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<Property, PropertyViewModel>()
            .ForMember(dest => dest.LandSize, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType.Name))
            .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType.Name))
            .ReverseMap()
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.LandSize))
            .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
            .ForMember(dest => dest.SaleType, opt => opt.Ignore());

        CreateMap<Property, SavePropertyViewModel>()
            .ForMember(dest => dest.LandSize, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.Improvements, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.LandSize))
            .ForMember(dest => dest.Improvements, opt => opt.Ignore())
            .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
            .ForMember(dest => dest.SaleType, opt => opt.Ignore());

        CreateMap<PropertyType, PropertyTypeViewModel>().ReverseMap();
        CreateMap<PropertyType, SavePropertyTypeViewModel>().ReverseMap();
        CreateMap<SaleType, SaleTypeViewModel>().ReverseMap();
        CreateMap<SaleType, SaveSaleTypeViewModel>().ReverseMap();
        CreateMap<Improvement, ImprovementViewModel>().ReverseMap();
        CreateMap<Improvement, SaveImprovementViewModel>().ReverseMap();

        CreateMap<UserDto, UserViewModel>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}