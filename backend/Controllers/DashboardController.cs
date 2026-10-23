using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Data;

namespace LenguajesFormalesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
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

    /// <summary>Estadísticas detalladas para gráficos (tendencias, distribución y rankings) — solo Admin/Supervisor</summary>
    [HttpGet("estadisticas/detalle")]
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    public async Task<IActionResult> EstadisticasDetalle([FromQuery] int dias = 30)
    {
        dias = Math.Clamp(dias, 7, 365);

        // Guatemala no usa horario de verano: UTC-6 fijo
        var offset = TimeSpan.FromHours(-6);
        DateTime Local(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc) + offset;

        var hoyLocal = Local(DateTime.UtcNow).Date;
        var inicioLocal = hoyLocal.AddDays(-(dias - 1));
        var desdeUtc = DateTime.SpecifyKind(inicioLocal - offset, DateTimeKind.Utc);

        // ── Usuarios ──
        var usuarios = await _db.Usuarios
            .Select(u => new
            {
                u.Rol,
                u.Activo,
                u.MetodoNotificacion,
                u.FechaRegistro,
                ConRostro = u.EncodingFacial != null && u.EncodingFacial != "",
                ConGoogle = u.GoogleSubject != null
            })
            .ToListAsync();

        // ── Análisis del periodo ──
        var analisis = await _db.ResultadosAnalisis
            .Where(r => r.FechaAnalisis >= desdeUtc)
            .Select(r => new
            {
                r.UsuarioId,
                Usuario = r.Usuario != null ? r.Usuario.Nickname : "—",
                r.Idioma,
                r.TotalPalabras,
                r.TotalTokens,
                r.FechaAnalisis
            })
            .ToListAsync();

        // ── Accesos del periodo ──
        var logins = await _db.BitacoraLogins
            .Where(b => b.FechaHora >= desdeUtc)
            .Select(b => new { b.Resultado, b.Metodo, b.FechaHora, b.IpOrigen })
            .ToListAsync();

        var fechas = Enumerable.Range(0, dias).Select(i => inicioLocal.AddDays(i)).ToList();

        var analisisPorDia = fechas.Select(f => new
        {
            fecha = f.ToString("yyyy-MM-dd"),
            cantidad = analisis.Count(a => Local(a.FechaAnalisis).Date == f),
            palabras = analisis.Where(a => Local(a.FechaAnalisis).Date == f).Sum(a => a.TotalPalabras)
        }).ToList();

        var loginsPorDia = fechas.Select(f => new
        {
            fecha = f.ToString("yyyy-MM-dd"),
            exitosos = logins.Count(l => Local(l.FechaHora).Date == f && l.Resultado == "exitoso"),
            fallidos = logins.Count(l => Local(l.FechaHora).Date == f && l.Resultado != "exitoso")
        }).ToList();

        var registrosPorDia = fechas.Select(f => new
        {
            fecha = f.ToString("yyyy-MM-dd"),
            cantidad = usuarios.Count(u => Local(u.FechaRegistro).Date == f)
        }).ToList();

        var totalExitosos = logins.Count(l => l.Resultado == "exitoso");
        var totalPalabrasPeriodo = analisis.Sum(a => a.TotalPalabras);

        return Ok(new
        {
            dias,
            desde = inicioLocal.ToString("yyyy-MM-dd"),
            hasta = hoyLocal.ToString("yyyy-MM-dd"),
            kpis = new
            {
                usuariosActivos = usuarios.Count(u => u.Activo),
                usuariosInactivos = usuarios.Count(u => !u.Activo),
                usuariosConRostro = usuarios.Count(u => u.Activo && u.ConRostro),
                usuariosConGoogle = usuarios.Count(u => u.Activo && u.ConGoogle),
                nuevosUsuarios = usuarios.Count(u => u.FechaRegistro >= desdeUtc),
                analisis = analisis.Count,
                palabras = totalPalabrasPeriodo,
                tokens = analisis.Sum(a => a.TotalTokens),
                promedioPalabras = analisis.Count == 0 ? 0 : Math.Round((double)totalPalabrasPeriodo / analisis.Count, 1),
                analistasActivos = analisis.Select(a => a.UsuarioId).Distinct().Count(),
                logins = logins.Count,
                loginsExitosos = totalExitosos,
                loginsFallidos = logins.Count - totalExitosos,
                tasaExito = logins.Count == 0 ? 0 : Math.Round(100.0 * totalExitosos / logins.Count, 1)
            },
            analisisPorDia,
            loginsPorDia,
            registrosPorDia,
            analisisPorIdioma = analisis
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Idioma) ? "sin idioma" : a.Idioma.ToLowerInvariant())
                .Select(g => new { idioma = g.Key, cantidad = g.Count(), palabras = g.Sum(a => a.TotalPalabras) })
                .OrderByDescending(x => x.cantidad)
                .ToList(),
            loginsPorMetodo = logins
                .GroupBy(l => l.Metodo)
                .Select(g => new
                {
                    metodo = g.Key,
                    exitosos = g.Count(l => l.Resultado == "exitoso"),
                    fallidos = g.Count(l => l.Resultado != "exitoso")
                })
                .OrderByDescending(x => x.exitosos + x.fallidos)
                .ToList(),
            loginsPorHora = Enumerable.Range(0, 24)
                .Select(h => new { hora = h, cantidad = logins.Count(l => Local(l.FechaHora).Hour == h) })
                .ToList(),
            usuariosPorRol = usuarios.Where(u => u.Activo)
                .GroupBy(u => u.Rol)
                .Select(g => new { rol = g.Key, cantidad = g.Count() })
                .OrderByDescending(x => x.cantidad)
                .ToList(),
            usuariosPorNotificacion = usuarios.Where(u => u.Activo)
                .GroupBy(u => u.MetodoNotificacion)
                .Select(g => new { metodo = g.Key, cantidad = g.Count() })
                .OrderByDescending(x => x.cantidad)
                .ToList(),
            topAnalistas = analisis
                .GroupBy(a => a.Usuario)
                .Select(g => new { usuario = g.Key, analisis = g.Count(), palabras = g.Sum(a => a.TotalPalabras) })
                .OrderByDescending(x => x.analisis)
                .Take(5)
                .ToList(),
            ipsConFallos = logins
                .Where(l => l.Resultado != "exitoso" && !string.IsNullOrEmpty(l.IpOrigen))
                .GroupBy(l => l.IpOrigen!)
                .Select(g => new { ip = g.Key, fallidos = g.Count() })
                .OrderByDescending(x => x.fallidos)
                .Take(5)
                .ToList()
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
                b.IpOrigen,
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
                u.Correo,
                u.Telefono,
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
}

public record CambiarRolDTO(string? Rol);
