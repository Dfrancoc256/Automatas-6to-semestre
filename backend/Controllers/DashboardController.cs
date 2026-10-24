using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.Interfaces;
using LenguajesFormalesAPI.Models;
using LenguajesFormalesAPI.Services;

namespace LenguajesFormalesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IWhatsAppService _whatsApp;
    private readonly ICredentialService _credential;
    private readonly IJwtService _jwt;

    public DashboardController(
        AppDbContext db,
        IEmailService email,
        IWhatsAppService whatsApp,
        ICredentialService credential,
        IJwtService jwt)
    {
        _db = db;
        _email = email;
        _whatsApp = whatsApp;
        _credential = credential;
        _jwt = jwt;
    }

    /// <summary>Estadísticas generales — solo Admin/Supervisor</summary>
    [HttpGet("estadisticas")]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public async Task<IActionResult> Estadisticas()
    {
        var totalUsuarios   = await _db.Usuarios.CountAsync(u => u.Activo);
        var totalAnalisis   = await _db.ResultadosAnalisis.CountAsync();
        var analisisHoy     = await _db.ResultadosAnalisis
            .CountAsync(r => r.FechaAnalisis.Date == DateTime.UtcNow.Date);
        var loginsHoy       = await _db.BitacoraLogins
            .CountAsync(b => b.FechaHora.Date == DateTime.UtcNow.Date && b.Resultado == "exitoso");
        var loginsFallidosHoy = await _db.BitacoraLogins
            .CountAsync(b => b.FechaHora.Date == DateTime.UtcNow.Date && b.Resultado == "fallido");

        var porIdioma = await _db.ResultadosAnalisis
            .GroupBy(r => r.Idioma)
            .Select(g => new { Idioma = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var recientes = await _db.ResultadosAnalisis
            .Include(r => r.Usuario)
            .OrderByDescending(r => r.FechaAnalisis)
            .Take(10)
            .Select(r => new
            {
                r.Id,
                r.NombreArchivo,
                r.Idioma,
                r.TotalPalabras,
                r.FechaAnalisis,
                Usuario = r.Usuario != null ? r.Usuario.Nickname : "—"
            })
            .ToListAsync();

        return Ok(new
        {
            totalUsuarios,
            totalAnalisis,
            analisisHoy,
            loginsHoy,
            loginsFallidosHoy,
            porIdioma,
            recientes
        });
    }

    /// <summary>
    /// Indicadores agregados para el panel ejecutivo. Las agrupaciones se ejecutan
    /// en PostgreSQL para no cargar historiales completos en memoria del servidor.
    /// </summary>
    [HttpGet("estadisticas/detalle")]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public async Task<IActionResult> EstadisticasDetalle([FromQuery] int dias = 30)
    {
        dias = dias switch { 7 or 30 or 90 => dias, _ => 30 };

        var hoy = DateTime.UtcNow.Date;
        var desde = hoy.AddDays(-(dias - 1));
        var hasta = hoy.AddDays(1);
        var analisisPeriodo = _db.ResultadosAnalisis.AsNoTracking()
            .Where(resultado => resultado.FechaAnalisis >= desde && resultado.FechaAnalisis < hasta);
        var accesosPeriodo = _db.BitacoraLogins.AsNoTracking()
            .Where(acceso => acceso.FechaHora >= desde && acceso.FechaHora < hasta);
        var usuariosActivos = _db.Usuarios.AsNoTracking().Where(usuario => usuario.Activo);

        var totalUsuarios = await usuariosActivos.CountAsync();
        var totalAnalisis = await analisisPeriodo.CountAsync();
        var totalPalabras = await analisisPeriodo.SumAsync(resultado => (long?)resultado.TotalPalabras) ?? 0;
        var totalAccesos = await accesosPeriodo.CountAsync();
        var accesosExitosos = await accesosPeriodo.CountAsync(acceso => acceso.Resultado == "exitoso");

        var analisisPorDia = await analisisPeriodo
            .GroupBy(resultado => resultado.FechaAnalisis.Date)
            .Select(grupo => new
            {
                fecha = grupo.Key,
                cantidad = grupo.Count(),
                palabras = grupo.Sum(resultado => resultado.TotalPalabras)
            })
            .OrderBy(item => item.fecha)
            .ToListAsync();

        var accesosPorDia = await accesosPeriodo
            .GroupBy(acceso => acceso.FechaHora.Date)
            .Select(grupo => new
            {
                fecha = grupo.Key,
                exitosos = grupo.Count(acceso => acceso.Resultado == "exitoso"),
                fallidos = grupo.Count(acceso => acceso.Resultado != "exitoso")
            })
            .OrderBy(item => item.fecha)
            .ToListAsync();

        var analisisPorIdioma = await analisisPeriodo
            .GroupBy(resultado => resultado.Idioma)
            .Select(grupo => new { idioma = grupo.Key, cantidad = grupo.Count(), palabras = grupo.Sum(resultado => resultado.TotalPalabras) })
            .OrderByDescending(item => item.cantidad)
            .ToListAsync();

        var accesosPorMetodo = await accesosPeriodo
            .GroupBy(acceso => acceso.Metodo)
            .Select(grupo => new
            {
                metodo = grupo.Key,
                exitosos = grupo.Count(acceso => acceso.Resultado == "exitoso"),
                fallidos = grupo.Count(acceso => acceso.Resultado != "exitoso")
            })
            .OrderByDescending(item => item.exitosos + item.fallidos)
            .ToListAsync();

        var usuariosPorRol = await usuariosActivos
            .GroupBy(usuario => usuario.Rol)
            .Select(grupo => new { rol = grupo.Key, cantidad = grupo.Count() })
            .OrderByDescending(item => item.cantidad)
            .ToListAsync();

        var usuariosConRostro = await usuariosActivos.CountAsync(usuario =>
            usuario.EncodingFacial != null && usuario.EncodingFacial != "");

        var nuevosUsuarios = await usuariosActivos.CountAsync(usuario =>
            usuario.FechaRegistro >= desde && usuario.FechaRegistro < hasta);

        var topAnalistas = await analisisPeriodo
            .GroupBy(resultado => new { resultado.UsuarioId, Usuario = resultado.Usuario != null ? resultado.Usuario.Nickname : "—" })
            .Select(grupo => new { usuario = grupo.Key.Usuario, analisis = grupo.Count(), palabras = grupo.Sum(resultado => resultado.TotalPalabras) })
            .OrderByDescending(item => item.analisis)
            .ThenByDescending(item => item.palabras)
            .Take(5)
            .ToListAsync();

        var alertasIp = await accesosPeriodo
            .Where(acceso => acceso.Resultado != "exitoso" && acceso.IpOrigen != null && acceso.IpOrigen != "")
            .GroupBy(acceso => acceso.IpOrigen)
            .Select(grupo => new { ip = grupo.Key, fallidos = grupo.Count() })
            .OrderByDescending(item => item.fallidos)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            dias,
            desde,
            hasta = hoy,
            indicadores = new
            {
                usuarios = totalUsuarios,
                analisis = totalAnalisis,
                palabras = totalPalabras,
                nuevosUsuarios,
                accesos = totalAccesos,
                accesosExitosos,
                accesosFallidos = totalAccesos - accesosExitosos,
                tasaExito = totalAccesos == 0 ? 0 : Math.Round(accesosExitosos * 100d / totalAccesos, 1),
                usuariosConRostro
            },
            analisisPorDia,
            accesosPorDia,
            analisisPorIdioma,
            accesosPorMetodo,
            usuariosPorRol,
            topAnalistas,
            alertasIp
        });
    }

    /// <summary>Bitácora de accesos — solo Admin</summary>
    [HttpGet("bitacora")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Bitacora([FromQuery] int pagina = 1, [FromQuery] int por = 20)
    {
        pagina = Math.Max(1, pagina);
        por = Math.Clamp(por, 1, 100);

        var total = await _db.BitacoraLogins.CountAsync();
        var registros = await _db.BitacoraLogins
            .Include(b => b.Usuario)
            .OrderByDescending(b => b.FechaHora)
            .Skip((pagina - 1) * por)
            .Take(por)
            .Select(b => new
            {
                b.Id,
                b.FechaHora,
                b.Resultado,
                b.Metodo,
                Usuario = b.Usuario != null ? b.Usuario.Nickname : "—"
            })
            .ToListAsync();

        return Ok(new { total, pagina, por, registros });
    }

    /// <summary>Gestión de usuarios — solo Admin</summary>
    [HttpGet("usuarios")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await _db.Usuarios
            .OrderByDescending(u => u.FechaRegistro)
            .Select(u => new
            {
                u.Id,
                u.Nickname,
                u.Nombre,
                u.Apellido,
                u.Correo,
                u.Telefono,
                // Algunas cuentas antiguas solo tienen foto original.
                FotoModificada = u.FotoModificada ?? u.FotoOriginal,
                u.MetodoNotificacion,
                u.Rol,
                u.Activo,
                u.FechaRegistro
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    /// <summary>Activar/desactivar usuario — solo Admin</summary>
    [HttpPatch("usuarios/{id:int}/toggle")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ToggleUsuario(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = !usuario.Activo;
        await _db.SaveChangesAsync();

        return Ok(new { usuario.Id, usuario.Activo });
    }

    /// <summary>Cambiar rol de usuario — solo Admin</summary>
    [HttpPatch("usuarios/{id:int}/rol")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolDTO dto)
    {
        var rolesValidos = new HashSet<string> { "ADMIN", "SUPERVISOR", "ANALISTA" };
        var rol = dto.Rol?.Trim().ToUpperInvariant();
        if (rol is null || !rolesValidos.Contains(rol))
            return BadRequest(new { mensaje = "Rol inválido." });

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Rol = rol;
        await _db.SaveChangesAsync();

        return Ok(new { usuario.Id, usuario.Rol });
    }

    [HttpPatch("usuarios/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioAdminDTO dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(u => u.Id != id && u.Correo == correo))
            return Conflict(new { mensaje = "El correo ya está registrado." });

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Apellido = dto.Apellido.Trim();
        usuario.Correo = correo;
        usuario.Telefono = dto.Telefono.Trim();
        usuario.MetodoNotificacion = dto.MetodoNotificacion;
        usuario.Rol = dto.Rol;
        await _db.SaveChangesAsync();

        return Ok(new { usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Correo, usuario.Telefono, usuario.MetodoNotificacion, usuario.Rol });
    }

    [HttpDelete("usuarios/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DesactivarUsuario(int id)
    {
        var idActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idActual == id.ToString())
            return BadRequest(new { mensaje = "No puedes desactivar tu propia cuenta." });

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = false;
        await _db.SaveChangesAsync();
        return Ok(new { usuario.Id, usuario.Activo });
    }

    /// <summary>Envía el carnet PDF de un usuario por correo o WhatsApp — solo Admin</summary>
    [HttpPost("usuarios/{id:int}/credencial/enviar")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> EnviarCredencialUsuario(int id, [FromBody] EnviarCredencialUsuarioDTO dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        if (!ModelState.IsValid)
            return BadRequest(new { mensaje = "Canal inválido. Usa correo o WhatsApp." });

        var nombre = WebUtility.HtmlEncode(usuario.NombreCompleto);
        var codigoQr = _jwt.GenerarCodigoQrCredencial(usuario);
        var pdf = _credential.GenerarPdf(usuario, codigoQr);
        var nombreArchivo = $"credencial-{usuario.Nickname}.pdf";

        try
        {
            if (dto.Canal == "email")
            {
                await _email.SendEmailAsync(
                    usuario.Correo,
                    "Tu carnet - Lenguajes Formales",
                    $"<p>Hola {nombre},</p><p>Adjuntamos tu carnet de la plataforma de Lenguajes Formales y Autómatas.</p><p>El código QR permite verificar su autenticidad.</p>",
                    pdf,
                    nombreArchivo);

                return Ok(new { mensaje = "Carnet PDF enviado por correo." });
            }

            var enviado = await _whatsApp.SendPdfAsync(
                usuario.Telefono,
                pdf,
                nombreArchivo,
                $"Hola {usuario.NombreCompleto}, adjuntamos tu carnet. El código QR permite verificar su autenticidad.");

            return enviado
                ? Ok(new { mensaje = "Carnet PDF enviado por WhatsApp." })
                : StatusCode(502, new { mensaje = "No se pudo enviar el carnet por WhatsApp. Inténtalo de nuevo o usa el correo." });
        }
        catch (Exception)
        {
            return StatusCode(502, new { mensaje = "No se pudo enviar el carnet PDF en este momento." });
        }
    }
}

public record CambiarRolDTO(string? Rol);

public class ActualizarUsuarioAdminDTO
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(80, MinimumLength = 2)]
    public string Apellido { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
    public string Correo { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression(@"^\+?[0-9\s-]{8,20}$")]
    public string Telefono { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression("^(email|whatsapp|ambos)$")]
    public string MetodoNotificacion { get; set; } = "email";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression("^(ADMIN|SUPERVISOR|ANALISTA)$")]
    public string Rol { get; set; } = "ANALISTA";
}

public class EnviarCredencialUsuarioDTO
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression("^(email|whatsapp)$")]
    public string Canal { get; set; } = string.Empty;
}
