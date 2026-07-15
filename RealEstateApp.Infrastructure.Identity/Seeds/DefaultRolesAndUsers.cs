// Seeds/DefaultRolesAndUsers.cs
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Infrastructure.Identity.Models;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

public static class DefaultRolesAndUsers
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("Administrador"));
        await roleManager.CreateAsync(new IdentityRole("Cliente"));
        await roleManager.CreateAsync(new IdentityRole("Agente"));
        await roleManager.CreateAsync(new IdentityRole("Desarrollador"));

        var adminUser = new ApplicationUser
        {
            UserName = "adminuser",
            Email = "admin@realestateapp.com",
            FirstName = "Default",
            LastName = "Admin",
            Cedula = "00000000001",
            EmailConfirmed = true,
            PhoneNumber = "8090000000"
        };

        if (userManager.Users.All(u => u.Id != adminUser.Id))
        {
            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(adminUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        var clientUser = new ApplicationUser
        {
            UserName = "clientuser",
            Email = "client@realestateapp.com",
            FirstName = "Default",
            LastName = "Client",
            Cedula = "00000000002",
            EmailConfirmed = true,
            PhoneNumber = "8090000001"
        };

        if (userManager.Users.All(u => u.Id != clientUser.Id))
        {
            var user = await userManager.FindByEmailAsync(clientUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(clientUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(clientUser, "Cliente");
            }
        }

        var agentUser = new ApplicationUser
        {
            UserName = "agentuser",
            Email = "agent@realestateapp.com",
            FirstName = "Default",
            LastName = "Agent",
            Cedula = "00000000003",
            EmailConfirmed = true,
            PhoneNumber = "8090000002"
        };

        if (userManager.Users.All(u => u.Id != agentUser.Id))
        {
            var user = await userManager.FindByEmailAsync(agentUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(agentUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(agentUser, "Agente");
            }
        }

        var devUser = new ApplicationUser
        {
            UserName = "devuser",
            Email = "dev@realestateapp.com",
            FirstName = "Default",
            LastName = "Developer",
            Cedula = "00000000004",
            EmailConfirmed = true,
            PhoneNumber = "8090000003"
        };

        if (userManager.Users.All(u => u.Id != devUser.Id))
        {
            var user = await userManager.FindByEmailAsync(devUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(devUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(devUser, "Desarrollador");
            }
        }
    }
}