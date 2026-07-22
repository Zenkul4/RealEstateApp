using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Models;

namespace RealEstateApp.Presentation.WebApi.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize(Roles = "Administrador,Desarrollador")]
public class AgentsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IPropertyService _propertyService;

    public AgentsController(IAccountService accountService, IPropertyService propertyService)
    {
        _accountService = accountService;
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var agents = await _accountService.GetUsersByRoleAsync("Agente");
        if (agents.Count == 0) return NoContent();
        var properties = await _propertyService.GetAllWithInclude();
        return Ok(agents.Select(agent => ToResponse(agent, properties.Count(property => property.AgentId == agent.Id))));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var agent = await FindAgentAsync(id);
        if (agent == null) return NotFound(new { Error = "El agente solicitado no existe." });
        var count = (await _propertyService.GetAllWithInclude()).Count(property => property.AgentId == id);
        return Ok(ToResponse(agent, count));
    }

    [HttpGet("{id}/properties")]
    public async Task<IActionResult> GetAgentProperties(string id)
    {
        if (await FindAgentAsync(id) == null) return NotFound(new { Error = "El agente solicitado no existe." });
        var properties = (await _propertyService.GetAllWithInclude()).Where(property => property.AgentId == id).ToList();
        return properties.Count == 0 ? NoContent() : Ok(properties);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ChangeStatus(string id, ChangeAgentStatusRequest request)
    {
        if (request.IsActive is null) return BadRequest(new { Error = "El estado enviado no es válido." });
        if (await FindAgentAsync(id) == null) return NotFound(new { Error = "El agente solicitado no existe." });
        await _accountService.UpdateUserStatusAsync(id, request.IsActive.Value);
        return NoContent();
    }

    private async Task<Core.Application.DTOs.Account.UserDto?> FindAgentAsync(string id)
    {
        var user = await _accountService.GetUserByIdAsync(id);
        return user?.Role == "Agente" ? user : null;
    }

    private static object ToResponse(Core.Application.DTOs.Account.UserDto agent, int propertyCount) => new
    {
        agent.Id,
        agent.FirstName,
        agent.LastName,
        agent.Email,
        agent.Phone,
        IsActive = agent.IsActive,
        PropertyCount = propertyCount
    };
}
