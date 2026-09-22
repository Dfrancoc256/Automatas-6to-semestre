using System.ComponentModel.DataAnnotations;

namespace LenguajesFormalesAPI.DTOs
{
    public class SendEmailDto
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;
    }
}