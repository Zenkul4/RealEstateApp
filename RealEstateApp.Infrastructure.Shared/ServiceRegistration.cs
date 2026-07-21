using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Shared.Services;
using RealEstateApp.Infrastructure.Shared.Settings;

namespace RealEstateApp.Infrastructure.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.SectionName));
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }
}
