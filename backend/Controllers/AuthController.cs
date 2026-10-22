using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;
using LenguajesFormalesAPI.Services;
using LenguajesFormalesAPI.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

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
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public AuthController(IAuthService auth, IRecaptchaService recaptcha,
        ICredentialService credential, IJwtService jwt,
        IEmailService email, IWhatsAppService whatsApp,
        ILogger<AuthController> logger, IWebHostEnvironment environment, AppDbContext db, IConfiguration configuration,
        IMemoryCache cache)
    {
        _email     = email;
        _whatsApp  = whatsApp;
        _auth      = auth;
        _recaptcha = recaptcha;
        _credential = credential;
        _jwt = jwt;
        _logger    = logger;
        _environment = environment;
        _configuration = configuration;
        _db = db;
        _cache = cache;
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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgres)
        {
            // El detalle queda en el log.
            _logger.LogError(ex, "No se pudo guardar el registro. PostgreSQL: {SqlState}", postgres.SqlState);

            return postgres.SqlState switch
            {
                "23505" => Conflict(new { mensaje = "Ya existe una cuenta con esos datos." }),
                "42703" or "23502" => StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { mensaje = "La base de datos requiere una actualización. Ejecuta la migración pendiente y vuelve a intentarlo." }),
                _ => StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { mensaje = "No fue posible guardar el registro en este momento. Intenta nuevamente en unos minutos." })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro");
            return StatusCode(500, new { mensaje = "No fue posible completar el registro. Si el problema continúa, contacta al administrador." });
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

    /// <summary>Valida el CAPTCHA y entrega un permiso de un solo uso para iniciar OAuth con Google.</summary>
    [HttpPost("google/verificar-captcha")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerificarCaptchaGoogle([FromBody] RecaptchaTokenDTO dto)
    {
        if (!ModelState.IsValid || !await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Completa la verificación reCAPTCHA antes de continuar con Google." });

        var permiso = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _cache.Set($"google-captcha:{permiso}", true, TimeSpan.FromMinutes(5));
        Response.Cookies.Append("umg_google_captcha", permiso, new CookieOptions
        {
            HttpOnly = true,
            Secure = UsarCookieSegura(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth/google",
            MaxAge = TimeSpan.FromMinutes(5),
            IsEssential = true
        });
        return NoContent();
    }

    [HttpGet("google/iniciar")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public IActionResult IniciarGoogle()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Google:ClientId"]))
            return BadRequest("El acceso con Google no está configurado.");

        if (!Request.Cookies.TryGetValue("umg_google_captcha", out var permiso)
            || string.IsNullOrWhiteSpace(permiso)
            || !_cache.TryGetValue<bool>($"google-captcha:{permiso}", out _))
        {
            return BadRequest(new { mensaje = "Completa la verificación reCAPTCHA antes de continuar con Google." });
        }

        _cache.Remove($"google-captcha:{permiso}");
        Response.Cookies.Delete("umg_google_captcha", new CookieOptions
        {
            Secure = UsarCookieSegura(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth/google",
            IsEssential = true
        });

        return DesafiarGoogle(false);
    }

    /// <summary>Inicia el alta de una cuenta nueva con Google. No inicia sesión de cuentas existentes.</summary>
    [HttpGet("google/registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public IActionResult RegistrarConGoogle()
    {
        if (string.IsNullOrWhiteSpace(_configuration["Google:ClientId"]))
            return BadRequest("El acceso con Google no está configurado.");

        return DesafiarGoogle(true);
    }

    [HttpGet("google/completar")]
    [AllowAnonymous]
    public async Task<IActionResult> CompletarGoogle()
    {
        var externo = await HttpContext.AuthenticateAsync("GoogleExternal");
        await HttpContext.SignOutAsync("GoogleExternal");
        var subject = externo.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var correo = externo.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var nombre = externo.Principal?.FindFirst(ClaimTypes.GivenName)?.Value
            ?? externo.Principal?.FindFirst(ClaimTypes.Name)?.Value;
        var esRegistro = externo.Properties?.Items.TryGetValue("modo_registro", out var modoRegistro) == true
            && modoRegistro == "google";
        if (!externo.Succeeded || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(correo)
            || string.IsNullOrWhiteSpace(nombre))
            return Redirect(RutaPublica("/login?error=google"));

        if (esRegistro)
        {
            var correoNormalizado = correo.Trim().ToLowerInvariant();
            var cuentaExiste = await _db.Usuarios.AsNoTracking().AnyAsync(u => u.Activo
                && (u.GoogleSubject == subject || u.Correo == correoNormalizado));
            if (cuentaExiste)
                return Redirect(RutaPublica("/login?error=google-existente"));

            var tokenRegistroNuevo = _jwt.GenerarTokenRegistroGoogle(subject, correoNormalizado, nombre);
            return Redirect(RutaPublica($"/login?modo=registro&googleRegistro={Uri.EscapeDataString(tokenRegistroNuevo)}"));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido";
        var resultado = await _auth.LoginGoogleAsync(subject, correo, ip, Request.Headers.UserAgent.ToString());
        if (resultado != null)
        {
            EstablecerCookieSesion(resultado.Token, resultado.Expiracion);
            return Redirect(RutaPublica("/dashboard"));
        }

        var tokenRegistro = _jwt.GenerarTokenRegistroGoogle(subject, correo, nombre);
        return Redirect(RutaPublica($"/login?modo=registro&googleRegistro={Uri.EscapeDataString(tokenRegistro)}"));
    }

    [HttpPost("google-registro")]
    [AllowAnonymous]
    public IActionResult ObtenerDatosRegistroGoogle([FromBody] GoogleRegistroTokenDTO dto)
    {
        var principal = _jwt.ValidarTokenRegistroGoogle(dto.Token);
        var correo = principal?.FindFirst(ClaimTypes.Email)?.Value;
        var nombre = principal?.FindFirst(ClaimTypes.GivenName)?.Value;
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nombre))
            return BadRequest(new { mensaje = "La verificación de Google venció. Selecciona tu cuenta nuevamente." });

        return Ok(new { correo, nombre });
    }

    [HttpPost("cambiar-password-temporal")]
    [Authorize]
    public async Task<IActionResult> CambiarPasswordTemporal([FromBody] CambiarPasswordTemporalDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage) });

        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var usuarioId))
            return Unauthorized();

        var result = await _auth.CambiarPasswordTemporalAsync(
            usuarioId, dto.PasswordTemporal, dto.NuevoPassword);
        if (result == null)
            return BadRequest(new { mensaje = "La contraseña temporal no es correcta o ya fue cambiada." });

        EstablecerCookieSesion(result.Token, result.Expiracion);
        return Ok(result);
    }

    [HttpGet("validar-enlace-inicial")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ValidarEnlaceInicial([FromQuery] string token)
    {
        var valido = await _auth.EnlaceCambioInicialValidoAsync(token);
        return valido
            ? Ok(new { valido = true })
            : BadRequest(new { mensaje = "El enlace expiró o ya fue utilizado." });
    }

    [HttpPost("cambiar-password-inicial")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CambiarPasswordInicial([FromBody] CambiarPasswordInicialDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var sesion = await _auth.CambiarPasswordInicialAsync(dto.Token, dto.NuevoPassword);
        if (sesion is null)
            return BadRequest(new { mensaje = "El enlace expiró o ya fue utilizado." });

        EstablecerCookieSesion(sesion.Token, sesion.Expiracion);
        return Ok(sesion);
    }

    /// <summary>Solicita un enlace de un solo uso para restablecer la contraseña.</summary>
    [HttpPost("solicitar-restablecimiento")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SolicitarRestablecimiento([FromBody] ResetPasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        if (!await _recaptcha.ValidarAsync(dto.RecaptchaToken))
            return BadRequest(new { mensaje = "Verificación reCAPTCHA inválida." });

        var enviado = await _auth.SolicitarRestablecimientoPasswordAsync(dto.Correo);
        if (!enviado)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensaje = "No fue posible enviar el correo en este momento. Intenta nuevamente más tarde." });

        // Respuesta neutral: evita confirmar si un correo tiene cuenta.
        return Ok(new { mensaje = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña." });
    }

    /// <summary>Aplica una nueva contraseña usando el token recibido por correo.</summary>
    [HttpPost("restablecer-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RestablecerPassword([FromBody] ConfirmarResetPasswordDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var actualizado = await _auth.RestablecerPasswordAsync(dto.Token, dto.NuevoPassword);
        if (!actualizado)
            return BadRequest(new { mensaje = "El enlace no es válido, ya fue utilizado o venció. Solicita uno nuevo." });

        return Ok(new { mensaje = "Tu contraseña fue actualizada. Ya puedes iniciar sesión." });
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

    [HttpGet("verificar-credencial")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerificarCredencial([FromQuery] string token)
    {
        var principal = _jwt.ValidarCodigoQrCredencial(token);
        var idTexto = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idTexto, out var usuarioId))
            return Unauthorized(new { valido = false, mensaje = "La credencial no es válida o venció." });

        var usuario = await _db.Usuarios
            .Where(u => u.Id == usuarioId && u.Activo)
            .Select(u => new { u.Nombre, u.Apellido, u.Nickname, u.Rol, u.FechaRegistro })
            .FirstOrDefaultAsync();
        if (usuario == null)
            return NotFound(new { valido = false, mensaje = "La credencial no se encuentra activa." });

        return Ok(new
        {
            valido = true,
            nombre = $"{usuario.Nombre} {usuario.Apellido}".Trim(),
            usuario.Nickname,
            usuario.Rol,
            usuario.FechaRegistro
        });
    }

    /// <summary>Login mediante reconocimiento facial (descriptor calculado en el navegador)</summary>
    [HttpPost("login-facial")]
    [AllowAnonymous]
    [EnableRateLimiting("facial-login")]
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

    /// <summary>Valida la cookie de sesión sin exponer el JWT al navegador.</summary>
    [HttpGet("sesion")]
    [Authorize]
    public async Task<IActionResult> SesionActual()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id))
            return Unauthorized();

        var perfil = await _auth.ObtenerPerfilAsync(id);
        if (perfil == null)
            return Unauthorized();

        var exp = User.FindFirst("exp")?.Value;
        var expiracion = long.TryParse(exp, out var segundos)
            ? DateTimeOffset.FromUnixTimeSeconds(segundos).UtcDateTime
            : DateTime.UtcNow;

        return Ok(new SesionActualDTO
        {
            Rol = perfil.Rol,
            Nickname = perfil.Nickname,
            FotoModificada = perfil.FotoModificada,
            Expiracion = expiracion
        });
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

        // Usa los datos de contacto registrados.
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
                            El código QR permite verificar la autenticidad de esta credencial.
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
                $"Hola {perfil.Nickname}, esta es tu credencial. El código QR permite verificar su autenticidad.");

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
            Nombre = perfil.Nombre,
            Apellido = perfil.Apellido,
            Telefono = perfil.Telefono,
            Nickname = perfil.Nickname,
            Rol = perfil.Rol,
            FotoModificada = perfil.FotoModificada,
            FechaRegistro = perfil.FechaRegistro
        };
        var codigoQr = _jwt.GenerarCodigoQrCredencial(usuario);
        var pdf = _credential.GenerarPdf(usuario, codigoQr);
        return (pdf, $"credencial-{usuario.Nickname}.pdf", perfil);
    }

    private static string OcultarCorreo(string correo)
    {
        var arroba = correo.IndexOf('@');
        return arroba <= 1 ? correo : $"{correo[0]}***{correo[arroba..]}";
    }

    private string RutaPublica(string ruta)
    {
        var baseUrl = _configuration["App:PublicUrl"];
        return $"{(string.IsNullOrWhiteSpace(baseUrl) ? "https://analizador-umg.duckdns.org" : baseUrl.TrimEnd('/'))}{ruta}";
    }

    private IActionResult DesafiarGoogle(bool esRegistro)
    {
        var retorno = Url.Action(nameof(CompletarGoogle), "Auth", null, Request.Scheme);
        var propiedades = new AuthenticationProperties { RedirectUri = retorno };
        if (esRegistro)
            propiedades.Items["modo_registro"] = "google";

        return Challenge(propiedades, GoogleDefaults.AuthenticationScheme);
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
            Secure = UsarCookieSegura(),
            HttpOnly = true,
            IsEssential = true
        });
        return NoContent();
    }

    private void EstablecerCookieSesion(string token, DateTime expiracion)
    {
        Response.Cookies.Append("umg_session", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = UsarCookieSegura(),
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(expiracion),
            Path = "/",
            IsEssential = true
        });
    }

    // HTTPS en producción.
    private bool UsarCookieSegura() => !_environment.IsDevelopment() || Request.IsHttps;
}
