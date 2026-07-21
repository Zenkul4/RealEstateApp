using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RealEstateApp.Core.Application.DTOs.Email;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Shared.Settings;

namespace RealEstateApp.Infrastructure.Shared.Services;

public class EmailService : IEmailService
{
    private readonly MailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<MailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("Configuración SMTP incompleta o en modo prueba. Se omite el envío real del correo.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new BodyBuilder { HtmlBody = request.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || _settings.Host == "smtp.example.com" ||
            string.IsNullOrWhiteSpace(_settings.UserName) || _settings.UserName == "dev_user" ||
            string.IsNullOrWhiteSpace(_settings.Password) || _settings.Password == "dev_password" ||
            string.IsNullOrWhiteSpace(_settings.FromEmail) || _settings.FromEmail == "no-reply@realestateapp.com")
        {
            return false;
        }
        return true;
    }
}
