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
        var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SavePropertyTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        await _propertyTypeService.Update(vm, vm.Id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _propertyTypeService.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
