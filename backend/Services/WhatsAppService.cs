using System.Net.Http.Json;
using System.Text.RegularExpressions;
using LenguajesFormalesAPI.Interfaces;

namespace LenguajesFormalesAPI.Services
{
    /// <summary>
    /// Cliente de Evolution API (https://github.com/EvolutionAPI/evolution-api) desplegada en Render.
    /// Configuración (sección "WhatsApp"): BaseUrl, ApiKey, InstanceName, DefaultCountryCode.
    /// Si falta BaseUrl o ApiKey el servicio se desactiva solo: no rompe registro ni login.
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(HttpClient http, IConfiguration config, ILogger<WhatsAppService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public Task<bool> SendTextAsync(string telefono, string mensaje) =>
            EnviarAsync("sendText", telefono, numero => new { number = numero, text = mensaje });

        public Task<bool> SendPdfAsync(string telefono, byte[] pdf, string nombreArchivo, string? leyenda = null) =>
            EnviarAsync("sendMedia", telefono, numero => new
            {
                number    = numero,
                mediatype = "document",
                mimetype  = "application/pdf",
                caption   = leyenda ?? string.Empty,
                fileName  = nombreArchivo,
                media     = Convert.ToBase64String(pdf)
            });

        private async Task<bool> EnviarAsync(string accion, string telefono, Func<string, object> cuerpo)
        {
            var baseUrl  = _config["WhatsApp:BaseUrl"]?.TrimEnd('/');
            var apiKey   = _config["WhatsApp:ApiKey"];
            var instance = _config["WhatsApp:InstanceName"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(instance))
            {
                _logger.LogWarning("WhatsApp no configurado (WhatsApp:BaseUrl / ApiKey / InstanceName); mensaje omitido.");
                return false;
            }

            var numero = NormalizarNumero(telefono, _config["WhatsApp:DefaultCountryCode"] ?? "502");
            if (numero == null)
            {
                _logger.LogWarning("Teléfono inválido para WhatsApp: {Telefono}", telefono);
                return false;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/message/{accion}/{Uri.EscapeDataString(instance)}")
            {
                Content = JsonContent.Create(cuerpo(numero))
            };
            request.Headers.Add("apikey", apiKey);

            try
            {
                // Render (plan gratis) duerme el servicio tras inactividad: el primer envío puede tardar ~1 min.
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                using var response = await _http.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode) return true;

                var detalle = await response.Content.ReadAsStringAsync();
                _logger.LogError("Evolution API respondió {Status}: {Detalle}", (int)response.StatusCode, detalle);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo contactar a Evolution API");
                return false;
            }
        }

        /// <summary>Deja solo dígitos con código de país (sin "+"). 8 dígitos se asumen del país por defecto.</summary>
        internal static string? NormalizarNumero(string telefono, string codigoPais)
        {
            var digitos = Regex.Replace(telefono ?? string.Empty, @"\D", "");
            if (digitos.StartsWith("00")) digitos = digitos[2..];
            if (digitos.Length == 8) digitos = codigoPais + digitos;
            return digitos.Length is >= 10 and <= 15 ? digitos : null;
        }
    }
}
