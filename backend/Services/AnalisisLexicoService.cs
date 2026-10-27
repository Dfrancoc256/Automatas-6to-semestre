using System.Text.RegularExpressions;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LenguajesFormalesAPI.Services;

public interface IAnalisisLexicoService
{
    AnalisisResultadoDTO Analizar(string contenido, string idioma, string nombreArchivo);
    ValidacionIdiomaDTO ValidarIdioma(string contenido, string idioma);
}

public class AnalisisLexicoService : IAnalisisLexicoService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AnalisisLexicoService> _logger;

    public AnalisisLexicoService(AppDbContext db, ILogger<AnalisisLexicoService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static readonly HashSet<string> IndicadoresEspanol = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "la", "el", "los", "las", "y", "en", "que", "por", "para", "con", "una", "un", "del", "se", "es", "como", "más", "su", "al"
    };
    private static readonly HashSet<string> IndicadoresIngles = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "of", "and", "to", "in", "is", "for", "that", "with", "on", "this", "as", "are", "be", "by", "from", "or", "an", "it", "at"
    };
    private static readonly Regex _patronCorreo =
        new(@"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled);

    private static readonly Regex _patronUrl =
        new(@"https?://[^\s""'<>]+|www\.[^\s""'<>]+", RegexOptions.Compiled);

    private static readonly Regex _patronFecha =
        new(@"\b\d{1,2}[/\-]\d{1,2}[/\-]\d{2,4}\b|\b\d{4}[/\-]\d{1,2}[/\-]\d{1,2}\b", RegexOptions.Compiled);

    private static readonly Regex _patronHora =
        new(@"\b\d{1,2}:\d{2}(:\d{2})?\s*(AM|PM|am|pm)?\b", RegexOptions.Compiled);

    private static readonly Regex _patronNumero =
        new(@"\b\d+([.,]\d+)?\b", RegexOptions.Compiled);

    private static readonly Regex _patronCirilico = new(@"[\u0400-\u04FF]", RegexOptions.Compiled);
    private static readonly Regex _patronChino = new(@"[\u4E00-\u9FFF]", RegexOptions.Compiled);
    private static readonly Regex _patronArabe = new(@"[\u0600-\u06FF]", RegexOptions.Compiled);
    private static readonly Regex _patronLetras = new(@"\p{L}", RegexOptions.Compiled);

    public ValidacionIdiomaDTO ValidarIdioma(string contenido, string idioma)
    {
        var letras = _patronLetras.Matches(contenido).Count;
        var palabras = Regex.Matches(contenido.ToLowerInvariant(), @"[\p{L}]+")
            .Select(coincidencia => coincidencia.Value)
            .ToList();

        var unidadesSuficientes = idioma == "chino"
            ? _patronChino.Matches(contenido).Count >= 6
            : palabras.Count >= 6;
        if (letras < 20 || !unidadesSuficientes)
        {
            return Rechazar(idioma == "chino"
                ? "El documento es demasiado corto para validar chino. Usa al menos veinte caracteres chinos."
                : "El documento es demasiado corto para validar su idioma. Usa al menos seis palabras con contenido.");
        }

        var conteosEscritura = new Dictionary<string, int>
        {
            ["ruso"] = _patronCirilico.Matches(contenido).Count,
            ["chino"] = _patronChino.Matches(contenido).Count,
            ["árabe"] = _patronArabe.Matches(contenido).Count
        };
        var idiomaPorEscritura = conteosEscritura.MaxBy(item => item.Value);

        if (idiomaPorEscritura.Value > 0 && idiomaPorEscritura.Value * 100 >= letras * 35)
        {
            if (idioma != idiomaPorEscritura.Key)
                return Rechazar($"El documento parece estar en {NombreIdioma(idiomaPorEscritura.Key)}. Selecciona ese idioma para procesarlo correctamente.", idiomaPorEscritura.Key);

            if (idiomaPorEscritura.Value * 100 < letras * 55)
                return Rechazar($"No hay suficiente contenido en {NombreIdioma(idioma)} para un análisis confiable.", idioma);

            return Aceptar();
        }

        if (idioma is "ruso" or "chino" or "árabe")
            return Rechazar($"No se detectó escritura compatible con {NombreIdioma(idioma)}. Revisa el idioma seleccionado.");

        var puntajeEspanol = palabras.Count(palabra => IndicadoresEspanol.Contains(palabra));
        var puntajeIngles = palabras.Count(palabra => IndicadoresIngles.Contains(palabra));
        var puntajeSeleccionado = idioma == "español" ? puntajeEspanol : puntajeIngles;
        var puntajeAlterno = idioma == "español" ? puntajeIngles : puntajeEspanol;
        var idiomaAlterno = idioma == "español" ? "inglés" : "español";

        if (puntajeSeleccionado < 2 || puntajeAlterno > puntajeSeleccionado)
        {
            var detectado = puntajeAlterno > puntajeSeleccionado ? idiomaAlterno : null;
            var mensaje = detectado is null
                ? $"No se encontraron señales suficientes de {NombreIdioma(idioma)}. Revisa el idioma seleccionado o utiliza un documento más extenso."
                : $"El documento parece estar en {NombreIdioma(detectado)}. Selecciona ese idioma para procesarlo correctamente.";
            return Rechazar(mensaje, detectado);
        }

        return Aceptar();
    }

    private static ValidacionIdiomaDTO Aceptar() => new() { EsValido = true };

    private static ValidacionIdiomaDTO Rechazar(string mensaje, string? idiomaDetectado = null) => new()
    {
        EsValido = false,
        Mensaje = mensaje,
        IdiomaDetectado = idiomaDetectado
    };

    private static string NombreIdioma(string idioma) => idioma switch
    {
        "español" => "español",
        "inglés" => "inglés",
        "ruso" => "ruso",
        "chino" => "chino",
        "árabe" => "árabe",
        _ => idioma
    };

    // ──────────────────────────────────────────────────────────────
    public AnalisisResultadoDTO Analizar(string contenido, string idioma, string nombreArchivo)
    {
        var lexico = PrepararLexico(CargarEntradas(idioma));
        var automata = CrearAutomata(lexico);

        // Patrones especiales antes de tokenizar
        var correos  = _patronCorreo.Matches(contenido).Select(m => m.Value).Distinct().ToList();
        var urls     = _patronUrl.Matches(contenido).Select(m => m.Value).Distinct().ToList();
        var fechas   = _patronFecha.Matches(contenido).Select(m => m.Value).Distinct().ToList();
        var horas    = _patronHora.Matches(contenido)
            .Select(m => m.Value.Trim())
            .Distinct()
            .ToList();
        var numeros  = _patronNumero.Matches(contenido).Select(m => m.Value).Distinct().ToList();

        // Párrafos y oraciones
        var parrafos  = contenido.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var oraciones = Regex.Split(contenido, @"(?<=[.!?])\s+|(?<=[。！？؟])\s*")
            .Where(oracion => !string.IsNullOrWhiteSpace(oracion))
            .ToArray();

        var tokens = Tokenizar(contenido, idioma, automata);

        var totalTokens  = tokens.Count;
        var totalPalabras = tokens.Count;

        // Frecuencia
        var frecuencia = tokens
            .GroupBy(t => t.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.Count());

        var masFrecuentes = frecuencia
            .OrderByDescending(kv => kv.Value)
            .Take(20)
            .Select(kv => new FrecuenciaToken { Token = kv.Key, Frecuencia = kv.Value })
            .ToList();

        var menosFrecuentes = frecuencia
            .Where(kv => kv.Value == 1)
            .Take(20)
            .Select(kv => new FrecuenciaToken { Token = kv.Key, Frecuencia = kv.Value })
            .ToList();

        // Clasificación: el AFD recorre cada token aplicando δ(q, símbolo).
        var reconocimientos = tokens
            .Select(token => token.ToLowerInvariant())
            .Distinct()
            .Select(token => Reconocer(token, automata))
            .ToList();
        reconocimientos.AddRange(EncontrarConectoresCompuestos(tokens, lexico["conector"], automata));

        var pronombresEncontrados = ObtenerCategoria(reconocimientos, "pronombre");
        var verbosEncontrados = ObtenerCategoria(reconocimientos, "verbo");
        var conectoresEncontrados = ObtenerCategoria(reconocimientos, "conector");
        var preposicionesEncontradas = ObtenerCategoria(reconocimientos, "preposición");

        // Solo los nombres registrados en la tabla; una mayúscula no es evidencia suficiente.
        var nombresPersonas = ObtenerCategoria(reconocimientos, "nombre_propio")
            .Take(30)
            .ToList();

        // Solo las entradas explícitas son sustantivos; repetir una palabra no lo demuestra.
        var sustantivos = ObtenerCategoria(reconocimientos, "sustantivo")
            .Take(25)
            .ToList();

        // Los adjetivos también dependen de entradas explícitas.
        var adjetivos = ObtenerCategoria(reconocimientos, "adjetivo")
            .Take(20)
            .ToList();

        var promLongitud = tokens.Any() ? tokens.Average(t => t.Length) : 0;

        return new AnalisisResultadoDTO
        {
            TotalPalabras          = totalPalabras,
            TotalTokens            = totalTokens,
            TotalOraciones         = oraciones.Length,
            TotalParrafos          = parrafos.Length,
            PromedioLongitudPalabra = Math.Round(promLongitud, 2),
            PalabrasMasFrecuentes  = masFrecuentes,
            PalabrasMenosFrecuentes = menosFrecuentes,
            Pronombres             = pronombresEncontrados,
            NombresPersonas        = nombresPersonas,
            Sustantivos            = sustantivos,
            Verbos                 = verbosEncontrados,
            Adjetivos              = adjetivos,
            Numeros                = numeros.Take(20).ToList(),
            Conectores             = conectoresEncontrados,
            Preposiciones          = preposicionesEncontradas,
            CorreosEncontrados     = correos,
            UrlsEncontradas        = urls,
            FechasEncontradas      = fechas,
            HorasEncontradas       = horas,
            EstadosAutomata        = automata.Estados.Count,
            SimbolosAutomata       = automata.Alfabeto.Count,
            TransicionesAutomata   = automata.CantidadTransiciones,
            RecorridosAutomata     = reconocimientos
                .OrderByDescending(reconocimiento => reconocimiento.Resultado.Aceptado)
                .ThenBy(reconocimiento => reconocimiento.Token)
                .Take(25)
                .Select(reconocimiento => new RecorridoAutomataDTO
                {
                    Token = reconocimiento.Token,
                    Aceptado = reconocimiento.Resultado.Aceptado,
                    Categorias = reconocimiento.CategoriaPrincipal is null
                        ? new List<string>()
                        : new List<string> { reconocimiento.CategoriaPrincipal },
                    Estados = reconocimiento.Resultado.EstadosRecorridos.ToList()
                })
                .ToList(),
            Idioma                 = idioma,
            NombreArchivo          = nombreArchivo,
            FechaAnalisis          = DateTime.UtcNow
        };
    }

    private List<EntradaLexica> CargarEntradas(string idioma)
    {
        try
        {
            return _db.EntradasLexicas.AsNoTracking()
                .Where(entrada => entrada.Idioma == idioma)
                .ToList();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            // Sin la tabla, el análisis de frecuencia sigue disponible, pero no hay categorías.
            _logger.LogWarning("Falta la tabla lexico_entradas; la clasificación quedará vacía hasta crearla.");
            return new List<EntradaLexica>();
        }
    }

    private static Dictionary<string, HashSet<string>> PrepararLexico(
        IEnumerable<EntradaLexica> entradas)
    {
        var lexico = new Dictionary<string, HashSet<string>>
        {
            ["nombre_propio"] = new(StringComparer.OrdinalIgnoreCase),
            ["sustantivo"] = new(StringComparer.OrdinalIgnoreCase),
            ["pronombre"] = new(StringComparer.OrdinalIgnoreCase),
            ["verbo"] = new(StringComparer.OrdinalIgnoreCase),
            ["conector"] = new(StringComparer.OrdinalIgnoreCase),
            ["preposición"] = new(StringComparer.OrdinalIgnoreCase),
            ["adjetivo"] = new(StringComparer.OrdinalIgnoreCase)
        };

        foreach (var entrada in entradas)
        {
            var palabra = entrada.Palabra.Trim().ToLowerInvariant();
            if (palabra.Length == 0 || !lexico.TryGetValue(entrada.Categoria, out var categoria)) continue;

            categoria.Add(palabra);
        }

        return lexico;
    }

    private static AutomataFinitoDeterminista CrearAutomata(
        Dictionary<string, HashSet<string>> lexico) =>
        AutomataFinitoDeterminista.CrearDesdeLexico(lexico.ToDictionary(
            categoria => categoria.Key,
            categoria => (IEnumerable<string>)categoria.Value));

    private static readonly string[] PrioridadCategorias =
    [
        "nombre_propio", "pronombre", "preposición", "conector", "verbo", "adjetivo", "sustantivo"
    ];

    private static TokenReconocido Reconocer(string token, AutomataFinitoDeterminista automata)
    {
        var resultado = automata.Evaluar(token);
        var categoria = PrioridadCategorias.FirstOrDefault(resultado.Categorias.Contains);
        return new TokenReconocido(token, resultado, categoria);
    }

    private static List<string> ObtenerCategoria(
        IEnumerable<TokenReconocido> reconocimientos,
        string categoria) => reconocimientos
            .Where(reconocimiento => reconocimiento.CategoriaPrincipal == categoria)
            .Select(reconocimiento => reconocimiento.Token)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(token => token)
            .ToList();

    private static IEnumerable<TokenReconocido> EncontrarConectoresCompuestos(
        IReadOnlyList<string> tokens,
        HashSet<string> conectores,
        AutomataFinitoDeterminista automata)
    {
        var expresiones = conectores
            .Where(expresion => expresion.Contains(' '))
            .Select(expresion => expresion.ToLowerInvariant())
            .ToList();
        if (expresiones.Count == 0) yield break;

        var primerosTerminos = expresiones
            .Select(expresion => expresion.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var longitudMaxima = expresiones
            .Max(expresion => expresion.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

        for (var inicio = 0; inicio < tokens.Count; inicio++)
        {
            if (!primerosTerminos.Contains(tokens[inicio])) continue;

            for (var longitud = 2; longitud <= longitudMaxima && inicio + longitud <= tokens.Count; longitud++)
            {
                var candidato = string.Join(' ', tokens.Skip(inicio).Take(longitud)).ToLowerInvariant();
                var resultado = automata.Evaluar(candidato);
                if (resultado.Aceptado && resultado.Categorias.Contains("conector", StringComparer.OrdinalIgnoreCase))
                    yield return new TokenReconocido(candidato, resultado, "conector");
            }
        }
    }

    private static List<string> Tokenizar(
        string contenido, string idioma, AutomataFinitoDeterminista automata)
    {
        if (idioma != "chino")
        {
            return Regex.Matches(contenido, @"[\p{L}]+")
                .Select(match => match.Value)
                .ToList();
        }

        var tokens = new List<string>();

        foreach (Match coincidencia in Regex.Matches(contenido, @"[\u4E00-\u9FFF]+|[\p{L}]+"))
        {
            var bloque = coincidencia.Value;
            if (!Regex.IsMatch(bloque, @"^[\u4E00-\u9FFF]+$"))
            {
                tokens.Add(bloque);
                continue;
            }

            for (var posicion = 0; posicion < bloque.Length;)
            {
                // Segmentación por recorrido AFD: elige la coincidencia aceptada más larga.
                var coincidenciaAfD = automata.BuscarPrefijoMasLargo(bloque, posicion);
                if (coincidenciaAfD is null)
                {
                    tokens.Add(bloque[posicion].ToString());
                    posicion++;
                }
                else
                {
                    tokens.Add(bloque.Substring(posicion, coincidenciaAfD.Longitud));
                    posicion += coincidenciaAfD.Longitud;
                }
            }
        }

        return tokens;
    }

    private sealed record TokenReconocido(
        string Token, ResultadoAutomata Resultado, string? CategoriaPrincipal);
}
