using System.ComponentModel.DataAnnotations;

namespace LenguajesFormalesAPI.DTOs;

public class RegisterDTO
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe contener entre 2 y 80 caracteres")]
    [RegularExpression(@"^[\p{L}][\p{L}\s'-]*$", ErrorMessage = "El nombre solo puede contener letras, espacios, apóstrofes o guiones")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe contener entre 2 y 80 caracteres")]
    [RegularExpression(@"^[\p{L}][\p{L}\s'-]*$", ErrorMessage = "El apellido solo puede contener letras, espacios, apóstrofes o guiones")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [RegularExpression(@"^\+?[0-9\s-]{8,20}$", ErrorMessage = "Teléfono inválido")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El método de notificación es obligatorio")]
    [RegularExpression("^(email|whatsapp|ambos)$", ErrorMessage = "Método de notificación inválido")]
    public string MetodoNotificacion { get; set; } = "email"; // "email", "whatsapp", "ambos"

    // Foto en base64 (tomada por webcam)
    [MaxLength(7_000_000, ErrorMessage = "La fotografía excede el tamaño permitido")]
    public string? FotoBase64 { get; set; }

    // Foto para avatar y credencial.
    [MaxLength(7_000_000, ErrorMessage = "La fotografía personalizada excede el tamaño permitido")]
    public string? FotoModificadaBase64 { get; set; }

    // Descriptor facial.
    public List<double>? Descriptor { get; set; }

    // Token de reCAPTCHA
    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;

    public string? GoogleRegistroToken { get; set; }
}

public class LoginDTO
{
    [Required(ErrorMessage = "El nickname o correo es obligatorio")]
    [MinLength(3, ErrorMessage = "El identificador debe tener al menos 3 caracteres")]
    [MaxLength(30, ErrorMessage = "El correo o nickname no puede superar 30 caracteres")]
    public string Identificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [MaxLength(35, ErrorMessage = "La contraseña no puede superar 35 caracteres")]
    public string Password { get; set; } = string.Empty;

    // Token de reCAPTCHA
    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class LoginFacialDTO
{
    // Descriptor facial.
    [Required(ErrorMessage = "No se detectó un rostro en la cámara.")]
    [MinLength(1)]
    public List<double> Descriptor { get; set; } = new();

}

public class EnrolarFacialDTO
{
    [Required(ErrorMessage = "No se detectó un rostro en la cámara.")]
    [MinLength(1)]
    public List<double> Descriptor { get; set; } = new();
}

public class LoginQrDTO
{
    [Required(ErrorMessage = "El código QR es obligatorio")]
    [MaxLength(2048, ErrorMessage = "El código QR no tiene un formato válido")]
    public string CodigoQr { get; set; } = string.Empty;

    [Required]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class ResetPasswordDTO
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class ConfirmarResetPasswordDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?=.{8,128}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$",
        ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres, mayúscula, minúscula, número y carácter especial")]
    public string NuevoPassword { get; set; } = string.Empty;
}

public class CambiarPasswordDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NuevoPassword { get; set; } = string.Empty;
}

public class CambiarPasswordTemporalDTO
{
    [Required]
    [RegularExpression(@"^(?=.{8,35}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$",
        ErrorMessage = "La contraseña debe tener entre 8 y 35 caracteres, mayúscula, minúscula, número y carácter especial")]
    public string NuevoPassword { get; set; } = string.Empty;
}

public class GoogleRegistroTokenDTO
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class RecaptchaTokenDTO
{
    [Required(ErrorMessage = "Verificación reCAPTCHA requerida")]
    public string RecaptchaToken { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? FotoModificada { get; set; }
    public DateTime Expiracion { get; set; }
    public string? CodigoQr { get; set; }
    public bool DebeCambiarPassword { get; set; }
    // Nunca expone la clave: solo confirma que el correo con la clave temporal salió del servidor.
    public bool PasswordTemporalEnviada { get; set; }
}

/// <summary>Datos mínimos de una sesión ya validada por la cookie HttpOnly.</summary>
public class SesionActualDTO
{
    public string Rol { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? FotoModificada { get; set; }
    public DateTime Expiracion { get; set; }
    public bool DebeCambiarPassword { get; set; }
}

public class UsuarioPerfilDTO
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string MetodoNotificacion { get; set; } = string.Empty;
    public string? FotoModificada { get; set; }
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public bool TieneRostroEnrolado { get; set; }
}

public class EnviarCredencialDTO
{
    /// <summary>"email" o "whatsapp"</summary>
    [Required(ErrorMessage = "Indica el canal de envío")]
    [RegularExpression("^(email|whatsapp)$", ErrorMessage = "Canal inválido")]
    public string Canal { get; set; } = string.Empty;
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
    [MinLength(8), MaxLength(35)]
    public string? NuevoPassword { get; set; }
    public string? ConfirmarNuevoPassword { get; set; }
}
