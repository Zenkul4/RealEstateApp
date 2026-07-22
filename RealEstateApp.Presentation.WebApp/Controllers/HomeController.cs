using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApp.Models;
using RealEstateApp.Core.Application.ViewModels.Message;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Presentation.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly ISaleTypeService _saleTypeService;
    private readonly IFavoritePropertyService _favoritePropertyService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IMessageService _messageService;
    private readonly IOfferService _offerService;

    public HomeController(
        IPropertyService propertyService,
        IPropertyTypeService propertyTypeService,
        ISaleTypeService saleTypeService,
        IFavoritePropertyService favoritePropertyService,
        IUserService userService,
        IEmailService emailService,
        IMessageService messageService,
        IOfferService offerService)
    {
        _propertyService = propertyService;
        _propertyTypeService = propertyTypeService;
        _saleTypeService = saleTypeService;
        _favoritePropertyService = favoritePropertyService;
        _userService = userService;
        _emailService = emailService;
        _messageService = messageService;
        _offerService = offerService;
    }

    public async Task<IActionResult> Index(int? propertyTypeId, int? saleTypeId, decimal? minPrice, decimal? maxPrice, int? rooms, int? bathrooms)
    {
        var properties = await _propertyService.GetAllWithInclude();

        if (propertyTypeId.HasValue)
        {
            properties = properties.Where(p => p.PropertyTypeId == propertyTypeId.Value).ToList();
        }
        if (saleTypeId.HasValue)
        {
            properties = properties.Where(p => p.SaleTypeId == saleTypeId.Value).ToList();
        }
        if (minPrice.HasValue)
        {
            properties = properties.Where(p => p.Price >= minPrice.Value).ToList();
        }
        if (maxPrice.HasValue)
        {
            properties = properties.Where(p => p.Price <= maxPrice.Value).ToList();
        }
        if (rooms.HasValue)
        {
            properties = properties.Where(p => p.Rooms == rooms.Value).ToList();
        }
        if (bathrooms.HasValue)
        {
            properties = properties.Where(p => p.Bathrooms == bathrooms.Value).ToList();
        }

        properties = properties.Where(p => 
            string.Equals(p.Status, "Disponible", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(p.Status, "Available", StringComparison.OrdinalIgnoreCase) || 
            p.Status == "0"
        ).ToList();

        try
        {
            var agents = await _userService.GetAllAgentsAsync();
            var activeAgentIds = agents.Where(a => a.IsActive).Select(a => a.Id).ToHashSet();
            properties = properties.Where(p => activeAgentIds.Contains(p.AgentId)).ToList();
        }
        catch { }

        properties = properties.OrderByDescending(p => p.Id).ToList();

        string? clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var favoriteIds = new System.Collections.Generic.List<int>();
        if (!string.IsNullOrEmpty(clientId))
        {
            var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
            favoriteIds = favorites.Select(f => f.Id).ToList();
        }
        ViewBag.FavoriteIds = favoriteIds;

        var propertyTypes = await _propertyTypeService.GetAllWithInclude();
        var saleTypes = await _saleTypeService.GetAllWithInclude();
        ViewBag.PropertyTypes = new SelectList(propertyTypes, "Id", "Name", propertyTypeId);
        ViewBag.SaleTypes = new SelectList(saleTypes, "Id", "Name", saleTypeId);

        ViewBag.SelectedPropertyTypeId = propertyTypeId;
        ViewBag.SelectedSaleTypeId = saleTypeId;
        ViewBag.SelectedMinPrice = minPrice;
        ViewBag.SelectedMaxPrice = maxPrice;
        ViewBag.SelectedRooms = rooms;
        ViewBag.SelectedBathrooms = bathrooms;

        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var property = await _propertyService.GetByIdWithInclude(id);
            var agent = await _userService.GetByIdAsync(property.AgentId);
            if (agent != null)
            {
                ViewBag.AgentEmail = agent.Email;
                ViewBag.AgentName = agent.FullName;
                ViewBag.AgentPhone = agent.Phone;
                ViewBag.AgentPhotoUrl = agent.PhotoUrl;
            }

            ViewBag.IsPropertyAvailable = string.Equals(property.Status, "Disponible", StringComparison.OrdinalIgnoreCase) || string.Equals(property.Status, "Available", StringComparison.OrdinalIgnoreCase) || property.Status == "0";

            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Cliente") || User.IsInRole("Client")))
            {
                string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var chatMessages = await _messageService.GetConversationAsync(id, clientId, property.AgentId);
                ViewBag.ChatMessages = chatMessages;

                var clientOffers = await _offerService.GetOffersByClientAndPropertyAsync(clientId, id);
                ViewBag.ClientOffers = clientOffers;

                ViewBag.HasAcceptedOffer = await _offerService.HasAcceptedOfferAsync(id);
                ViewBag.HasPendingOffer = await _offerService.HasPendingOfferAsync(clientId, id);
            }

            return View(property);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Cliente,Client")]
    public async Task<IActionResult> SendMessage(int propertyId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "El mensaje no puede estar vacío.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        try
        {
            var property = await _propertyService.GetByIdWithInclude(propertyId);
            if (property.Status != "Disponible")
            {
                TempData["ErrorMessage"] = "No se pueden enviar mensajes sobre una propiedad que no esté disponible.";
                return RedirectToAction(nameof(Details), new { id = propertyId });
            }

            string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _messageService.AddAsync(new SaveMessageViewModel
            {
                PropertyId = propertyId,
                ClientId = clientId,
                AgentId = property.AgentId,
                Content = content.Trim()
            }, clientId);

            TempData["SuccessMessage"] = "Mensaje enviado exitosamente al agente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al enviar el mensaje: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Cliente,Client")]
    public async Task<IActionResult> SendOffer(int propertyId, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["ErrorMessage"] = "El monto ofertado debe ser mayor a 0.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        try
        {
            var property = await _propertyService.GetByIdWithInclude(propertyId);
            if (property.Status != "Disponible")
            {
                TempData["ErrorMessage"] = "No se pueden realizar ofertas sobre una propiedad que no esté disponible.";
                return RedirectToAction(nameof(Details), new { id = propertyId });
            }

            string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            bool hasAcceptedOffer = await _offerService.HasAcceptedOfferAsync(propertyId);
            if (hasAcceptedOffer)
            {
                TempData["ErrorMessage"] = "Esta propiedad ya tiene una oferta aceptada.";
                return RedirectToAction(nameof(Details), new { id = propertyId });
            }

            bool hasPendingOffer = await _offerService.HasPendingOfferAsync(clientId, propertyId);
            if (hasPendingOffer)
            {
                TempData["ErrorMessage"] = "Ya tiene una oferta pendiente para esta propiedad.";
                return RedirectToAction(nameof(Details), new { id = propertyId });
            }

            await _offerService.AddAsync(new SaveOfferViewModel
            {
                PropertyId = propertyId,
                ClientId = clientId,
                Amount = amount
            });

            TempData["SuccessMessage"] = "Oferta enviada exitosamente. Se encuentra en estado Pendiente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al realizar la oferta: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Favorites()
    {
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int propertyId, string returnUrl)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!User.IsInRole("Cliente"))
        {
            TempData["ErrorMessage"] = "Solo los clientes pueden añadir propiedades a sus favoritos.";
            return RedirectToAction(nameof(Index));
        }

        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var isFav = await _favoritePropertyService.IsFavoriteAsync(propertyId, clientId);

        if (isFav)
        {
            await _favoritePropertyService.DeleteAsync(propertyId, clientId);
            TempData["SuccessMessage"] = "La propiedad fue eliminada de sus favoritas correctamente.";
        }
        else
        {
            await _favoritePropertyService.AddAsync(propertyId, clientId);
            TempData["SuccessMessage"] = "La propiedad fue agregada a sus favoritas correctamente.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Agents(string name)
    {
        var agents = await _userService.GetAllAgentsAsync();
        
        agents = agents.Where(a => a.IsActive).ToList();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var search = name.Trim().ToLower();
            agents = agents.Where(a => 
                (!string.IsNullOrEmpty(a.FullName) && a.FullName.ToLower().Contains(search))
            ).ToList();
            ViewBag.SearchName = name;
        }

        agents = agents.OrderBy(a => a.FullName).ToList();

        var properties = await _propertyService.GetAllWithInclude();
        foreach (var agent in agents)
        {
            agent.ActivePropertiesCount = properties.Count(p => p.AgentId == agent.Id && p.Status == "Disponible");
        }

        return View(agents);
    }

    public async Task<IActionResult> AgentProperties(string agentId)
    {
        var agent = await _userService.GetByIdAsync(agentId);
        if (agent == null || !agent.IsActive)
        {
            TempData["ErrorMessage"] = "El agente solicitado no existe o no está activo.";
            return RedirectToAction(nameof(Agents));
        }

        var properties = await _propertyService.GetAllWithInclude();
        var agentProperties = properties.Where(p => p.AgentId == agentId && p.Status == "Disponible").ToList();

        ViewBag.AgentName = agent.FullName;
        ViewBag.AgentEmail = agent.Email;
        ViewBag.AgentPhone = agent.Phone;
        ViewBag.AgentPhotoUrl = agent.PhotoUrl;

        return View(agentProperties);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactAgent(int propertyId, string clientName, string clientEmail, string messageText)
    {
        if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(clientEmail) || string.IsNullOrWhiteSpace(messageText))
        {
            TempData["ErrorMessage"] = "Todos los campos del formulario de consulta son obligatorios.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        var property = await _propertyService.GetByIdWithInclude(propertyId);
        var agent = await _userService.GetByIdAsync(property.AgentId);

        if (agent != null)
        {
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e2e8f0; max-width: 600px; margin: auto;'>
                    <h3 style='color: #9a8043;'>Nueva consulta sobre propiedad CÓD: {property.Code}</h3>
                    <hr style='border-color: #f1f5f9;'/>
                    <p><strong>De:</strong> {clientName} ({clientEmail})</p>
                    <p><strong>Mensaje:</strong> {messageText}</p>
                </div>
            ";

            await _emailService.SendAsync(new RealEstateApp.Core.Application.DTOs.Email.EmailRequest
            {
                To = agent.Email,
                Subject = $"Consulta de Propiedad CÓD: {property.Code}",
                HtmlBody = emailBody
            });

            TempData["SuccessMessage"] = "Su consulta ha sido enviada con éxito al asesor.";
        }
        else
        {
            TempData["ErrorMessage"] = "El asesor de esta propiedad no pudo ser encontrado.";
        }

        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeOffer(int propertyId, string clientName, string clientEmail, decimal offerAmount)
    {
        if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(clientEmail) || offerAmount <= 0)
        {
            TempData["ErrorMessage"] = "Todos los campos de la oferta son obligatorios y el monto debe ser mayor que cero.";
            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        var property = await _propertyService.GetByIdWithInclude(propertyId);
        var agent = await _userService.GetByIdAsync(property.AgentId);

        if (agent != null)
        {
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e2e8f0; max-width: 600px; margin: auto;'>
                    <h3 style='color: #9a8043;'>Nueva oferta sobre propiedad CÓD: {property.Code}</h3>
                    <hr style='border-color: #f1f5f9;'/>
                    <p><strong>De:</strong> {clientName} ({clientEmail})</p>
                    <p><strong>Monto Ofertado:</strong> {offerAmount:C} DOP</p>
                </div>
            ";

            await _emailService.SendAsync(new RealEstateApp.Core.Application.DTOs.Email.EmailRequest
            {
                To = agent.Email,
                Subject = $"Oferta de Propiedad CÓD: {property.Code}",
                HtmlBody = emailBody
            });

            TempData["SuccessMessage"] = "Su oferta ha sido enviada con éxito al asesor.";
        }
        else
        {
            TempData["ErrorMessage"] = "El asesor de esta propiedad no pudo ser encontrado.";
        }

        return RedirectToAction(nameof(Details), new { id = propertyId });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
