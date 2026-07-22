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
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers;

[Authorize(Roles = "Agente,Agent")]
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
        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var properties = await _propertyService.GetAllWithInclude();
        // Maintenance list shows ALL properties owned by this agent (both Disponible and Vendida)
        var agentProperties = properties.Where(p => p.AgentId == agentId).OrderByDescending(p => p.Id).ToList();
        return View(agentProperties);
    }

    public async Task<IActionResult> Create()
    {
        bool hasCategories = await LoadViewBags();
        if (!hasCategories)
        {
            ViewBag.CategoryMissingError = "Debe haber al menos un Tipo de Propiedad, Tipo de Venta y Mejora registrados antes de crear una propiedad.";
        }

        var vm = new SavePropertyViewModel
        {
            AgentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Code = Random.Shared.Next(100000, 999999).ToString()
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
            return View(new SavePropertyViewModel { AgentId = User.FindFirstValue(ClaimTypes.NameIdentifier)! });
        }

        vm.AgentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (string.IsNullOrWhiteSpace(vm.Code))
        {
            vm.Code = Random.Shared.Next(100000, 999999).ToString();
        }

        bool hasCategories = await LoadViewBags();
        if (!hasCategories)
        {
            ModelState.AddModelError(string.Empty, "Debe haber al menos un Tipo de Propiedad, Tipo de Venta y Mejora registrados antes de crear una propiedad.");
            return View(vm);
        }

        if (vm.SelectedImprovements == null || vm.SelectedImprovements.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos una mejora para la propiedad.");
            return View(vm);
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (!ValidateImageFiles(vm))
        {
            return View(vm);
        }

        // Validación de negocio: Debe cargarse al menos una imagen
        if (vm.ImageOne == null && vm.ImageTwo == null && vm.ImageThree == null && vm.ImageFour == null)
        {
            ModelState.AddModelError(string.Empty, "Debe cargar al menos una imagen de la propiedad (de 1 a 4 imágenes).");
            return View(vm);
        }

        var result = await _propertyService.Add(vm);

        result.ImageOneUrl = UploadFile(vm.ImageOne, result.Id);
        result.ImageTwoUrl = UploadFile(vm.ImageTwo, result.Id);
        result.ImageThreeUrl = UploadFile(vm.ImageThree, result.Id);
        result.ImageFourUrl = UploadFile(vm.ImageFour, result.Id);

        await _propertyService.Update(result, result.Id);
        TempData["SuccessMessage"] = "Propiedad creada exitosamente.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _propertyService.GetByIdSaveViewModel(id);
            string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (vm.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            var property = await _propertyService.GetByIdWithInclude(id);
            if (property.Status == "Vendida")
            {
                TempData["ErrorMessage"] = "No se puede editar una propiedad en estado Vendida.";
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

        string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        vm.AgentId = agentId;

        if (!ModelState.IsValid)
        {
            await LoadViewBags();
            return View(vm);
        }

        if (!ValidateImageFiles(vm))
        {
            await LoadViewBags();
            return View(vm);
        }

        try
        {
            var currentProperty = await _propertyService.GetByIdWithInclude(vm.Id);
            if (currentProperty.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            if (currentProperty.Status == "Vendida")
            {
                TempData["ErrorMessage"] = "No se puede editar una propiedad en estado Vendida.";
                return RedirectToAction(nameof(Index));
            }

            var currentSaveVm = await _propertyService.GetByIdSaveViewModel(vm.Id);

            // Comprobamos si tiene al menos una imagen activa tras el edit
            bool hasImageOne = vm.ImageOne != null || !string.IsNullOrEmpty(currentSaveVm.ImageOneUrl);
            bool hasImageTwo = vm.ImageTwo != null || !string.IsNullOrEmpty(currentSaveVm.ImageTwoUrl);
            bool hasImageThree = vm.ImageThree != null || !string.IsNullOrEmpty(currentSaveVm.ImageThreeUrl);
            bool hasImageFour = vm.ImageFour != null || !string.IsNullOrEmpty(currentSaveVm.ImageFourUrl);

            if (!hasImageOne && !hasImageTwo && !hasImageThree && !hasImageFour)
            {
                ModelState.AddModelError(string.Empty, "La propiedad debe mantener al menos una imagen.");
                await LoadViewBags();
                return View(vm);
            }

            vm.ImageOneUrl = UploadFile(vm.ImageOne, vm.Id, currentSaveVm.ImageOneUrl);
            vm.ImageTwoUrl = UploadFile(vm.ImageTwo, vm.Id, currentSaveVm.ImageTwoUrl);
            vm.ImageThreeUrl = UploadFile(vm.ImageThree, vm.Id, currentSaveVm.ImageThreeUrl);
            vm.ImageFourUrl = UploadFile(vm.ImageFour, vm.Id, currentSaveVm.ImageFourUrl);

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
            var property = await _propertyService.GetByIdWithInclude(id);
            string agentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (property.AgentId != agentId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar esta propiedad.";
                return RedirectToAction(nameof(Index));
            }

            if (property.Status == "Vendida")
            {
                TempData["ErrorMessage"] = "No se puede eliminar una propiedad en estado Vendida.";
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

    private async Task<bool> LoadViewBags()
    {
        var propertyTypes = await _propertyTypeService.GetAllWithInclude();
        var saleTypes = await _saleTypeService.GetAllWithInclude();
        var improvements = await _improvementService.GetAllWithInclude();

        ViewBag.PropertyTypes = new SelectList(propertyTypes, "Id", "Name");
        ViewBag.SaleTypes = new SelectList(saleTypes, "Id", "Name");
        ViewBag.Improvements = improvements;

        return propertyTypes.Count > 0 && saleTypes.Count > 0 && improvements.Count > 0;
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

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private bool ValidateImageFiles(SavePropertyViewModel vm)
    {
        var files = new[] { vm.ImageOne, vm.ImageTwo, vm.ImageThree, vm.ImageFour };
        bool isValid = true;
        foreach (var file in files)
        {
            if (file != null && file.Length > 0)
            {
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty, "Solo se permiten archivos de imagen con extensión .jpg, .jpeg o .png.");
                    isValid = false;
                    break;
                }
            }
        }
        return isValid;
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
