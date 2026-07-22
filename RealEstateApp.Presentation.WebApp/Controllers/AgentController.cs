using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Application.ViewModels.Message;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Agente,Agent")]
public class AgentController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IOfferService _offerService;
    private readonly IMessageService _messageService;
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AgentController(
        IPropertyService propertyService,
        IOfferService offerService,
        IMessageService messageService,
        IAccountService accountService,
        IUserService userService,
        IWebHostEnvironment webHostEnvironment)
    {
        _propertyService = propertyService;
        _offerService = offerService;
        _messageService = messageService;
        _accountService = accountService;
        _userService = userService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var properties = await _propertyService.GetAllWithInclude();
        
        // Agent Isolation: Only properties belonging to this agent (both Disponible and Vendida)
        var agentProperties = properties.Where(p => p.AgentId == agentId).OrderByDescending(p => p.Id).ToList();

        return View(agentProperties);
    }

    public async Task<IActionResult> PropertyDetails(int id)
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var property = await _propertyService.GetByIdWithInclude(id);
        if (property == null || property.AgentId != agentId)
        {
            TempData["ErrorMessage"] = "No tiene permisos para ver los detalles de esta propiedad.";
            return RedirectToAction(nameof(Index));
        }

        // Get Client IDs who messaged
        var clientIds = await _messageService.GetClientIdsWhoMessagedPropertyAsync(id, agentId);
        var clientsWhoMessaged = new System.Collections.Generic.List<RealEstateApp.Core.Application.ViewModels.Account.UserViewModel>();
        foreach (var clientId in clientIds)
        {
            var c = await _userService.GetByIdAsync(clientId);
            if (c != null) clientsWhoMessaged.Add(c);
        }

        // Get Offers
        var offers = await _offerService.GetOffersByPropertyAsync(id);
        var offersWithClients = new System.Collections.Generic.List<(RealEstateApp.Core.Application.ViewModels.Offer.OfferViewModel Offer, string ClientName, string ClientEmail)>();
        foreach (var offer in offers)
        {
            var client = await _userService.GetByIdAsync(offer.ClientId);
            string clientName = client != null ? client.FullName : "Cliente";
            string clientEmail = client != null ? client.Email : offer.ClientId;
            offersWithClients.Add((offer, clientName, clientEmail));
        }

        ViewBag.ClientsWhoMessaged = clientsWhoMessaged;
        ViewBag.OffersWithClients = offersWithClients;

        return View(property);
    }

    public async Task<IActionResult> Chat(int propertyId, string clientId)
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var property = await _propertyService.GetByIdWithInclude(propertyId);
        if (property == null || property.AgentId != agentId)
        {
            TempData["ErrorMessage"] = "No tiene permisos para ver este chat.";
            return RedirectToAction(nameof(Index));
        }

        var client = await _userService.GetByIdAsync(clientId);
        if (client == null)
        {
            TempData["ErrorMessage"] = "El cliente solicitado no existe.";
            return RedirectToAction(nameof(PropertyDetails), new { id = propertyId });
        }

        var messages = await _messageService.GetConversationAsync(propertyId, clientId, agentId);

        ViewBag.Property = property;
        ViewBag.Client = client;

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(int propertyId, string clientId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "El mensaje no puede estar vacío.";
            return RedirectToAction(nameof(Chat), new { propertyId, clientId });
        }

        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var property = await _propertyService.GetByIdWithInclude(propertyId);
        if (property == null || property.AgentId != agentId)
        {
            TempData["ErrorMessage"] = "No tiene permisos para responder en este chat.";
            return RedirectToAction(nameof(Index));
        }

        await _messageService.AddAsync(new SaveMessageViewModel
        {
            PropertyId = propertyId,
            ClientId = clientId,
            AgentId = agentId,
            Content = content.Trim()
        }, agentId);

        TempData["SuccessMessage"] = "Respuesta enviada correctamente.";
        return RedirectToAction(nameof(Chat), new { propertyId, clientId });
    }

    public async Task<IActionResult> Offers(int propertyId)
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var property = await _propertyService.GetByIdWithInclude(propertyId);
        if (property == null || property.AgentId != agentId)
        {
            TempData["ErrorMessage"] = "No tiene permisos para administrar ofertas de esta propiedad.";
            return RedirectToAction(nameof(Index));
        }

        var offers = await _offerService.GetOffersByPropertyAsync(propertyId);
        var offersWithClients = new System.Collections.Generic.List<(RealEstateApp.Core.Application.ViewModels.Offer.OfferViewModel Offer, string ClientName, string ClientEmail)>();
        foreach (var offer in offers)
        {
            var client = await _userService.GetByIdAsync(offer.ClientId);
            string clientName = client != null ? client.FullName : "Cliente";
            string clientEmail = client != null ? client.Email : offer.ClientId;
            offersWithClients.Add((offer, clientName, clientEmail));
        }

        ViewBag.Property = property;
        return View(offersWithClients);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptOffer(int offerId, int propertyId)
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _offerService.AcceptOfferAsync(offerId, agentId);
            TempData["SuccessMessage"] = "La oferta fue aceptada exitosamente. La propiedad fue marcada como Vendida y las demás ofertas pendientes fueron rechazadas.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"No fue posible aceptar la oferta: {ex.Message}";
        }

        return RedirectToAction(nameof(Offers), new { propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectOffer(int offerId, int propertyId)
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _offerService.RejectOfferAsync(offerId, agentId);
            TempData["SuccessMessage"] = "La oferta fue rechazada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"No fue posible rechazar la oferta: {ex.Message}";
        }

        return RedirectToAction(nameof(Offers), new { propertyId });
    }

    public async Task<IActionResult> Profile()
    {
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _accountService.GetUserByIdAsync(agentId);
        if (user == null)
        {
            return NotFound();
        }

        var vm = new SaveAgentProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            PhotoUrl = user.PhotoUrl
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(SaveAgentProfileViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        vm.Id = agentId;

        string? photoUrl = vm.PhotoUrl;
        if (vm.Photo != null && vm.Photo.Length > 0)
        {
            photoUrl = UploadProfilePhoto(vm.Photo, agentId, vm.PhotoUrl);
        }

        await _accountService.UpdateUserProfileAsync(agentId, vm.FirstName.Trim(), vm.LastName.Trim(), vm.Phone.Trim(), photoUrl);
        TempData["SuccessMessage"] = "Perfil actualizado correctamente.";

        return RedirectToAction(nameof(Profile));
    }

    private string UploadProfilePhoto(IFormFile file, string userId, string? currentUrl = null)
    {
        if (!string.IsNullOrEmpty(currentUrl))
        {
            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, currentUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        string basePath = $"/images/agents/{userId}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "agents", userId);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(path, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return $"{basePath}/{fileName}";
    }
}
