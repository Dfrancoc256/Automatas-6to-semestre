using System.Text.Json;

namespace LenguajesFormalesAPI.Services;

public interface IRecaptchaService
{
    Task<bool> ValidarAsync(string token, string accionEsperada);
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

    public async Task<bool> ValidarAsync(string token, string accionEsperada)
    {
        var bypassHabilitado = _environment.IsDevelopment()
            && _config.GetValue<bool>("Recaptcha:BypassInDevelopment");

        if (bypassHabilitado && token == "dev-bypass")
        {
            _logger.LogWarning("Bypass de reCAPTCHA habilitado exclusivamente para desarrollo");
            return true;
        }

        var secretKey = _config["Recaptcha:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError("reCAPTCHA SecretKey no configurado");
            return false;
        }

        try
        {
            var client = _httpFactory.CreateClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"]   = secretKey,
                    ["response"] = token
                }), timeout.Token);

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync(timeout.Token);
            using var doc = JsonDocument.Parse(json);
            var resultado = doc.RootElement;
            if (!resultado.TryGetProperty("success", out var success) || success.ValueKind != JsonValueKind.True
                || !resultado.TryGetProperty("score", out var score) || !score.TryGetDouble(out var puntuacion)
                || !resultado.TryGetProperty("action", out var action) || action.GetString() != accionEsperada
                || !resultado.TryGetProperty("hostname", out var hostname))
            {
                _logger.LogWarning("Respuesta de reCAPTCHA v3 incompleta o con acción incorrecta para {Accion}", accionEsperada);
                return false;
            }

            var dominio = hostname.GetString();
            var dominiosPermitidos = _config.GetSection("Recaptcha:AllowedHostnames").Get<string[]>() ?? [];
            if (dominiosPermitidos.Length == 0)
            {
                if (Uri.TryCreate(_config["App:PublicUrl"], UriKind.Absolute, out var urlPublica))
                    dominiosPermitidos = [urlPublica.Host];
                if (_environment.IsDevelopment())
                    dominiosPermitidos = [.. dominiosPermitidos, "localhost", "127.0.0.1"];
            }

            var minimo = Math.Clamp(_config.GetValue("Recaptcha:MinScore", 0.5), 0, 1);
            var dominioValido = dominiosPermitidos.Contains(dominio, StringComparer.OrdinalIgnoreCase);
            if (!dominioValido || puntuacion < minimo)
            {
                _logger.LogWarning("reCAPTCHA v3 rechazado para {Accion}: puntuación {Puntuacion}, dominio {Dominio}",
                    accionEsperada, puntuacion, dominio);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando reCAPTCHA");
            return false;
        }
    }
}
