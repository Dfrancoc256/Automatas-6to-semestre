using System.ComponentModel.DataAnnotations;

namespace LenguajesFormalesAPI.DTOs;

public class RegisterDTO
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\+?[0-9\s-]{8,20}$", ErrorMessage = "Teléfono inválido")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El nickname es obligatorio")]
    [MinLength(3), MaxLength(50)]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El método de notificación es obligatorio")]
    [RegularExpression("^(email|whatsapp|ambos)$", ErrorMessage = "Método de notificación inválido")]
    public string MetodoNotificacion { get; set; } = "email"; // "email", "whatsapp", "ambos"

    // Foto en base64 (tomada por webcam)
    [MaxLength(7_000_000, ErrorMessage = "La fotografía excede el tamaño permitido")]
    public string? FotoBase64 { get; set; }

    // Versión personalizada para el avatar y la credencial. La original se
    // conserva exclusivamente como referencia para la futura validación facial.
    [MaxLength(7_000_000, ErrorMessage = "La fotografía personalizada excede el tamaño permitido")]
    public string? FotoModificadaBase64 { get; set; }

    // Token de reCAPTCHA
    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class LoginDTO
{
    [Required(ErrorMessage = "El nickname o correo es obligatorio")]
    [MinLength(3, ErrorMessage = "El identificador debe tener al menos 3 caracteres")]
    [MaxLength(254)]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    // Token de reCAPTCHA
    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class LoginFacialDTO
{
    // Foto en base64 capturada en el login
    [Required]
    public string FotoBase64 { get; set; } = string.Empty;

    [Required]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class LoginQrDTO
{
    [Required]
    public string CodigoQr { get; set; } = string.Empty;

    [Required]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;
}

public class CambiarPasswordDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NuevoPassword { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string? FotoModificada { get; set; }
    public DateTime Expiracion { get; set; }
    public string? CodigoQr { get; set; }
}

public class UsuarioPerfilDTO
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string MetodoNotificacion { get; set; } = string.Empty;
    public string? FotoModificada { get; set; }
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class ActualizarPerfilDTO
{
    [RegularExpression(@"^\+?[0-9\s-]{8,20}$", ErrorMessage = "Teléfono inválido")]
    public string? Telefono { get; set; }
    [RegularExpression("^(email|whatsapp|ambos)$", ErrorMessage = "Método de notificación inválido")]
    public string? MetodoNotificacion { get; set; }
    [MaxLength(7_000_000, ErrorMessage = "La fotografía excede el tamaño permitido")]
    public string? FotoBase64 { get; set; }
    [MaxLength(7_000_000, ErrorMessage = "La fotografía modificada excede el tamaño permitido")]
    public string? FotoModificadaBase64 { get; set; }
    public string? PasswordActual { get; set; }
    [MinLength(8), MaxLength(128)]
    public string? NuevoPassword { get; set; }
}
