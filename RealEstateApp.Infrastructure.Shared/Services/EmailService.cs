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
        EnsureConfigured();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new BodyBuilder { HtmlBody = request.HtmlBody }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var socketOptions = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Correo enviado correctamente a {To} mediante {Host}:{Port}.",
                request.To, _settings.Host, _settings.Port);
        }
        catch (SmtpCommandException exception)
        {
            _logger.LogError(exception,
                "El servidor SMTP rechazó el correo a {To} con estado {StatusCode}.",
                request.To, exception.StatusCode);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "No fue posible enviar el correo SMTP a {To}.", request.To);
            throw;
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            _settings.Port is <= 0 or > 65535 ||
            string.IsNullOrWhiteSpace(_settings.UserName) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "La configuración SMTP está incompleta. Configura Host, Port, UserName, Password y FromEmail mediante User Secrets o variables de entorno.");
        }
    }
}
