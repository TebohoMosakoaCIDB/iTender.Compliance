using System.Net;
using System.Net.Mail;
using iTender.Compliance.Application.DTOs;
using iTender.Compliance.Application.Interfaces.Services;
using iTender.Compliance.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace iTender.Compliance.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IOptions<SmtpOptions> options,
            ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            EmailMessageModel message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                _logger.LogWarning(
                    "Smtp:Host is not configured - skipping delivery of '{Subject}' to {To}.",
                    message.Subject,
                    message.ToAddress);

                throw new InvalidOperationException(
                    "Email has not been configured (Smtp:Host is missing).");
            }

            using var mail = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(
                message.ToAddress,
                string.IsNullOrWhiteSpace(message.ToName) ? message.ToAddress : message.ToName));

            if (!string.IsNullOrWhiteSpace(message.CcAddress))
                mail.CC.Add(new MailAddress(message.CcAddress));

            var attachments = new List<Attachment>();

            try
            {
                foreach (var path in message.AttachmentPaths)
                {
                    if (!File.Exists(path))
                        continue;

                    var attachment = new Attachment(path);
                    attachments.Add(attachment);
                    mail.Attachments.Add(attachment);
                }

                using var client = new SmtpClient(
                    _options.Host,
                    _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    Credentials = string.IsNullOrWhiteSpace(_options.Username)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(
                            _options.Username,
                            _options.Password)
                };

                _logger.LogInformation(
                    "Sending email to {To} using SMTP {Host}:{Port}.",
                    message.ToAddress,
                    _options.Host,
                    _options.Port);

                await client.SendMailAsync(
                    mail,
                    cancellationToken);

                _logger.LogInformation(
                    "Email successfully sent to {To}.",
                    message.ToAddress);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(
                    ex,
                    "SMTP error while sending email to {To}. StatusCode: {StatusCode}",
                    message.ToAddress,
                    ex.StatusCode);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while sending email to {To}.",
                    message.ToAddress);

                throw;
            }
            finally
            {
                foreach (var attachment in attachments)
                    attachment.Dispose();
            }
        }
    }
}