using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Cliente,Client")]
public class FavoriteController : Controller
{
    private readonly IFavoritePropertyService _favoritePropertyService;

    public FavoriteController(IFavoritePropertyService favoritePropertyService)
    {
        _favoritePropertyService = favoritePropertyService;
    }

    public async Task<IActionResult> Index()
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int propertyId)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFavorite(int propertyId)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _favoritePropertyService.AddAsync(propertyId, clientId);
        TempData["SuccessMessage"] = "La propiedad fue agregada a sus favoritas correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFavorite(int propertyId)
    {
        string clientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _favoritePropertyService.DeleteAsync(propertyId, clientId);
        TempData["SuccessMessage"] = "La propiedad fue eliminada de sus favoritas correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
