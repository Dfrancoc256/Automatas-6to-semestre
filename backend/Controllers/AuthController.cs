using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Services;
using System.Security.Claims;

namespace LenguajesFormalesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService      _auth;
    private readonly IRecaptchaService _recaptcha;
    private readonly ICredentialService _credential;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, IRecaptchaService recaptcha,
        ICredentialService credential, IJwtService jwt,
        ILogger<AuthController> logger)
    {
        _auth      = auth;
        _recaptcha = recaptcha;
        _credential = credential;
        _jwt = jwt;
        _logger    = logger;
    }

    /// <summary>Registro de nuevo usuario</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Registrar([FromBody] RegisterDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage) });

        if (!await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Verificación reCAPTCHA inválida." });

        try
        {
            var result = await _auth.RegistrarAsync(dto);
            EstablecerCookieSesion(result.Token, result.Expiracion);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro");
            return StatusCode(500, new { mensaje = "Error interno del servidor." });
        }
    }

    /// <summary>Login con usuario/contraseña</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage) });

        if (!await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Verificación reCAPTCHA inválida." });

        var ip        = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var userAgent = Request.Headers["User-Agent"].ToString();

        try
        {
            var result = await _auth.LoginAsync(dto, ip, userAgent);
            if (result == null)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            EstablecerCookieSesion(result.Token, result.Expiracion);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión");
            return StatusCode(500, new { mensaje = "No fue posible iniciar sesión." });
        }
    }

    /// <summary>Login mediante el código QR firmado de la credencial</summary>
    [HttpPost("login-qr")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginQr([FromBody] LoginQrDTO dto)
    {
        if (!await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Verificación reCAPTCHA inválida." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var userAgent = Request.Headers.UserAgent.ToString();

        try
        {
            var result = await _auth.LoginQrAsync(dto, ip, userAgent);
            if (result == null)
                return Unauthorized(new { mensaje = "Código QR inválido o vencido." });

            EstablecerCookieSesion(result.Token, result.Expiracion);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión QR");
            return StatusCode(500, new { mensaje = "No fue posible iniciar sesión." });
        }
    }

    /// <summary>Obtener perfil del usuario autenticado</summary>
    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id))
            return Unauthorized();

        var perfil = await _auth.ObtenerPerfilAsync(id);
        return perfil == null ? NotFound() : Ok(perfil);
    }

    /// <summary>Descargar la credencial PDF con código QR del usuario autenticado</summary>
    [HttpGet("credencial")]
    [Authorize]
    public async Task<IActionResult> DescargarCredencial()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id))
            return Unauthorized();

        var perfil = await _auth.ObtenerPerfilAsync(id);
        if (perfil == null)
            return NotFound();

        var usuario = new LenguajesFormalesAPI.Models.Usuario
        {
            Id = perfil.Id,
            Correo = perfil.Correo,
            Telefono = perfil.Telefono,
            Nickname = perfil.Nickname,
            Rol = perfil.Rol,
            FotoModificada = perfil.FotoModificada,
            FechaRegistro = perfil.FechaRegistro
        };
        var codigoQr = _jwt.GenerarTokenQr(usuario);
        var pdf = _credential.GenerarPdf(usuario, codigoQr);
        return File(pdf, "application/pdf", $"credencial-{usuario.Nickname}.pdf");
    }

    /// <summary>Actualizar perfil</summary>
    [HttpPut("perfil")]
    [Authorize]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDTO dto)
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id))
            return Unauthorized();

        try
        {
            var ok = await _auth.ActualizarPerfilAsync(id, dto);
            return ok ? Ok(new { mensaje = "Perfil actualizado." }) : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Elimina la cookie de sesión del navegador actual.</summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("umg_session", new CookieOptions
        {
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        });
        return NoContent();
    }

    private void EstablecerCookieSesion(string token, DateTime expiracion)
    {
        Response.Cookies.Append("umg_session", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiracion),
            Path = "/"
        });
    }
}
