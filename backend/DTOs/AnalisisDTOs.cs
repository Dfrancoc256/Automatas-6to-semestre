using System.ComponentModel.DataAnnotations;

namespace LenguajesFormalesAPI.DTOs;

public class AnalisisRequestDTO
{
    [Required(ErrorMessage = "El idioma es obligatorio")]
    [RegularExpression("^(español|inglés|ruso|chino|árabe)$", ErrorMessage = "Idioma no soportado")]
    public string Idioma { get; set; } = "español"; // "español", "inglés", "ruso", "chino", "árabe"

    [Required(ErrorMessage = "El contenido del archivo es obligatorio")]
    [MaxLength(5_000_000, ErrorMessage = "El archivo excede el tamaño permitido")]
    public string Contenido { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[^\\/:*?\""<>|]+\.txt$", ErrorMessage = "El archivo debe tener extensión .txt")]
    [MaxLength(255)]
    public string NombreArchivo { get; set; } = "archivo.txt";
}

public class FrecuenciaToken
{
    public string Token { get; set; } = string.Empty;
    public int Frecuencia { get; set; }
}

public class AnalisisResultadoDTO
{
    public int TotalPalabras { get; set; }
    public int TotalTokens { get; set; }
    public int TotalOraciones { get; set; }
    public int TotalParrafos { get; set; }
    public double PromedioLongitudPalabra { get; set; }

    public List<FrecuenciaToken> PalabrasMasFrecuentes { get; set; } = new();
    public List<FrecuenciaToken> PalabrasMenosFrecuentes { get; set; } = new();

    public List<string> Pronombres { get; set; } = new();
    public List<string> NombresPersonas { get; set; } = new();
    public List<string> Sustantivos { get; set; } = new();
    public List<string> Verbos { get; set; } = new();
    public List<string> Adjetivos { get; set; } = new();
    public List<string> Numeros { get; set; } = new();
    public List<string> Conectores { get; set; } = new();
    public List<string> Preposiciones { get; set; } = new();

    // Patrones adicionales
    public List<string> CorreosEncontrados { get; set; } = new();
    public List<string> UrlsEncontradas { get; set; } = new();
    public List<string> FechasEncontradas { get; set; } = new();
    public List<string> HorasEncontradas { get; set; } = new();

    public string Idioma { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime FechaAnalisis { get; set; } = DateTime.UtcNow;
    public int AnalisisId { get; set; }
}

public class HistorialAnalisisDTO
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Idioma { get; set; } = string.Empty;
    public int TotalPalabras { get; set; }
    public DateTime FechaAnalisis { get; set; }
}
