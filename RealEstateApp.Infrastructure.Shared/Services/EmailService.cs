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
        _logger.LogInformation("[DEBUG_EMAIL] Inicio de SendAsync para destinatario {To}", request.To);
        _logger.LogInformation("[DEBUG_EMAIL] Configuración SMTP recibida -> Host: {Host}, Port: {Port}, UseSsl: {UseSsl}, FromEmail: {FromEmail}, FromName: {FromName}, HasUserName: {HasUser}, HasPassword: {HasPass}",
            _settings.Host, _settings.Port, _settings.UseSsl, _settings.FromEmail, _settings.FromName,
            !string.IsNullOrWhiteSpace(_settings.UserName), !string.IsNullOrWhiteSpace(_settings.Password));

        var configured = IsConfigured();
        _logger.LogInformation("[DEBUG_EMAIL] Resultado de IsConfigured(): {Configured}", configured);

        if (!configured)
        {
            _logger.LogWarning("[DEBUG_EMAIL] Configuración SMTP incompleta o en modo prueba. Se omite el envío real del correo.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName ?? "RealEstateApp", _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new BodyBuilder { HtmlBody = request.HtmlBody }.ToMessageBody();

        _logger.LogInformation("[DEBUG_EMAIL] MimeMessage construido exitosamente -> From: {From}, To: {To}, Subject: {Subject}",
            message.From.ToString(), message.To.ToString(), message.Subject);

        try
        {
            using var client = new SmtpClient();
            var socketOptions = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            _logger.LogInformation("[DEBUG_EMAIL] Intentando conectar con client.ConnectAsync({Host}, {Port}, {SocketOptions})...", _settings.Host, _settings.Port, socketOptions);
            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);
            _logger.LogInformation("[DEBUG_EMAIL] Conexión SMTP establecida exitosamente con {Host}:{Port}", _settings.Host, _settings.Port);

            _logger.LogInformation("[DEBUG_EMAIL] Intentando autenticar con client.AuthenticateAsync({UserName})...", _settings.UserName);
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
            _logger.LogInformation("[DEBUG_EMAIL] Autenticación SMTP exitosa para usuario {UserName}", _settings.UserName);

            _logger.LogInformation("[DEBUG_EMAIL] Intentando enviar mensaje con client.SendAsync...");
            await client.SendAsync(message, cancellationToken);
            _logger.LogInformation("[DEBUG_EMAIL] Mensaje enviado exitosamente a {To}", request.To);

            _logger.LogInformation("[DEBUG_EMAIL] Intentando desconectar cliente SMTP...");
            await client.DisconnectAsync(true, cancellationToken);
            _logger.LogInformation("[DEBUG_EMAIL] Desconexión SMTP completada con éxito.");
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {To}. Fallo SMTP Command: {Message} | StatusCode: {StatusCode}", request.To, ex.Message, ex.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo SMTP a {To}", request.To);
            throw;
        }
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
