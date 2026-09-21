using Microsoft.AspNetCore.Mvc;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;

namespace LenguajesFormalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "No fue posible enviar el correo.",
                    error = ex.Message
                });
            }
        }
    }
}