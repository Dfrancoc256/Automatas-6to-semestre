using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesFormalesAPI.Models;

[Table("bitacora_login")]
public class BitacoraLogin
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("fecha_hora")]
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    [Column("ip_origen")]
    public string? IpOrigen { get; set; }

    [MaxLength(300)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    // "exitoso", "fallido"
    [MaxLength(20)]
    [Column("resultado")]
    public string Resultado { get; set; } = "exitoso";

    [MaxLength(20)]
    [Column("metodo")]
    public string Metodo { get; set; } = "password"; // "password", "facial", "qr"

    [ForeignKey("UsuarioId")]
    public Usuario? Usuario { get; set; }
}
