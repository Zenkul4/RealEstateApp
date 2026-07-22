using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("authenticate")]
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(AuthenticationRequest request)
    {
        var response = await _accountService.AuthenticateAsync(request);
        return response.HasError ? Unauthorized(new { response.Error }) : Ok(response);
    }

    [HttpPost("register-developer")]
    [HttpPost("developers")]
    [Authorize(Roles = "Administrador")]
    public Task<IActionResult> RegisterDeveloper(RegisterRequest request) => Register(request, UserType.Developer);

    [HttpPost("register-admin")]
    [HttpPost("administrators")]
    [Authorize(Roles = "Administrador")]
    public Task<IActionResult> RegisterAdministrator(RegisterRequest request) => Register(request, UserType.Administrator);

    private async Task<IActionResult> Register(RegisterRequest request, UserType userType)
    {
        request.UserType = userType;
        var response = await _accountService.RegisterAdminOrDeveloperAsync(request);
        if (response.HasError) return BadRequest(new { response.Error });

        return Created($"/api/account/users/{response.UserId}", new
        {
            response.UserId,
            response.Email,
            Role = userType == UserType.Administrator ? "Administrador" : "Desarrollador"
        });
    }
}
