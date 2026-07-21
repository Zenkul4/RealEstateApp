using RealEstateApp.Core.Application.DTOs.Email;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default);
}
