using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using LenguajesFormalesAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace LenguajesFormalesAPI.Services;

public interface IJwtService
{
    string GenerarToken(Usuario usuario, bool debeCambiarPassword = false);
    string GenerarTokenQr(Usuario usuario);
    string GenerarCodigoQrCredencial(Usuario usuario);
    ClaimsPrincipal? ValidarToken(string token);
    ClaimsPrincipal? ValidarTokenQr(string token);
    ClaimsPrincipal? ValidarCodigoQrCredencial(string token);
    string GenerarTokenRegistroGoogle(string subject, string correo, string nombre);
    ClaimsPrincipal? ValidarTokenRegistroGoogle(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerarToken(Usuario usuario, bool debeCambiarPassword = false)
    {
        var claims = CrearClaimsBase(usuario)
            .Append(new Claim("tipo", "sesion"))
            .Append(new Claim("debe_cambiar_password", debeCambiarPassword ? "true" : "false"));
        return EscribirToken(claims, DateTime.UtcNow.AddHours(8));
    }

    public string GenerarTokenQr(Usuario usuario)
    {
        var dias = Math.Clamp(_config.GetValue("Jwt:QrExpirationDays", 365), 1, 730);
        var claims = CrearClaimsBase(usuario).Append(new Claim("tipo", "credencial_qr"));
        return EscribirToken(claims, DateTime.UtcNow.AddDays(dias));
    }

    public ClaimsPrincipal? ValidarToken(string token) =>
        ValidarTokenInterno(token, "sesion");

    public ClaimsPrincipal? ValidarTokenQr(string token) =>
        ValidarTokenInterno(token, "credencial_qr");

    public string GenerarCodigoQrCredencial(Usuario usuario)
    {
        var dias = Math.Clamp(_config.GetValue("Jwt:QrExpirationDays", 365), 1, 730);
        var vencimiento = DateTimeOffset.UtcNow.AddDays(dias).ToUnixTimeSeconds();
        var contenido = $"{usuario.Id}.{vencimiento}";
        var firma = FirmarCodigoQr(contenido);
        var token = $"{contenido}.{firma}";
        var urlConfigurada = _config["App:PublicUrl"];
        var urlBase = (string.IsNullOrWhiteSpace(urlConfigurada)
            ? "https://analizador-umg.duckdns.org"
            : urlConfigurada).TrimEnd('/');
        return $"{urlBase}/verificar-credencial?token={Uri.EscapeDataString(token)}";
    }

    public ClaimsPrincipal? ValidarCodigoQrCredencial(string token)
    {
        var partes = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length != 3 || !int.TryParse(partes[0], out var usuarioId)
            || !long.TryParse(partes[1], out var vencimiento))
            return null;

        var contenido = $"{partes[0]}.{partes[1]}";
        var firmaEsperada = FirmarCodigoQr(contenido);
        if (!FirmaCoincide(partes[2], firmaEsperada))
            return null;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > vencimiento)
            return null;

        var identidad = new ClaimsIdentity("credencial_qr");
        identidad.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()));
        identidad.AddClaim(new Claim("tipo", "verificacion_credencial"));
        return new ClaimsPrincipal(identidad);
    }

    public string GenerarTokenRegistroGoogle(string subject, string correo, string nombre)
    {
        var claims = new[]
        {
            new Claim("tipo", "registro_google"),
            new Claim("google_subject", subject),
            new Claim(ClaimTypes.Email, correo),
            new Claim(ClaimTypes.GivenName, nombre)
        };
        return EscribirToken(claims, DateTime.UtcNow.AddMinutes(10));
    }

    public ClaimsPrincipal? ValidarTokenRegistroGoogle(string token) =>
        ValidarTokenInterno(token, "registro_google");

    private IEnumerable<Claim> CrearClaimsBase(Usuario usuario) =>
    [
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Email, usuario.Correo),
        new Claim(ClaimTypes.Name, usuario.Nickname),
        new Claim(ClaimTypes.Role, usuario.Rol),
        new Claim("nickname", usuario.Nickname),
        new Claim("foto", usuario.FotoModificada ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    ];

    private string EscribirToken(IEnumerable<Claim> claims, DateTime expiracion)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ObtenerClaveSecreta()));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidarTokenInterno(string token, string tipoEsperado)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ObtenerClaveSecreta()));
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var tokenValidado);

            if (tokenValidado is not JwtSecurityToken jwt
                || !string.Equals(jwt.Header.Alg, SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase)
                || principal.FindFirst("tipo")?.Value != tipoEsperado)
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string ObtenerClaveSecreta()
    {
        var secretKey = _config["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
            throw new InvalidOperationException("JWT SecretKey debe contener al menos 32 bytes");

        return secretKey;
    }

    private string FirmarCodigoQr(string contenido)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ObtenerClaveSecreta()));
        return Base64UrlEncoder.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(contenido)));
    }

    private static bool FirmaCoincide(string recibida, string esperada)
    {
        var izquierda = Encoding.UTF8.GetBytes(recibida);
        var derecha = Encoding.UTF8.GetBytes(esperada);
        return izquierda.Length == derecha.Length
            && CryptographicOperations.FixedTimeEquals(izquierda, derecha);
    }
}
