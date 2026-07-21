using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Properties;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Agente")]
public class PropertyController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly ISaleTypeService _saleTypeService;
    private readonly IImprovementService _improvementService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PropertyController(
        IPropertyService propertyService,
        IPropertyTypeService propertyTypeService,
        ISaleTypeService saleTypeService,
        IImprovementService improvementService,
        IWebHostEnvironment webHostEnvironment)
    {
        _propertyService = propertyService;
        _propertyTypeService = propertyTypeService;
        _saleTypeService = saleTypeService;
        _improvementService = improvementService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        string agentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id";
        
        var properties = await _propertyService.GetAllWithInclude();
        var agentProperties = properties.Where(p => p.AgentId == agentId).ToList();
        return View(agentProperties);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewBags();
        var vm = new SavePropertyViewModel
        {
            AgentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id"
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SavePropertyViewModel vm)
    {
        if (vm == null)
        {
            ModelState.AddModelError(string.Empty, "Los datos de la propiedad son nulos o inválidos.");
            await LoadViewBags();
            return View(new SavePropertyViewModel { AgentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id" });
        }

        vm.AgentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id";

        if (!ModelState.IsValid)
        {
            await LoadViewBags();
            return View(vm);
        }

        // Validación de negocio: Debe cargarse al menos una imagen
        if (vm.ImageOne == null && vm.ImageTwo == null && vm.ImageThree == null && vm.ImageFour == null)
        {
            ModelState.AddModelError(string.Empty, "Debe cargar al menos una imagen de la propiedad.");
            await LoadViewBags();
            return View(vm);
        }

        var result = await _propertyService.Add(vm);

        result.ImageOneUrl = UploadFile(vm.ImageOne, result.Id);
        result.ImageTwoUrl = UploadFile(vm.ImageTwo, result.Id);
        result.ImageThreeUrl = UploadFile(vm.ImageThree, result.Id);
        result.ImageFourUrl = UploadFile(vm.ImageFour, result.Id);

        await _propertyService.Update(result, result.Id);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _propertyService.GetByIdSaveViewModel(id);
            string agentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id";
            if (vm.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }
            await LoadViewBags();
            return View(vm);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La propiedad solicitada no existe o ya fue eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SavePropertyViewModel vm)
    {
        if (vm == null)
        {
            TempData["ErrorMessage"] = "Los datos de la propiedad son nulos o inválidos.";
            return RedirectToAction(nameof(Index));
        }

        string agentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id";
        vm.AgentId = agentId;

        if (!ModelState.IsValid)
        {
            await LoadViewBags();
            return View(vm);
        }

        try
        {
            var currentProperty = await _propertyService.GetByIdSaveViewModel(vm.Id);
            if (currentProperty.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            // Comprobamos si tiene al menos una imagen activa tras el edit
            bool hasImageOne = vm.ImageOne != null || !string.IsNullOrEmpty(currentProperty.ImageOneUrl);
            bool hasImageTwo = vm.ImageTwo != null || !string.IsNullOrEmpty(currentProperty.ImageTwoUrl);
            bool hasImageThree = vm.ImageThree != null || !string.IsNullOrEmpty(currentProperty.ImageThreeUrl);
            bool hasImageFour = vm.ImageFour != null || !string.IsNullOrEmpty(currentProperty.ImageFourUrl);

            if (!hasImageOne && !hasImageTwo && !hasImageThree && !hasImageFour)
            {
                ModelState.AddModelError(string.Empty, "La propiedad debe mantener al menos una imagen.");
                await LoadViewBags();
                return View(vm);
            }

            vm.ImageOneUrl = UploadFile(vm.ImageOne, vm.Id, currentProperty.ImageOneUrl);
            vm.ImageTwoUrl = UploadFile(vm.ImageTwo, vm.Id, currentProperty.ImageTwoUrl);
            vm.ImageThreeUrl = UploadFile(vm.ImageThree, vm.Id, currentProperty.ImageThreeUrl);
            vm.ImageFourUrl = UploadFile(vm.ImageFour, vm.Id, currentProperty.ImageFourUrl);

            await _propertyService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Propiedad actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "No se puede editar una propiedad que no existe o ya fue eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var vm = await _propertyService.GetByIdSaveViewModel(id);
            string agentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "dummy-agent-id";
            if (vm.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            await _propertyService.Delete(id);
            DeletePropertyDirectory(id);
            TempData["SuccessMessage"] = "Propiedad eliminada correctamente.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "La propiedad ya ha sido eliminada o no existe.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadViewBags()
    {
        var propertyTypes = await _propertyTypeService.GetAllWithInclude();
        var saleTypes = await _saleTypeService.GetAllWithInclude();
        var improvements = await _improvementService.GetAllWithInclude();

        ViewBag.PropertyTypes = new SelectList(propertyTypes, "Id", "Name");
        ViewBag.SaleTypes = new SelectList(saleTypes, "Id", "Name");
        ViewBag.Improvements = improvements;
    }

    private string UploadFile(IFormFile? file, int id, string? currentUrl = null)
    {
        if (file == null || file.Length == 0)
        {
            return currentUrl ?? string.Empty;
        }

        // Delete existing file if we are replacing it
        if (!string.IsNullOrEmpty(currentUrl))
        {
            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, currentUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        string basePath = $"/images/properties/{id}";
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties", id.ToString());

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(path, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return $"{basePath}/{fileName}";
    }

    private void DeletePropertyDirectory(int id)
    {
        string path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties", id.ToString());
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
