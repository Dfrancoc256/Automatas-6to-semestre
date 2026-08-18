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
