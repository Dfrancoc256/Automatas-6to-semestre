using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesFormalesAPI.Models;

[Table("tokens_restablecimiento_password")]
public class PasswordResetToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    // Solo se persiste el hash SHA-256; el token original llega únicamente por correo.
    [Required, MaxLength(64)]
    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("expira_en")]
    public DateTime ExpiraEn { get; set; }

    [Column("usado_en")]
    public DateTime? UsadoEn { get; set; }

    [Column("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }
}
