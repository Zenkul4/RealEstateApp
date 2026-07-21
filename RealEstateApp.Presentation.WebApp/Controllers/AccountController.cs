using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.DTOs.Email;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;

using Microsoft.Extensions.Logging;

namespace RealEstateApp.Presentation.WebApp.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAccountService accountService,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToRoleHome();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(viewModel);

        var response = await _accountService.SignInWebAppAsync(new AuthenticationRequest
        {
            EmailOrUserName = viewModel.EmailOrUserName.Trim(),
            Password = viewModel.Password
        }, viewModel.RememberMe);

        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Error!);
            return View(viewModel);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return LocalRedirect(viewModel.ReturnUrl);
        }

        return RedirectToRoleHome(response.Roles);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutWebAppAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.UserType is not (UserType.Client or UserType.Agent))
        {
            ModelState.AddModelError(nameof(viewModel.UserType), "Solo puedes registrarte como cliente o agente.");
        }

        if (!ModelState.IsValid) return View(viewModel);

        string photoUrl;
        try
        {
            photoUrl = await _fileStorageService.SaveProfileImageAsync(viewModel.Photo!, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(viewModel.Photo), exception.Message);
            return View(viewModel);
        }

        var origin = $"{Request.Scheme}://{Request.Host}";
        _logger.LogInformation("[DEBUG_CONTROLLER] POST Register iniciado para {Email}. Origen generado: {Origin}", viewModel.Email, origin);

        var response = await _accountService.RegisterBasicUserAsync(new RegisterRequest
        {
            FirstName = viewModel.FirstName.Trim(),
            LastName = viewModel.LastName.Trim(),
            Cedula = viewModel.Cedula.Trim(),
            Email = viewModel.Email.Trim(),
            UserName = viewModel.UserName.Trim(),
            Phone = viewModel.Phone.Trim(),
            Password = viewModel.Password,
            ConfirmPassword = viewModel.ConfirmPassword,
            PhotoUrl = photoUrl,
            UserType = viewModel.UserType
        }, origin);

        _logger.LogInformation("[DEBUG_CONTROLLER] Respuesta devuelta por RegisterBasicUserAsync -> HasError: {HasError}, UserId: {UserId}, Email: {Email}, HasToken: {HasToken}",
            response.HasError, response.UserId, response.Email, !string.IsNullOrEmpty(response.EmailConfirmationToken));

        if (response.HasError)
        {
            _logger.LogWarning("[DEBUG_CONTROLLER] Error en el registro de usuario: {Error}", response.Error);
            ModelState.AddModelError(string.Empty, response.Error!);
            return View(viewModel);
        }

        if (viewModel.UserType == UserType.Client)
        {
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new
            {
                userId = response.UserId,
                token = response.EmailConfirmationToken
            }, Request.Scheme)!;

            _logger.LogInformation("[DEBUG_CONTROLLER] CallbackUrl generado para activación: {CallbackUrl}", callbackUrl);
            _logger.LogInformation("[DEBUG_CONTROLLER] Llamando a _emailService.SendAsync para enviar correo de activación a {Email}...", response.Email);

            await _emailService.SendAsync(new EmailRequest
            {
                To = response.Email,
                Subject = "Activa tu cuenta de RealEstateApp",
                HtmlBody = BuildActivationEmail(viewModel.FirstName, callbackUrl)
            }, cancellationToken);

            _logger.LogInformation("[DEBUG_CONTROLLER] Envío de correo completado exitosamente sin excepciones.");
        }

        return RedirectToAction(nameof(RegistrationPending), new { accountType = viewModel.UserType.ToString() });
    }

    [AllowAnonymous]
    public IActionResult RegistrationPending(string accountType)
    {
        ViewBag.IsAgent = string.Equals(accountType, UserType.Agent.ToString(), StringComparison.OrdinalIgnoreCase);
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            ViewBag.Success = false;
            return View();
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            ViewBag.Success = await _accountService.ConfirmEmailAsync(userId, decodedToken);
        }
        catch (FormatException)
        {
            ViewBag.Success = false;
        }

        return View();
    }

    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToRoleHome(IEnumerable<string>? roles = null)
    {
        var roleSet = roles?.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet?.Contains("Administrador") == true || User.IsInRole("Administrador"))
            return RedirectToAction("Agents", "Admin");
        if (roleSet?.Contains("Agente") == true || User.IsInRole("Agente"))
            return RedirectToAction("Index", "Property");
        return RedirectToAction("Index", "Home");
    }

    private static string BuildActivationEmail(string firstName, string callbackUrl)
    {
        var safeName = HtmlEncoder.Default.Encode(firstName);
        var safeUrl = HtmlEncoder.Default.Encode(callbackUrl);
        return $$"""
            <div style="font-family:Arial,sans-serif;max-width:620px;margin:auto;color:#17202a">
              <p style="text-transform:uppercase;letter-spacing:2px;color:#9a8043">RealEstateApp</p>
              <h1 style="font-size:30px">Tu próxima propiedad comienza aquí.</h1>
              <p>Hola {{safeName}}, confirma tu correo para activar tu cuenta de cliente.</p>
              <p style="margin:32px 0"><a href="{{safeUrl}}" style="background:#b59a57;color:white;padding:14px 24px;text-decoration:none">Activar mi cuenta</a></p>
              <p style="font-size:13px;color:#64748b">Si no solicitaste esta cuenta, puedes ignorar este mensaje.</p>
            </div>
            """;
    }
}
