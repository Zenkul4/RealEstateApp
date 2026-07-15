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
        var user = await _userManager.FindByEmailAsync(request.EmailOrUserName) ??
                   await _userManager.FindByNameAsync(request.EmailOrUserName);

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

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWTSettings:DurationInMinutes"])),
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
            HasError = false
        };
    }

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
            // CORRECCIÓN: Conversión explícita del Enum a String
            await _userManager.AddToRoleAsync(user, request.UserType.ToString());
            return new RegisterResponse { HasError = false };
        }

        return new RegisterResponse { HasError = true, Error = "Ha ocurrido un error al registrar el usuario." };
    }
}