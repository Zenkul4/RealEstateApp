using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador,Admin")]
public class ImprovementController : Controller
{
    private readonly IImprovementService _improvementService;
    private readonly IPropertyService _propertyService;

    public ImprovementController(
        IImprovementService improvementService,
        IPropertyService propertyService)
    {
        _improvementService = improvementService;
        _propertyService = propertyService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _improvementService.GetAllWithInclude();
        var properties = await _propertyService.GetAllWithInclude();

        foreach (var item in list)
        {
            item.PropertiesCount = properties.Count(p => p.ImprovementNames != null && p.ImprovementNames.Contains(item.Name));
        }

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

        var list = await _improvementService.GetAllWithInclude();
        if (list.Any(x => x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe una mejora registrada con el nombre '{vm.Name}'.");
            return View(vm);
        }

        await _improvementService.Add(vm);
        TempData["SuccessMessage"] = "Mejora creada exitosamente.";
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

        var list = await _improvementService.GetAllWithInclude();
        if (list.Any(x => x.Id != vm.Id && x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe otra mejora registrada con el nombre '{vm.Name}'.");
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
            var properties = await _propertyService.GetAllWithInclude();
            ViewBag.PropertiesCount = properties.Count(p => p.ImprovementNames != null && p.ImprovementNames.Contains(vm.Name));
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
            // Deletes ONLY the improvement entity and its relationship links, DOES NOT delete properties
            await _improvementService.Delete(id);
            TempData["SuccessMessage"] = "Mejora eliminada correctamente. Las propiedades asociadas no fueron afectadas.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La mejora ya ha sido eliminada.";
        }
        return RedirectToAction(nameof(Index));
    }
}
