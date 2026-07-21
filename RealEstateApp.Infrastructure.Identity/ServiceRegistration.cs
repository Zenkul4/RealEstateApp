// ServiceRegistration.cs
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Models;
using RealEstateApp.Infrastructure.Identity.Services;

namespace RealEstateApp.Infrastructure.Identity;

public static class ServiceRegistration
{
    public static void AddWebAppIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddIdentityCore(services, configuration);

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });
    }

    public static void AddApiIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddIdentityCore(services, configuration);

        var jwtKey = configuration["JWTSettings:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env == "Development" || string.IsNullOrEmpty(env))
            {
                jwtKey = "SuperSecretKeyForDevelopmentRealEstateAppIdentity";
            }
            else
            {
                throw new InvalidOperationException("JWTSettings:Key debe configurarse mediante User Secrets o variables de entorno.");
            }
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = configuration["JWTSettings:Issuer"],
                ValidAudience = configuration["JWTSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = context => WriteJsonErrorAsync(context.Response, 401, "No está autorizado para acceder a este recurso.", context.HandleResponse),
                OnForbidden = context => WriteJsonErrorAsync(context.Response, 403, "Acceso denegado. No tiene permisos para realizar esta acción.")
            };
        });
    }

    private static void AddIdentityCore(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<IAccountService, AccountService>();
    }

    private static Task WriteJsonErrorAsync(HttpResponse response, int statusCode, string message, Action? beforeWrite = null)
    {
        beforeWrite?.Invoke();
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        return response.WriteAsJsonAsync(new { hasError = true, error = message });
    }
}
