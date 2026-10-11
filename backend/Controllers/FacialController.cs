using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Services;

namespace LenguajesFormalesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FacialController : ControllerBase
{
    private readonly IFacialService _facial;
    private readonly ILogger<FacialController> _logger;

    public FacialController(IFacialService facial, ILogger<FacialController> logger)
    {
        _facial = facial;
        _logger = logger;
    }

    private int? UsuarioId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : null;
    }

    /// <summary>Enrola o actualiza el rostro del usuario autenticado</summary>
    [HttpPost("enrolar")]
    public async Task<IActionResult> Enrolar([FromBody] EnrolarFacialDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { mensaje = "No se recibió un descriptor facial válido." });

        var id = UsuarioId();
        if (id == null) return Unauthorized();

        try
        {
            var ok = await _facial.EnrolarAsync(id.Value, dto.Descriptor);
            return ok
                ? Ok(new { mensaje = "Rostro enrolado correctamente." })
                : BadRequest(new { mensaje = "No se pudo enrolar el rostro. Intenta de nuevo con mejor iluminación." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enrolar rostro");
            return StatusCode(500, new { mensaje = "Error interno al enrolar el rostro." });
        }
    }

    /// <summary>Indica si el usuario autenticado ya tiene un rostro enrolado</summary>
    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        var id = UsuarioId();
        if (id == null) return Unauthorized();

        var tieneRostro = await _facial.TieneRostroAsync(id.Value);
        return Ok(new { tieneRostro });
    }
}
