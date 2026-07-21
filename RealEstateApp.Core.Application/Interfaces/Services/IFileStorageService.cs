using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveProfileImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}
