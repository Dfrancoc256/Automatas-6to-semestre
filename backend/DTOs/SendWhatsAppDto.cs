using System.ComponentModel.DataAnnotations;

namespace LenguajesFormalesAPI.DTOs
{
    public class SendWhatsAppDto
    {
        /// <summary>Teléfono con o sin código de país (8 dígitos = Guatemala).</summary>
        [Required]
        [MaxLength(25)]
        public string To { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
