using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Presentation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Desarrollador")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var properties = await _propertyService.GetAllWithInclude();
        return properties.Count == 0 ? NoContent() : Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0) return BadRequest(new { Error = "El Id enviado no tiene un formato válido." });
        try
        {
            return Ok(await _propertyService.GetByIdWithInclude(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "La propiedad solicitada no existe." });
        }
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        if (code.Length != 6 || !code.All(char.IsDigit))
            return BadRequest(new { Error = "El código debe contener exactamente 6 dígitos." });

        var property = (await _propertyService.GetAllWithInclude())
            .FirstOrDefault(item => item.Code == code);
        return property == null
            ? NotFound(new { Error = "No existe una propiedad registrada con el código enviado." })
            : Ok(property);
    }
}
