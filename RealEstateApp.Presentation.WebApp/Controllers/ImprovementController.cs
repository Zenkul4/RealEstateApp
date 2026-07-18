using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

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
        var vm = await _improvementService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveImprovementViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        await _improvementService.Update(vm, vm.Id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _improvementService.GetByIdSaveViewModel(id);
        return View(vm);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _improvementService.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
