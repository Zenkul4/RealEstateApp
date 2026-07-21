using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Cliente")]
public class FavoriteController : Controller
{
    private readonly IFavoritePropertyService _favoritePropertyService;

    public FavoriteController(IFavoritePropertyService favoritePropertyService)
    {
        _favoritePropertyService = favoritePropertyService;
    }

    public async Task<IActionResult> Index()
    {
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy-client-id";
        var favorites = await _favoritePropertyService.GetFavoritesByClientAsync(clientId);
        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int propertyId)
    {
        string clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "dummy-client-id";
        var isFav = await _favoritePropertyService.IsFavoriteAsync(propertyId, clientId);

        if (isFav)
        {
            await _favoritePropertyService.DeleteAsync(propertyId, clientId);
            TempData["SuccessMessage"] = "Propiedad eliminada de favoritos.";
        }
        else
        {
            await _favoritePropertyService.AddAsync(propertyId, clientId);
            TempData["SuccessMessage"] = "Propiedad agregada a favoritos.";
        }

        return RedirectToAction(nameof(Index));
    }
}
