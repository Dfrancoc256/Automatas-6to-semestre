using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;

namespace LenguajesFormalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> SendEmail(
            [FromBody] SendEmailDto request)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    request.To,
                    request.Subject,
                    request.Body
                );

                return Ok(new
                {
                    success = true,
                    message = "Correo enviado correctamente."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "No fue posible enviar el correo. Revisa los registros del servidor."
                });
            }
        }
    }
}
