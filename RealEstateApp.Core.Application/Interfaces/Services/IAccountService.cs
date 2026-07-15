using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Account;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IAccountService
{
    Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
    Task<RegisterResponse> RegisterBasicUserAsync(RegisterRequest request);
    Task<RegisterResponse> RegisterAdminOrDeveloperAsync(RegisterRequest request);
}