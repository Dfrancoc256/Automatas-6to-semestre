using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LenguajesFormalesAPI.Models;

[Table("resultado_analisis")]
public class ResultadoAnalisis
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [MaxLength(30)]
    [Column("idioma")]
    public string Idioma { get; set; } = "español";

    [Column("nombre_archivo")]
    public string NombreArchivo { get; set; } = string.Empty;

    [Column("total_palabras")]
    public int TotalPalabras { get; set; }

    [Column("total_tokens")]
    public int TotalTokens { get; set; }

    // JSON serializado con el detalle del análisis
    [Column("detalle_json", TypeName = "jsonb")]
    public string DetalleJson { get; set; } = "{}";

    [Column("fecha_analisis")]
    public DateTime FechaAnalisis { get; set; } = DateTime.UtcNow;

    [ForeignKey("UsuarioId")]
    public Usuario? Usuario { get; set; }
}
