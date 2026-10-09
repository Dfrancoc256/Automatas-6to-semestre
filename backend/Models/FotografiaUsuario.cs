using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesFormalesAPI.Models;

[Table("fotografias_usuario")]
public class FotografiaUsuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Required]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [Column("ubicacion_almacen")]
    public string UbicacionAlmacen { get; set; } = string.Empty;

    [Required]
    [Column("hash_contenido")]
    public string HashContenido { get; set; } = string.Empty;

    [Required]
    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Required]
    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; }

    [Column("reemplazada_en")]
    public DateTime? ReemplazadaEn { get; set; }

    public Usuario Usuario { get; set; } = null!;
}