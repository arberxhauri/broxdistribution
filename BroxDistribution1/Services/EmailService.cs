using System.Net;
using System.Net.Mail;

namespace BroxDistribution1.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var smtpHost = _configuration["Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Smtp:Port"]);
        var smtpUser = _configuration["Smtp:User"];
        var smtpPass = _configuration["Smtp:Pass"];
        var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]);

        using (var client = new SmtpClient(smtpHost, smtpPort))
        {
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, "Brox Distribution"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}