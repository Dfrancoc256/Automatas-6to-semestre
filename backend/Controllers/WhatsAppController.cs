using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;

namespace LenguajesFormalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsApp;

        public WhatsAppController(IWhatsAppService whatsApp)
        {
            _whatsApp = whatsApp;
        }

        /// <summary>Envía un WhatsApp de prueba. Solo ADMIN, para no exponer un relé abierto.</summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendWhatsAppDto request)
        {
            var ok = await _whatsApp.SendTextAsync(request.To, request.Message);

            return ok
                ? Ok(new { success = true, message = "Mensaje de WhatsApp enviado." })
                : StatusCode(502, new
                {
                    success = false,
                    message = "No se pudo enviar. Revisa la configuración de WhatsApp, que la instancia esté conectada y los logs del backend."
                });
        }
    }
}
