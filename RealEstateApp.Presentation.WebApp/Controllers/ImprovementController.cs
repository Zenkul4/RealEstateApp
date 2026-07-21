using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador")]
public class ImprovementController : Controller
{
    private readonly IImprovementService _improvementService;

    public ImprovementController(IImprovementService improvementService)
    {
        _improvementService = improvementService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _improvementService.GetAllWithInclude();
        return View(list);
    }

    public IActionResult Create()
    {
        return View(new SaveImprovementViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveImprovementViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        await _improvementService.Add(vm);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _improvementService.GetByIdSaveViewModel(id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La mejora solicitada no existe o ya fue eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveImprovementViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            await _improvementService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Mejora actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "No se puede editar una mejora que no existe o ya fue eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var vm = await _improvementService.GetByIdSaveViewModel(id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La mejora ya no existe o ya fue eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            await _improvementService.Delete(id);
            TempData["SuccessMessage"] = "Mejora eliminada correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La mejora ya ha sido eliminada.";
        }
        return RedirectToAction(nameof(Index));
    }
}
