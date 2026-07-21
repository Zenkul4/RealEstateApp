using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

public class SaleTypeController : Controller
{
    private readonly ISaleTypeService _saleTypeService;

    public SaleTypeController(ISaleTypeService saleTypeService)
    {
        _saleTypeService = saleTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _saleTypeService.GetAllWithInclude();
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

        await _saleTypeService.Add(vm);
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
            await _saleTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de venta eliminado correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "El tipo de venta ya ha sido eliminado.";
        }
        return RedirectToAction(nameof(Index));
    }
}
