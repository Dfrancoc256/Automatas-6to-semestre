using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;
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
    private readonly IEmailService _email;
    private readonly IWhatsAppService _whatsApp;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, IRecaptchaService recaptcha,
        ICredentialService credential, IJwtService jwt,
        IEmailService email, IWhatsAppService whatsApp,
        ILogger<AuthController> logger)
    {
        _email     = email;
        _whatsApp  = whatsApp;
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

    /// <summary>Login mediante reconocimiento facial (descriptor calculado en el navegador)</summary>
    [HttpPost("login-facial")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LoginFacial([FromBody] LoginFacialDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage) });

        if (!await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Verificación reCAPTCHA inválida." });

        var ip        = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var userAgent = Request.Headers.UserAgent.ToString();

        try
        {
            var result = await _auth.LoginFacialAsync(dto, ip, userAgent);
            if (result == null)
                return Unauthorized(new { mensaje = "Rostro no reconocido." });

            EstablecerCookieSesion(result.Token, result.Expiracion);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión facial");
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
        var credencial = await GenerarCredencialAsync();
        if (credencial == null)
            return Unauthorized();

        return File(credencial.Value.Pdf, "application/pdf", credencial.Value.NombreArchivo);
    }

    /// <summary>Enviar la credencial PDF al correo o al WhatsApp registrados del usuario autenticado</summary>
    [HttpPost("credencial/enviar")]
    [Authorize]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> EnviarCredencial([FromBody] EnviarCredencialDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { mensaje = "Canal inválido. Usa \"email\" o \"whatsapp\"." });

        var credencial = await GenerarCredencialAsync();
        if (credencial == null)
            return Unauthorized();

        var (pdf, nombreArchivo, perfil) = credencial.Value;

        // Siempre al correo/teléfono registrados: nunca a un destino indicado por el cliente.
        try
        {
            if (dto.Canal == "email")
            {
                await _email.SendEmailAsync(
                    perfil.Correo,
                    "Tu credencial - Lenguajes Formales",
                    $"""
                    <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto;">
                        <h2 style="color: #051B2E;">Hola, {System.Net.WebUtility.HtmlEncode(perfil.Nickname)}</h2>
                        <p>Adjuntamos tu credencial de la plataforma de Lenguajes Formales y Autómatas.</p>
                        <p style="font-size: 12px; color: #666;">
                            El código QR permite iniciar sesión: no compartas este archivo con otras personas.
                        </p>
                    </div>
                    """,
                    pdf,
                    nombreArchivo);

                return Ok(new { mensaje = $"Credencial enviada a {OcultarCorreo(perfil.Correo)}." });
            }

            var enviado = await _whatsApp.SendPdfAsync(
                perfil.Telefono,
                pdf,
                nombreArchivo,
                $"Hola {perfil.Nickname}, esta es tu credencial. No la compartas con otras personas.");

            return enviado
                ? Ok(new { mensaje = "Credencial enviada por WhatsApp." })
                : StatusCode(502, new { mensaje = "No se pudo enviar por WhatsApp en este momento. Inténtalo de nuevo o usa el correo." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar la credencial por {Canal}", dto.Canal);
            return StatusCode(502, new { mensaje = "No se pudo enviar la credencial en este momento." });
        }
    }

    private async Task<(byte[] Pdf, string NombreArchivo, UsuarioPerfilDTO Perfil)?> GenerarCredencialAsync()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id))
            return null;

        var perfil = await _auth.ObtenerPerfilAsync(id);
        if (perfil == null)
            return null;

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
        return (pdf, $"credencial-{usuario.Nickname}.pdf", perfil);
    }

    private static string OcultarCorreo(string correo)
    {
        var arroba = correo.IndexOf('@');
        return arroba <= 1 ? correo : $"{correo[0]}***{correo[arroba..]}";
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
