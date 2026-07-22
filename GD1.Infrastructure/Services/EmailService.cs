using GD1.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GD1.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var host = _config["Email:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var sender = (_config["Email:SenderEmail"] ?? "").Trim();
            var user = (_config["Email:User"] ?? sender).Trim();
            var name = _config["Email:SenderName"] ?? "GD1";
            var password = (_config["Email:Password"] ?? "").Replace(" ", "");
            var useDevMode = _config.GetValue<bool>("Email:UseDevMode");

            if (useDevMode || string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(password))
            {
                if (useDevMode) _logger.LogInformation("[EMAIL DEV MODE ACTIVE]");
                else _logger.LogWarning("SMTP configuration is incomplete. Skipping email send. Sender: {Sender}", sender);
                
                _logger.LogInformation(
                    "\n" +
                    "************************************************************\n" +
                    "                     [EMAIL DEV OUTPUT]                     \n" +
                    "************************************************************\n" +
                    "TO      : {To}\n" +
                    "SUBJECT : {Subject}\n" +
                    "BODY    :\n{Body}\n" +
                    "************************************************************",
                    to, subject, body);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(name, sender));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                
                // Use Auto to let MailKit decide the best security option
                await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SMTP failure sending to {To} using account {Sender}. Verify Email credentials in appsettings.", to, sender);

                throw new Exception($"SMTP failure (Account: {sender}): {ex.Message}");
            }
        }

        public async Task SendWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentFileName)
        {
            var host = _config["Email:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:Port"] ?? "587");
            var sender = (_config["Email:SenderEmail"] ?? "").Trim();
            var user = (_config["Email:User"] ?? sender).Trim();
            var name = _config["Email:SenderName"] ?? "GD1";
            var password = (_config["Email:Password"] ?? "").Replace(" ", "");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(name, sender));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            bodyBuilder.Attachments.Add(attachmentFileName, attachment, new MimeKit.ContentType("application", "pdf"));
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Agreement PDF emailed successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP failure sending PDF attachment to {To}", to);
                throw new Exception($"SMTP failure: {ex.Message}");
            }
        }
    }
}
