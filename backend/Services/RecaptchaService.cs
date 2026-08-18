using System.Text.Json;

namespace LenguajesFormalesAPI.Services;

public interface IRecaptchaService
{
    Task<bool> ValidarAsync(string token);
}

public class RecaptchaService : IRecaptchaService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _environment;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<RecaptchaService> _logger;

    public RecaptchaService(IConfiguration config, IHostEnvironment environment, IHttpClientFactory httpFactory,
        ILogger<RecaptchaService> logger)
    {
        _config     = config;
        _environment = environment;
        _httpFactory = httpFactory;
        _logger     = logger;
    }

    public async Task<bool> ValidarAsync(string token)
    {
        var bypassHabilitado = _environment.IsDevelopment()
            && _config.GetValue<bool>("Recaptcha:BypassInDevelopment");

        if (bypassHabilitado && token == "dev-bypass")
        {
            _logger.LogWarning("Bypass de reCAPTCHA habilitado exclusivamente para desarrollo");
            return true;
        }

        var secretKey = _config["Recaptcha:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("reCAPTCHA SecretKey no configurado");
            return false;
        }

        try
        {
            var client = _httpFactory.CreateClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"]   = secretKey,
                    ["response"] = token
                }));

            var json = await response.Content.ReadAsStringAsync();
            var doc  = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("success").GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando reCAPTCHA");
            return false;
        }
    }
}
