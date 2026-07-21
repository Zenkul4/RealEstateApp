using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador")]
public class PropertyTypeController : Controller
{
    private readonly IPropertyTypeService _propertyTypeService;

    public PropertyTypeController(IPropertyTypeService propertyTypeService)
    {
        _propertyTypeService = propertyTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _propertyTypeService.GetAllWithInclude();
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

        await _propertyTypeService.Add(vm);
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
            await _propertyTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de propiedad eliminado correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de propiedad ya ha sido eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }
}
