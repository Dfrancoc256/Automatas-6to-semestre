using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Models;
using LenguajesFormalesAPI.Services;

namespace LenguajesFormalesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnalisisController : ControllerBase
{
    private readonly IAnalisisLexicoService _analizador;
    private readonly AppDbContext           _db;
    private readonly ILogger<AnalisisController> _logger;

    private static readonly HashSet<string> IdiomasPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
        { "español", "inglés", "ruso", "chino", "árabe" };

    public AnalisisController(IAnalisisLexicoService analizador, AppDbContext db,
        ILogger<AnalisisController> logger)
    {
        _analizador = analizador;
        _db         = db;
        _logger     = logger;
    }

    /// <summary>Procesar análisis léxico de un archivo de texto</summary>
    [HttpPost("procesar")]
    public async Task<IActionResult> Procesar([FromBody] AnalisisRequestDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage) });

        if (!IdiomasPermitidos.Contains(dto.Idioma))
            return BadRequest(new { mensaje = "Idioma no soportado. Use: español, inglés, ruso, chino o árabe." });

        if (string.IsNullOrWhiteSpace(dto.Contenido))
            return BadRequest(new { mensaje = "El contenido no puede estar vacío." });

        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var usuarioId))
            return Unauthorized();

        try
        {
            var resultado = _analizador.Analizar(dto.Contenido, dto.Idioma, dto.NombreArchivo);

            // Persistir resultado
            var registro = new ResultadoAnalisis
            {
                UsuarioId    = usuarioId,
                Idioma       = dto.Idioma,
                NombreArchivo = dto.NombreArchivo,
                TotalPalabras = resultado.TotalPalabras,
                TotalTokens  = resultado.TotalTokens,
                DetalleJson  = JsonSerializer.Serialize(resultado),
                FechaAnalisis = DateTime.UtcNow
            };

            _db.ResultadosAnalisis.Add(registro);
            await _db.SaveChangesAsync();

            resultado.AnalisisId = registro.Id;
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando análisis léxico");
            return StatusCode(500, new { mensaje = "Error al procesar el archivo." });
        }
    }

    /// <summary>Historial de análisis del usuario</summary>
    [HttpGet("historial")]
    public async Task<IActionResult> Historial()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var usuarioId))
            return Unauthorized();

        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        IQueryable<ResultadoAnalisis> query = _db.ResultadosAnalisis;

        // Admin/Supervisor ven todo; Analista ve solo los suyos
        if (rol != "ADMIN" && rol != "SUPERVISOR")
            query = query.Where(r => r.UsuarioId == usuarioId);

        var lista = await query
            .OrderByDescending(r => r.FechaAnalisis)
            .Take(50)
            .Select(r => new HistorialAnalisisDTO
            {
                Id           = r.Id,
                NombreArchivo = r.NombreArchivo,
                Idioma       = r.Idioma,
                TotalPalabras = r.TotalPalabras,
                FechaAnalisis = r.FechaAnalisis
            })
            .ToListAsync();

        return Ok(lista);
    }

    /// <summary>Detalle de un análisis específico</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var usuarioId))
            return Unauthorized();

        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var registro = await _db.ResultadosAnalisis.FindAsync(id);

        if (registro == null) return NotFound();

        if (rol != "ADMIN" && rol != "SUPERVISOR" && registro.UsuarioId != usuarioId)
            return Forbid();

        var detalle = JsonSerializer.Deserialize<AnalisisResultadoDTO>(registro.DetalleJson);
        return Ok(detalle);
    }
}
