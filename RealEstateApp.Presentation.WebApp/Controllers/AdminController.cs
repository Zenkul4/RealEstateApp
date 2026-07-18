using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces;

namespace RealEstateApp.Presentation.WebApp.Controllers;

public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IPropertyService _propertyService;

    public AdminController(
        IUserService userService,
        IPropertyService propertyService)
    {
        _userService = userService;
        _propertyService = propertyService;
    }

    public async Task<IActionResult> Agents()
    {
        var agents = await _userService.GetAllAgentsAsync();
        var properties = await _propertyService.GetAllWithInclude();

        // Calculate active properties count for each agent
        foreach (var agent in agents)
        {
            agent.ActivePropertiesCount = properties.Count(p => 
                p.AgentId == agent.Id || 
                p.AgentId == agent.Email || 
                p.AgentId == agent.UserName);
        }

        return View(agents);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAgentStatus(string agentId, bool isActive)
    {
        await _userService.UpdateActiveStatusAsync(agentId, isActive);
        return RedirectToAction(nameof(Agents));
    }
}
