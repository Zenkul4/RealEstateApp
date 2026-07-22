using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Presentation.WebApi.Controllers;

[ApiController]
[Route("api/saletypes")]
[Route("api/sale-types")]
[Authorize(Roles = "Administrador,Desarrollador")]
public class SaleTypesController : ControllerBase
{
    private readonly ISaleTypeService _service;
    public SaleTypesController(ISaleTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await _service.GetAllWithInclude();
        return items.Count == 0 ? NoContent() : Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0) return BadRequest(new { Error = "El Id enviado no tiene un formato válido." });
        try { return Ok(await _service.GetByIdSaveViewModel(id)); }
        catch (KeyNotFoundException) { return NotFound(new { Error = "El tipo de venta solicitado no existe." }); }
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(SaveSaleTypeViewModel model)
    {
        if (await NameExists(model.Name)) return BadRequest(new { Error = "Ya existe un tipo de venta con este nombre." });
        var created = await _service.Add(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(int id, SaveSaleTypeViewModel model)
    {
        if ((await _service.GetAllWithInclude()).Any(item => item.Id != id && item.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            return BadRequest(new { Error = "Ya existe otro tipo de venta con este nombre." });
        try { await _service.Update(model, id); return Ok(await _service.GetByIdSaveViewModel(id)); }
        catch (KeyNotFoundException) { return NotFound(new { Error = "El tipo de venta solicitado no existe." }); }
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _service.Delete(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(new { Error = "El tipo de venta solicitado no existe." }); }
    }

    private async Task<bool> NameExists(string name) =>
        (await _service.GetAllWithInclude()).Any(item => item.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
}
