using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Identity.Client;

namespace BroxDistribution1.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _http;

    public EmailService(IConfiguration configuration, HttpClient http)
    {
        _configuration = configuration;
        _http = http;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var tenantId = _configuration["GraphMail:TenantId"]!;
        var clientId = _configuration["GraphMail:ClientId"]!;
        var clientSecret = _configuration["GraphMail:ClientSecret"]!;
        var senderUpn = _configuration["GraphMail:SenderUpn"]!;

        var app = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();

        // OAuth2 client credentials uses scope https://graph.microsoft.com/.default
        var token = await app
            .AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
            .ExecuteAsync();

        var payload = new
        {
            message = new
            {
                subject = subject,
                body = new { contentType = "HTML", content = message },
                toRecipients = new[]
                {
                    new { emailAddress = new { address = toEmail } }
                }
            },
            saveToSentItems = true
        };

        var url = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(senderUpn)}/sendMail";

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode(); // Graph returns 202 Accepted on success
    }
}
