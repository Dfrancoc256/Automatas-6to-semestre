using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LenguajesFormalesAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace LenguajesFormalesAPI.Services;

public interface IJwtService
{
    string GenerarToken(Usuario usuario);
    string GenerarTokenQr(Usuario usuario);
    ClaimsPrincipal? ValidarToken(string token);
    ClaimsPrincipal? ValidarTokenQr(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerarToken(Usuario usuario)
    {
        var claims = CrearClaimsBase(usuario).Append(new Claim("tipo", "sesion"));
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
}
