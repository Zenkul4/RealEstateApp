using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador,Admin")]
public class PropertyTypeController : Controller
{
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly IPropertyService _propertyService;

    public PropertyTypeController(
        IPropertyTypeService propertyTypeService,
        IPropertyService propertyService)
    {
        _propertyTypeService = propertyTypeService;
        _propertyService = propertyService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _propertyTypeService.GetAllWithInclude();
        var properties = await _propertyService.GetAllWithInclude();

        foreach (var item in list)
        {
            item.PropertiesCount = properties.Count(p => p.PropertyTypeId == item.Id);
        }

        return View(list);
    }

    public IActionResult Create()
    {
        return View(new SavePropertyTypeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SavePropertyTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var list = await _propertyTypeService.GetAllWithInclude();
        if (list.Any(x => x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe un tipo de propiedad registrado con el nombre '{vm.Name}'.");
            return View(vm);
        }

        await _propertyTypeService.Add(vm);
        TempData["SuccessMessage"] = "Tipo de propiedad creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de propiedad solicitado no existe o ya fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SavePropertyTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var list = await _propertyTypeService.GetAllWithInclude();
        if (list.Any(x => x.Id != vm.Id && x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe otro tipo de propiedad registrado con el nombre '{vm.Name}'.");
            return View(vm);
        }

        try
        {
            await _propertyTypeService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Tipo de propiedad actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "No se puede editar un tipo de propiedad que no existe o ya fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
            var properties = await _propertyService.GetAllWithInclude();
            ViewBag.PropertiesCount = properties.Count(p => p.PropertyTypeId == id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de propiedad ya no existe o ya fue eliminado.";
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
            // Cascade delete associated properties
            var properties = await _propertyService.GetAllWithInclude();
            var linkedProperties = properties.Where(p => p.PropertyTypeId == id).ToList();
            foreach (var prop in linkedProperties)
            {
                await _propertyService.Delete(prop.Id);
            }

            await _propertyTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de propiedad y todas sus propiedades asociadas fueron eliminados correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de propiedad ya ha sido eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }
}
