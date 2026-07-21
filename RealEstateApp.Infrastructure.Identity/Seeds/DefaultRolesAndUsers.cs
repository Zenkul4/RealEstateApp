using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Infrastructure.Identity.Models;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

public static class DefaultRolesAndUsers
{
    private static readonly string[] Roles =
    {
        "Administrador", "Cliente", "Agente", "Desarrollador"
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)), $"crear el rol {role}");
            }
        }

        await EnsureUserAsync(userManager, configuration, "Admin", "Administrador", "adminuser",
            "admin@realestateapp.com", "Administrador", "Principal", "00000000001", "8090000000");
        await EnsureUserAsync(userManager, configuration, "Developer", "Desarrollador", "devuser",
            "dev@realestateapp.com", "Desarrollador", "Principal", "00000000004", "8090000003");
        await EnsureUserAsync(userManager, configuration, "Client", "Cliente", "clientuser",
            "client@realestateapp.com", "Cliente", "Demo", "00000000002", "8090000001");
        await EnsureUserAsync(userManager, configuration, "Agent", "Agente", "agentuser",
            "agent@realestateapp.com", "Agente", "Demo", "00000000003", "8090000002");
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        string configurationName,
        string role,
        string defaultUserName,
        string defaultEmail,
        string firstName,
        string lastName,
        string cedula,
        string phone)
    {
        var section = configuration.GetSection($"SeedUsers:{configurationName}");
        var email = section["Email"] ?? defaultEmail;
        var userName = section["UserName"] ?? defaultUserName;
        var password = section["Password"];

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env == "Development" || string.IsNullOrEmpty(env))
                {
                    password = "Admin123!*";
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Configura SeedUsers:{configurationName}:Password mediante User Secrets o una variable de entorno.");
                }
            }

            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Cedula = cedula,
                PhoneNumber = phone,
                EmailConfirmed = true
            };

            EnsureSucceeded(await userManager.CreateAsync(user, password), $"crear el usuario {userName}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), $"asignar el rol {role} a {userName}");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded) return;
        throw new InvalidOperationException(
            $"No fue posible {operation}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }
}
