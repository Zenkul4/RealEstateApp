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

namespace RealEstateApp.Presentation.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly ISaleTypeService _saleTypeService;
    private readonly IFavoritePropertyService _favoritePropertyService;

    public HomeController(
        IPropertyService propertyService,
        IPropertyTypeService propertyTypeService,
        ISaleTypeService saleTypeService,
        IFavoritePropertyService favoritePropertyService)
    {
        _propertyService = propertyService;
        _propertyTypeService = propertyTypeService;
        _saleTypeService = saleTypeService;
        _favoritePropertyService = favoritePropertyService;
    }

    public async Task<IActionResult> Index(int? propertyTypeId, decimal? minPrice, decimal? maxPrice, int? rooms, int? bathrooms)
    {
        var properties = await _propertyService.GetAllWithInclude();

        // Apply filters
        if (propertyTypeId.HasValue)
        {
            properties = properties.Where(p => p.PropertyTypeId == propertyTypeId.Value).ToList();
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

        // Get Client Favorites to toggle icons
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy-client-id";
        var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
        ViewBag.FavoriteIds = favorites.Select(f => f.Id).ToList();

        // Load property types for the dropdown filter
        var propertyTypes = await _propertyTypeService.GetAllWithInclude();
        ViewBag.PropertyTypes = new SelectList(propertyTypes, "Id", "Name", propertyTypeId);

        // Keep filter values in ViewBag to display them in the form
        ViewBag.SelectedPropertyTypeId = propertyTypeId;
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
            return View(property);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Favorites()
    {
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy-client-id";
        var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
        return View(favorites);
    }

    [HttpPost]
    [Authorize(Roles = "Cliente")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int propertyId, string returnUrl)
    {
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy-client-id";
        var isFav = await _favoritePropertyService.IsFavoriteAsync(propertyId, clientId);

        if (isFav)
        {
            await _favoritePropertyService.DeleteAsync(propertyId, clientId);
        }
        else
        {
            await _favoritePropertyService.AddAsync(propertyId, clientId);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
