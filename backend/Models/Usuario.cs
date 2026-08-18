using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesFormalesAPI.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("correo")]
    public string Correo { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [Column("fecha_nacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [Required, MaxLength(50)]
    [Column("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    // "email", "whatsapp", "ambos"
    [MaxLength(20)]
    [Column("metodo_notificacion")]
    public string MetodoNotificacion { get; set; } = "email";

    [Column("foto_original")]
    public string? FotoOriginal { get; set; }

    [Column("foto_modificada")]
    public string? FotoModificada { get; set; }

    // "ADMIN", "SUPERVISOR", "ANALISTA"
    [Required, MaxLength(20)]
    [Column("rol")]
    public string Rol { get; set; } = "ANALISTA";

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Virtual: logs de acceso
    public ICollection<BitacoraLogin> BitacoraLogins { get; set; } = new List<BitacoraLogin>();
    public ICollection<ResultadoAnalisis> ResultadosAnalisis { get; set; } = new List<ResultadoAnalisis>();
}
