using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Models;

namespace RealEstateApp.Infrastructure.Identity.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
    {
        var user = await FindUserAsync(request.EmailOrUserName);

        if (user == null)
        {
            return new AuthenticationResponse { HasError = true, Error = "Los datos de acceso son inválidos." };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return new AuthenticationResponse { HasError = true, Error = "Los datos de acceso son inválidos." };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthenticationResponse { HasError = true, Error = "El usuario se encuentra inactivo y no puede iniciar sesión." };
        }

        var rolesList = await _userManager.GetRolesAsync(user);

        if (!rolesList.Any(role => role is "Administrador" or "Desarrollador"))
        {
            return new AuthenticationResponse
            {
                HasError = true,
                Error = "Este usuario no está autorizado para iniciar sesión en la WebAPI."
            };
        }

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            };

        claims.AddRange(rolesList.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = _configuration["JWTSettings:Key"]
            ?? throw new InvalidOperationException(
                "JWTSettings:Key debe configurarse mediante User Secrets o variables de entorno.");
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

        var durationStr = _configuration["JWTSettings:DurationInMinutes"];
        double duration = 60;
        if (double.TryParse(durationStr, out double parsedDuration))
        {
            duration = parsedDuration;
        }
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(duration);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            Issuer = _configuration["JWTSettings:Issuer"],
            Audience = _configuration["JWTSettings:Audience"],
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return new AuthenticationResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = rolesList.ToList(),
            IsVerified = user.EmailConfirmed,
            Token = jwtToken,
            ExpiresAtUtc = expiresAtUtc,
            HasError = false
        };
    }

    public async Task<AuthenticationResponse> SignInWebAppAsync(AuthenticationRequest request, bool rememberMe)
    {
        var user = await FindUserAsync(request.EmailOrUserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Error("Los datos de acceso son inválidos.");
        }

        if (!user.EmailConfirmed)
        {
            return Error("Tu cuenta todavía no está activa. Revisa tu correo o contacta al administrador.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Desarrollador"))
        {
            return Error("Los desarrolladores deben iniciar sesión mediante la WebAPI.");
        }

        await _signInManager.SignInAsync(user, rememberMe);
        return new AuthenticationResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList(),
            IsVerified = true
        };
    }

    public Task SignOutWebAppAsync() => _signInManager.SignOutAsync();

    public async Task<RegisterResponse> RegisterBasicUserAsync(RegisterRequest request)
    {
        return await RegisterUserAsync(request, isEmailConfirmed: false);
    }

    public async Task<RegisterResponse> RegisterAdminOrDeveloperAsync(RegisterRequest request)
    {
        _logger.LogInformation("[DEBUG_ACCOUNT] RegisterAdminOrDeveloperAsync invocado para {Email}", request.Email);
        return await RegisterUserAsync(request, isEmailConfirmed: true);
    }

    private async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, bool isEmailConfirmed)
    {
        _logger.LogInformation("Iniciando registro para {Email} con tipo {UserType}.", request.Email, request.UserType);

        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("[DEBUG_ACCOUNT] Fallo de validación: Contraseña y confirmación no coinciden.");
            return new RegisterResponse { HasError = true, Error = "La contraseña y la confirmación no coinciden." };
        }

        var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithSameEmail != null)
        {
            _logger.LogWarning("[DEBUG_ACCOUNT] Fallo de registro: Ya existe usuario con correo {Email}", request.Email);
            return new RegisterResponse { HasError = true, Error = "Ya existe un usuario registrado con este correo electrónico." };
        }

        var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
        if (userWithSameUserName != null)
        {
            _logger.LogWarning("[DEBUG_ACCOUNT] Fallo de registro: Ya existe usuario con UserName {UserName}", request.UserName);
            return new RegisterResponse { HasError = true, Error = "Ya existe un usuario registrado con este nombre de usuario." };
        }

        if (_userManager.Users.Any(user => user.Cedula == request.Cedula))
        {
            _logger.LogWarning("[DEBUG_ACCOUNT] Fallo de registro: Ya existe usuario con Cédula {Cedula}", request.Cedula);
            return new RegisterResponse { HasError = true, Error = "Ya existe un usuario registrado con esta cédula." };
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Cedula = request.Cedula,
            PhotoUrl = request.PhotoUrl,
            EmailConfirmed = isEmailConfirmed,
            PhoneNumber = request.Phone
        };

        _logger.LogInformation("[DEBUG_ACCOUNT] Creando usuario en Identity: {UserName} ({Email})...", user.UserName, user.Email);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("[DEBUG_ACCOUNT] Usuario creado exitosamente con UserId: {UserId}", user.Id);
            var roleName = MapUserTypeToRole(request.UserType);
            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                _logger.LogError("[DEBUG_ACCOUNT] Error asignando rol {Role} al usuario {UserId}", roleName, user.Id);
                await _userManager.DeleteAsync(user);
                return new RegisterResponse { HasError = true, Error = "No fue posible asignar el rol seleccionado." };
            }
            _logger.LogInformation("[DEBUG_ACCOUNT] Rol {Role} asignado correctamente.", roleName);

            var confirmationToken = string.Empty;
            if (request.UserType == UserType.Client)
            {
                _logger.LogInformation("[DEBUG_ACCOUNT] Generando token de confirmación de email con GenerateEmailConfirmationTokenAsync...");
                var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("[DEBUG_ACCOUNT] Raw Token generado exitosamente (Length: {Length})", rawToken.Length);

                confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
                _logger.LogInformation("Token de confirmación generado para el usuario {UserId}.", user.Id);
            }

            return new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                EmailConfirmationToken = confirmationToken
            };
        }

        return new RegisterResponse
        {
            HasError = true,
            Error = string.Join(" ", result.Errors.Select(error => error.Description))
        };
    }

    private static string MapUserTypeToRole(UserType userType)
    {
        return userType switch
        {
            UserType.Administrator => "Administrador",
            UserType.Client => "Cliente",
            UserType.Agent => "Agente",
            UserType.Developer => "Desarrollador",
            _ => userType.ToString()
        };
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"No fue posible revertir el usuario: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    private Task<ApplicationUser?> FindUserAsync(string emailOrUserName) =>
        emailOrUserName.Contains('@')
            ? _userManager.FindByEmailAsync(emailOrUserName)
            : _userManager.FindByNameAsync(emailOrUserName);

    private static AuthenticationResponse Error(string message) => new()
    {
        HasError = true,
        Error = message
    };

    public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        var userDtos = new List<UserDto>();
        foreach (var user in usersInRole)
        {
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Phone = user.PhoneNumber ?? string.Empty,
                Cedula = user.Cedula,
                PhotoUrl = user.PhotoUrl,
                Role = role,
                IsActive = user.EmailConfirmed
            });
        }
        return userDtos;
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null!;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            Phone = user.PhoneNumber ?? string.Empty,
            Cedula = user.Cedula,
            PhotoUrl = user.PhotoUrl,
            Role = role,
            IsActive = user.EmailConfirmed
        };
    }

    public async Task UpdateUserStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.EmailConfirmed = isActive;
            await _userManager.UpdateAsync(user);
        }
    }
}
