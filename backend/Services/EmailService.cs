using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using LenguajesFormalesAPI.Interfaces;

namespace LenguajesFormalesAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            byte[]? adjunto = null,
            string? nombreAdjunto = null)
        {
            var host = _configuration["EmailSettings:Host"];
            var portText = _configuration["EmailSettings:Port"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var fromName = _configuration["EmailSettings:FromName"];

            if (string.IsNullOrWhiteSpace(host))
                throw new Exception("No se configuró el servidor SMTP.");

            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("No se configuró el correo de envío.");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("No se configuró la contraseña de aplicación.");

            if (!int.TryParse(portText, out int port))
                port = 587;

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    fromName ?? "Lenguajes Formales",
                    username
                )
            );

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var cuerpo = new BodyBuilder { HtmlBody = body };

            if (adjunto is { Length: > 0 })
            {
                cuerpo.Attachments.Add(
                    nombreAdjunto ?? "adjunto.pdf",
                    adjunto,
                    ContentType.Parse("application/pdf")
                );
            }

            email.Body = cuerpo.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                await smtp.ConnectAsync(
                    host,
                    port,
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    username,
                    password
                );

                await smtp.SendAsync(email);
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                }
            }
        }
    }
}