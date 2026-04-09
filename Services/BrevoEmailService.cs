using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace PetClinicAPI.Services;

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public BrevoEmailService(IConfiguration config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        var apiKey = _config["Brevo:ApiKey"];
        var senderEmail = _config["Brevo:SenderEmail"];
        var senderName = _config["Brevo:SenderName"];

        var payload = new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email = to } },
            subject = subject,
            htmlContent = body
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[BREVO-DEBUG] Attempting to send email to {to} using sender {senderEmail}");
            Console.WriteLine($"[BREVO-DEBUG] Payload: {JsonSerializer.Serialize(payload)}");
            Console.WriteLine($"[BREVO-DEBUG] Error: {response.StatusCode} - {error}");
            return false;
        }

        var successContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[BREVO-DEBUG] Email sent successfully to {to}. Response: {successContent}");
        return true;
    }
}
