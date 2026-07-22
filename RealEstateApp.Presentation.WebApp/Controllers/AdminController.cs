using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Application.ViewModels.Admin;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador,Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;
    private readonly IAccountService _accountService;
    private readonly IFileStorageService _fileStorageService;

    public AdminController(
        IUserService userService,
        IPropertyService propertyService,
        IAccountService accountService,
        IFileStorageService fileStorageService)
    {
        _userService = userService;
        _propertyService = propertyService;
        _accountService = accountService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var properties = await _propertyService.GetAllWithInclude();

        var agents = await GetUsersByRoleFlexibleAsync("Agente", "Agent");
        var clients = await GetUsersByRoleFlexibleAsync("Cliente", "Client");
        var developers = await GetUsersByRoleFlexibleAsync("Desarrollador", "Developer");

        ViewBag.AvailableProperties = properties.Count(p => p.Status == "Disponible");
        ViewBag.SoldProperties = properties.Count(p => p.Status == "Vendida");

        ViewBag.ActiveAgents = agents.Count(a => a.IsActive);
        ViewBag.InactiveAgents = agents.Count(a => !a.IsActive);

        ViewBag.ActiveClients = clients.Count(c => c.IsActive);
        ViewBag.InactiveClients = clients.Count(c => !c.IsActive);

        ViewBag.ActiveDevelopers = developers.Count(d => d.IsActive);
        ViewBag.InactiveDevelopers = developers.Count(d => !d.IsActive);

        return View();
    }

    #region Agents Management
    public async Task<IActionResult> Agents()
    {
        var agents = await GetUsersByRoleFlexibleAsync("Agente", "Agent");
        var properties = await _propertyService.GetAllWithInclude();

        foreach (var agent in agents)
        {
            agent.ActivePropertiesCount = properties.Count(p => p.AgentId == agent.Id);
        }

        return View(agents);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAgentStatus(string agentId, bool isActive)
    {
        await _userService.UpdateActiveStatusAsync(agentId, isActive);
        TempData["SuccessMessage"] = $"El estado del agente fue actualizado a {(isActive ? "Activo" : "Inactivo")}.";
        return RedirectToAction(nameof(Agents));
    }

    public async Task<IActionResult> DeleteAgent(string id)
    {
        var agent = await _userService.GetByIdAsync(id);
        if (agent == null)
        {
            TempData["ErrorMessage"] = "El agente no existe.";
            return RedirectToAction(nameof(Agents));
        }

        var properties = await _propertyService.GetAllWithInclude();
        var agentProperties = properties.Where(p => p.AgentId == id).ToList();

        ViewBag.PropertiesCount = agentProperties.Count;
        return View(agent);
    }

    [HttpPost]
    [ActionName("DeleteAgent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAgentPost(string id)
    {
        var agent = await _userService.GetByIdAsync(id);
        if (agent == null)
        {
            TempData["ErrorMessage"] = "El agente ya fue eliminado o no existe.";
            return RedirectToAction(nameof(Agents));
        }

        // Cascade delete agent's properties, physical image files & linked data
        var properties = await _propertyService.GetAllWithInclude();
        var agentProperties = properties.Where(p => p.AgentId == id).ToList();
        foreach (var prop in agentProperties)
        {
            if (prop.ImageUrls != null)
            {
                foreach (var imgUrl in prop.ImageUrls)
                {
                    try { await _fileStorageService.DeleteAsync(imgUrl); } catch { }
                }
            }
            await _propertyService.Delete(prop.Id);
        }

        if (!string.IsNullOrEmpty(agent.PhotoUrl))
        {
            try { await _fileStorageService.DeleteAsync(agent.PhotoUrl); } catch { }
        }

        await _accountService.DeleteUserAsync(id);
        TempData["SuccessMessage"] = "El agente y todas sus propiedades asociadas fueron eliminados correctamente.";
        return RedirectToAction(nameof(Agents));
    }
    #endregion

    #region Administrators Maintenance
    public async Task<IActionResult> Admins()
    {
        var admins = await GetUsersByRoleFlexibleAsync("Administrador", "Admin");
        return View(admins);
    }

    public IActionResult CreateAdmin()
    {
        return View("SaveAdmin", new SaveUserAdminViewModel { Role = "Administrador" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(SaveUserAdminViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Password))
        {
            ModelState.AddModelError("Password", "La contraseña es obligatoria para nuevos administradores.");
        }

        if (!ModelState.IsValid)
        {
            return View("SaveAdmin", vm);
        }

        var request = new RegisterRequest
        {
            FirstName = vm.FirstName.Trim(),
            LastName = vm.LastName.Trim(),
            Cedula = vm.Cedula.Trim(),
            Email = vm.Email.Trim(),
            UserName = vm.UserName.Trim(),
            Password = vm.Password!,
            ConfirmPassword = vm.ConfirmPassword!,
            Phone = "8090000000"
        };

        var response = await _accountService.RegisterAdminOrDeveloperAsync(request);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Error ?? "Error al crear administrador.");
            return View("SaveAdmin", vm);
        }

        TempData["SuccessMessage"] = "Administrador creado exitosamente.";
        return RedirectToAction(nameof(Admins));
    }

    public async Task<IActionResult> EditAdmin(string id)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == currentAdminId)
        {
            TempData["ErrorMessage"] = "No puede editar ni inactivar su propio usuario administrador.";
            return RedirectToAction(nameof(Admins));
        }

        var user = await _accountService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "El administrador no existe.";
            return RedirectToAction(nameof(Admins));
        }

        var vm = new SaveUserAdminViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cedula = user.Cedula,
            Email = user.Email,
            UserName = user.UserName,
            Role = "Administrador"
        };

        return View("SaveAdmin", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdmin(SaveUserAdminViewModel vm)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (vm.Id == currentAdminId)
        {
            TempData["ErrorMessage"] = "No puede editar ni inactivar su propio usuario administrador.";
            return RedirectToAction(nameof(Admins));
        }

        if (!ModelState.IsValid)
        {
            return View("SaveAdmin", vm);
        }

        var response = await _accountService.UpdateUserAdminAsync(vm.Id!, vm.FirstName.Trim(), vm.LastName.Trim(), vm.Cedula.Trim(), vm.Email.Trim(), vm.UserName.Trim(), vm.Password);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Error ?? "Error al actualizar administrador.");
            return View("SaveAdmin", vm);
        }

        TempData["SuccessMessage"] = "Administrador actualizado correctamente.";
        return RedirectToAction(nameof(Admins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminStatus(string adminId, bool isActive)
    {
        var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (adminId == currentAdminId)
        {
            TempData["ErrorMessage"] = "No puede editar ni inactivar su propio usuario administrador.";
            return RedirectToAction(nameof(Admins));
        }

        if (!isActive)
        {
            var admins = await GetUsersByRoleFlexibleAsync("Administrador", "Admin");
            int activeCount = admins.Count(a => a.IsActive);
            if (activeCount <= 1)
            {
                TempData["ErrorMessage"] = "REGLA DE SEGURIDAD: No se puede inactivar este administrador porque el sistema se quedaría sin administradores activos.";
                return RedirectToAction(nameof(Admins));
            }
        }

        await _userService.UpdateActiveStatusAsync(adminId, isActive);
        TempData["SuccessMessage"] = $"El estado del administrador fue actualizado a {(isActive ? "Activo" : "Inactivo")}.";
        return RedirectToAction(nameof(Admins));
    }
    #endregion

    #region Developers Maintenance
    public async Task<IActionResult> Developers()
    {
        var developers = await GetUsersByRoleFlexibleAsync("Desarrollador", "Developer");
        return View(developers);
    }

    public IActionResult CreateDeveloper()
    {
        return View("SaveDeveloper", new SaveUserAdminViewModel { Role = "Desarrollador" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDeveloper(SaveUserAdminViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Password))
        {
            ModelState.AddModelError("Password", "La contraseña es obligatoria para nuevos desarrolladores.");
        }

        if (!ModelState.IsValid)
        {
            return View("SaveDeveloper", vm);
        }

        var request = new RegisterRequest
        {
            FirstName = vm.FirstName.Trim(),
            LastName = vm.LastName.Trim(),
            Cedula = vm.Cedula.Trim(),
            Email = vm.Email.Trim(),
            UserName = vm.UserName.Trim(),
            Password = vm.Password!,
            ConfirmPassword = vm.ConfirmPassword!,
            Phone = "8090000000"
        };

        var response = await _accountService.RegisterAdminOrDeveloperAsync(request);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Error ?? "Error al crear desarrollador.");
            return View("SaveDeveloper", vm);
        }

        TempData["SuccessMessage"] = "Desarrollador creado exitosamente.";
        return RedirectToAction(nameof(Developers));
    }

    public async Task<IActionResult> EditDeveloper(string id)
    {
        var user = await _accountService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "El desarrollador no existe.";
            return RedirectToAction(nameof(Developers));
        }

        var vm = new SaveUserAdminViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cedula = user.Cedula,
            Email = user.Email,
            UserName = user.UserName,
            Role = "Desarrollador"
        };

        return View("SaveDeveloper", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDeveloper(SaveUserAdminViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("SaveDeveloper", vm);
        }

        var response = await _accountService.UpdateUserAdminAsync(vm.Id!, vm.FirstName.Trim(), vm.LastName.Trim(), vm.Cedula.Trim(), vm.Email.Trim(), vm.UserName.Trim(), vm.Password);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Error ?? "Error al actualizar desarrollador.");
            return View("SaveDeveloper", vm);
        }

        TempData["SuccessMessage"] = "Desarrollador actualizado correctamente.";
        return RedirectToAction(nameof(Developers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDeveloperStatus(string developerId, bool isActive)
    {
        await _userService.UpdateActiveStatusAsync(developerId, isActive);
        TempData["SuccessMessage"] = $"El estado del desarrollador fue actualizado a {(isActive ? "Activo" : "Inactivo")}.";
        return RedirectToAction(nameof(Developers));
    }
    #endregion

    private async Task<List<UserViewModel>> GetUsersByRoleFlexibleAsync(string role1, string role2)
    {
        var users = await _userService.GetUsersByRoleAsync(role1);
        if (users.Count == 0 && !string.IsNullOrEmpty(role2))
        {
            users = await _userService.GetUsersByRoleAsync(role2);
        }
        return users;
    }
}
