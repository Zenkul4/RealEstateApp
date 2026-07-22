using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services;

public class LocalFileStorageService : IFileStorageService
{
    private const long MaximumFileSize = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveProfileImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaximumFileSize)
        {
            throw new InvalidOperationException("La foto debe pesar menos de 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("La foto debe estar en formato JPG, PNG o WEBP.");
        }

        var folder = Path.Combine(_environment.WebRootPath, "images", "profiles");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absolutePath = Path.Combine(folder, fileName);
        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/images/profiles/{fileName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var normalizedPath = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var webRootFolder = Path.GetFullPath(_environment.WebRootPath);
        var absolutePath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, normalizedPath));

        if (absolutePath.StartsWith(webRootFolder, StringComparison.OrdinalIgnoreCase) && File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }
}
