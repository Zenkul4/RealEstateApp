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
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Models;

namespace RealEstateApp.Infrastructure.Identity.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
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

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            };

        claims.AddRange(rolesList.Select(role => new Claim(ClaimTypes.Role, role)));

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSettings:Key"]!));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWTSettings:DurationInMinutes"]));
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
        return await RegisterUserAsync(request, isEmailConfirmed: true);
    }

    private async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, bool isEmailConfirmed)
    {
        if (request.Password != request.ConfirmPassword)
        {
            return new RegisterResponse { HasError = true, Error = "La contraseña y la confirmación no coinciden." };
        }

        var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
        if (userWithSameEmail != null)
        {
            return new RegisterResponse { HasError = true, Error = "Ya existe un usuario registrado con este correo electrónico." };
        }

        var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
        if (userWithSameUserName != null)
        {
            return new RegisterResponse { HasError = true, Error = "Ya existe un usuario registrado con este nombre de usuario." };
        }

        if (_userManager.Users.Any(user => user.Cedula == request.Cedula))
        {
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

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, MapUserTypeToRole(request.UserType));
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new RegisterResponse { HasError = true, Error = "No fue posible asignar el rol seleccionado." };
            }

            var confirmationToken = request.UserType == UserType.Client
                ? await _userManager.GenerateEmailConfirmationTokenAsync(user)
                : string.Empty;

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
