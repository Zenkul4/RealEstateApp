using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Administrador")]
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
        var vm = await _saleTypeService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveSaleTypeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        await _saleTypeService.Update(vm, vm.Id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _saleTypeService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _saleTypeService.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
