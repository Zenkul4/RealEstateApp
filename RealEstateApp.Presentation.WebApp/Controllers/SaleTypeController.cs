using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador,Admin")]
public class SaleTypeController : Controller
{
    private readonly ISaleTypeService _saleTypeService;
    private readonly IPropertyService _propertyService;
    private readonly IFileStorageService _fileStorageService;

    public SaleTypeController(
        ISaleTypeService saleTypeService,
        IPropertyService propertyService,
        IFileStorageService fileStorageService)
    {
        _saleTypeService = saleTypeService;
        _propertyService = propertyService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _saleTypeService.GetAllWithInclude();
        var properties = await _propertyService.GetAllWithInclude();

        foreach (var item in list)
        {
            item.PropertiesCount = properties.Count(p => p.SaleTypeId == item.Id);
        }

        return View(list);
    }

    public IActionResult Create()
    {
        return View(new SaveSaleTypeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveSaleTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var list = await _saleTypeService.GetAllWithInclude();
        if (list.Any(x => x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe un tipo de venta registrado con el nombre '{vm.Name}'.");
            return View(vm);
        }

        await _saleTypeService.Add(vm);
        TempData["SuccessMessage"] = "Tipo de venta creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _saleTypeService.GetByIdSaveViewModel(id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de venta solicitado no existe o ya fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveSaleTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var list = await _saleTypeService.GetAllWithInclude();
        if (list.Any(x => x.Id != vm.Id && x.Name.Trim().Equals(vm.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError("Name", $"Ya existe otro tipo de venta registrado con el nombre '{vm.Name}'.");
            return View(vm);
        }

        try
        {
            await _saleTypeService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Tipo de venta actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "No se puede editar un tipo de venta que no existe o ya fue eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var vm = await _saleTypeService.GetByIdSaveViewModel(id);
            var properties = await _propertyService.GetAllWithInclude();
            ViewBag.PropertiesCount = properties.Count(p => p.SaleTypeId == id);
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de venta ya no existe o ya fue eliminado.";
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
            // Cascade delete associated properties & physical images
            var properties = await _propertyService.GetAllWithInclude();
            var linkedProperties = properties.Where(p => p.SaleTypeId == id).ToList();
            foreach (var prop in linkedProperties)
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

            await _saleTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de venta y todas sus propiedades asociadas fueron eliminados correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de venta ya ha sido eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }
}
