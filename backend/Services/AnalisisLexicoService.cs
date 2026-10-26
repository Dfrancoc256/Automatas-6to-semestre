using System.Text.RegularExpressions;
using LenguajesFormalesAPI.DTOs;

namespace LenguajesFormalesAPI.Services;

public interface IAnalisisLexicoService
{
    AnalisisResultadoDTO Analizar(string contenido, string idioma, string nombreArchivo);
    ValidacionIdiomaDTO ValidarIdioma(string contenido, string idioma);
}

public class AnalisisLexicoService : IAnalisisLexicoService
{
    private static readonly HashSet<string> PalabrasVacias = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> IndicadoresEspanol = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "la", "el", "los", "las", "y", "en", "que", "por", "para", "con", "una", "un", "del", "se", "es", "como", "más", "su", "al"
    };
    private static readonly HashSet<string> IndicadoresIngles = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "of", "and", "to", "in", "is", "for", "that", "with", "on", "this", "as", "are", "be", "by", "from", "or", "an", "it", "at"
    };
    // ──────────────────────────────────────────────────────────────
    // Listas léxicas por idioma
    // ──────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, HashSet<string>> Pronombres = new()
    {
        ["español"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "yo","tú","él","ella","nosotros","nosotras","vosotros","vosotras",
            "ellos","ellas","usted","ustedes","me","te","se","nos","os","le","les",
            "lo","la","los","las","mi","tu","su","mí","ti","sí"
        },
        ["inglés"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "i","you","he","she","it","we","they","me","him","her","us","them",
            "my","your","his","its","our","their","mine","yours","hers","ours","theirs",
            "myself","yourself","himself","herself","itself","ourselves","themselves"
        },
        ["ruso"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "я","ты","он","она","оно","мы","вы","они","меня","тебя","его","её","нас","вас","их"
        },
        ["chino"]  = new(StringComparer.OrdinalIgnoreCase) { "我","你","他","她","它","我们","你们","他们" },
        ["árabe"]  = new(StringComparer.OrdinalIgnoreCase) { "أنا","أنت","هو","هي","نحن","أنتم","هم" }
    };

    private static readonly Dictionary<string, HashSet<string>> Verbos = new()
    {
        ["español"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "ser","estar","tener","hacer","poder","decir","ir","ver","dar","saber",
            "querer","llegar","pasar","deber","poner","parecer","quedar","creer",
            "hablar","llevar","dejar","seguir","encontrar","llamar","venir","pensar",
            "salir","volver","tomar","conocer","vivir","sentir","tratar","mirar",
            "contar","empezar","esperar","buscar","existir","entrar","trabajar",
            "escribir","perder","producir","ocurrir","entender","pedir","recibir",
            "recordar","terminar","permitir","aparecer","conseguir","comenzar",
            "servir","sacar","necesitar","mantener","resultar","leer","caer",
            "cambiar","presentar","crear","abrir","considerar","oír","puede",
            "tiene","hace","va","ve","da"
        },
        ["inglés"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "be","have","do","say","go","get","make","know","think","take",
            "see","come","want","look","use","find","give","tell","work","call",
            "try","ask","need","feel","become","leave","put","mean","keep","let",
            "begin","show","hear","play","run","move","live","believe","hold","bring",
            "happen","write","provide","sit","stand","lose","pay","meet","include",
            "continue","set","learn","change","lead","understand","watch","follow",
            "stop","create","speak","read","spend","grow","open","walk","win","offer",
            "remember","love","consider","appear","buy","wait","serve","die","send","expect"
        },
        ["ruso"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "быть","есть","иметь","делать","мочь","сказать","идти","знать","видеть","дать",
            "хотеть","говорить","работать","жить","читать","писать","учиться","создать","использовать"
        },
        ["chino"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "是","有","在","做","说","去","看","给","想","知道","学习","阅读","写","使用","创建","喜欢"
        },
        ["árabe"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "يكون","كان","لديه","يفعل","يقول","يذهب","يرى","يعرف","يريد","يعمل","يقرأ","يكتب","يتعلم","يستخدم"
        }
    };

    private static readonly Dictionary<string, HashSet<string>> Conectores = new()
    {
        ["español"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "y","e","o","u","ni","pero","sino","aunque","sin embargo","no obstante",
            "además","también","tampoco","porque","ya que","puesto que","dado que",
            "pues","así que","por tanto","entonces","luego","por lo tanto","es decir",
            "o sea","esto es","en otras palabras","sin embargo","a pesar de","aunque",
            "si","cuando","mientras","donde","como","que","quien","cual","cuyo"
        },
        ["inglés"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "and","or","but","nor","yet","so","for","although","though","even though",
            "because","since","as","while","when","where","if","unless","until","after",
            "before","that","which","who","whom","whose","however","therefore","moreover",
            "furthermore","nevertheless","consequently","thus","hence","meanwhile"
        },
        ["ruso"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "и","или","но","а","потому что","так как","если","когда","однако","поэтому","также","затем"
        },
        ["chino"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "和","或","但是","因为","所以","如果","虽然","然而","同时","然后","而且"
        },
        ["árabe"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "و","أو","لكن","لأن","لذلك","إذا","رغم","مع ذلك","ثم","أيضًا"
        }
    };

    private static readonly Dictionary<string, HashSet<string>> Preposiciones = new()
    {
        ["español"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "a","ante","bajo","cabe","con","contra","de","desde","durante","en",
            "entre","hacia","hasta","mediante","para","por","según","sin","so",
            "sobre","tras","versus","vía"
        },
        ["inglés"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "in","on","at","by","for","with","about","against","between","into",
            "through","during","before","after","above","below","to","from","up",
            "down","out","off","over","under","around","along","following","across",
            "behind","beyond","plus","except","up","towards","upon"
        },
        ["ruso"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "в","на","с","к","из","у","для","по","о","об","над","под","между","перед","после"
        },
        ["chino"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "在","从","向","对","为","与","把","被","通过","关于"
        },
        ["árabe"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "في","من","إلى","على","عن","مع","ل","ب","بين","بعد","قبل"
        },
    };

    private static readonly Dictionary<string, HashSet<string>> AdjetivosLexicos = new()
    {
        ["español"] = new(StringComparer.OrdinalIgnoreCase) { "bueno","buena","grande","pequeño","pequeña","nuevo","nueva","viejo","vieja","importante","rápido","rápida","simple","complejo","compleja","formal","académico","académica" },
        ["inglés"] = new(StringComparer.OrdinalIgnoreCase) { "good","big","small","new","old","important","fast","simple","complex","formal","academic" },
        ["ruso"] = new(StringComparer.OrdinalIgnoreCase) { "большой","маленький","новый","старый","важный","красивый","быстрый","сильный","слабый" },
        ["chino"] = new(StringComparer.OrdinalIgnoreCase) { "好","大","小","新","旧","重要","快速","简单","复杂","正式" },
        ["árabe"] = new(StringComparer.OrdinalIgnoreCase) { "كبير","صغير","جديد","قديم","مهم","سريع","بسيط","رسمي" }
    };

    // Un AFD por idioma. Los léxicos definen el lenguaje aceptado por cada autómata.
    private static readonly Dictionary<string, AutomataFinitoDeterminista> AutomatasPorIdioma = new()
    {
        ["español"] = CrearAutomata("español"),
        ["inglés"] = CrearAutomata("inglés"),
        ["ruso"] = CrearAutomata("ruso"),
        ["chino"] = CrearAutomata("chino"),
        ["árabe"] = CrearAutomata("árabe")
    };

    // Patrones de nombres propios (palabras que inician con mayúscula, no al inicio de oración)
    private static readonly Regex _patronNombrePropio =
        new(@"(?<![.!?]\s)(?<![.!?]\s\s)\b[A-ZÁÉÍÓÚÜÑ][a-záéíóúüñ]{2,}\b", RegexOptions.Compiled);

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

        if (letras < 20 || palabras.Count < 6)
        {
            return Rechazar("El documento es demasiado corto para validar su idioma. Usa al menos seis palabras con contenido.");
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
        var oraciones = Regex.Split(contenido, @"(?<=[.!?])\s+")
            .Where(oracion => !string.IsNullOrWhiteSpace(oracion))
            .ToArray();

        var tokens = Tokenizar(contenido, idioma);

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
        var automata = AutomatasPorIdioma[idioma];
        var reconocimientos = tokens
            .Select(token => token.ToLowerInvariant())
            .Distinct()
            .Select(token => new TokenReconocido(token, automata.Evaluar(token)))
            .ToList();
        reconocimientos.AddRange(EncontrarConectoresCompuestos(tokens, idioma, automata));

        var pronomsSet = Pronombres.GetValueOrDefault(idioma, PalabrasVacias);
        var verbsSet   = Verbos.GetValueOrDefault(idioma, PalabrasVacias);
        var conecSet   = Conectores.GetValueOrDefault(idioma, PalabrasVacias);
        var prepSet    = Preposiciones.GetValueOrDefault(idioma, PalabrasVacias);

        var pronombresEncontrados = ObtenerCategoria(reconocimientos, "pronombre");
        var verbosEncontrados = ObtenerCategoria(reconocimientos, "verbo");
        var conectoresEncontrados = ObtenerCategoria(reconocimientos, "conector");
        var preposicionesEncontradas = ObtenerCategoria(reconocimientos, "preposición");

        // Nombres propios (heurística: mayúscula + no pronombre + no verbo + no al inicio)
        var stopWords = pronomsSet
            .Union(verbsSet)
            .Union(conecSet)
            .Union(prepSet)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nombresPersonas = idioma is "español" or "inglés"
            ? _patronNombrePropio.Matches(contenido)
                .Select(m => m.Value)
                .Where(n => !stopWords.Contains(n))
                .Distinct()
                .OrderBy(x => x)
                .Take(30)
                .ToList()
            : new List<string>();

        // Sustantivos (heurística: palabras largas, no clasificadas, frecuentes)
        var clasificadas = pronombresEncontrados
            .Union(verbosEncontrados)
            .Union(conectoresEncontrados)
            .Union(preposicionesEncontradas)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var longitudMinimaSustantivo = idioma == "chino" ? 1 : idioma == "árabe" ? 3 : 4;
        var sustantivos = frecuencia
            .Where(kv => kv.Key.Length >= longitudMinimaSustantivo
                      && kv.Value >= 2
                      && !clasificadas.Contains(kv.Key)
                      && !numeros.Contains(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .Take(25)
            .Select(kv => kv.Key)
            .ToList();

        var adjetivosSet = AdjetivosLexicos.GetValueOrDefault(idioma, PalabrasVacias);
        var adjetivos = ObtenerCategoria(reconocimientos, "adjetivo")
            .Union(tokens
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .Where(t => EsAdjetivo(t, idioma, adjetivosSet))
            .Where(t => !clasificadas.Contains(t)))
            .OrderBy(x => x)
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
                    Categorias = reconocimiento.Resultado.Categorias.ToList(),
                    Estados = reconocimiento.Resultado.EstadosRecorridos.ToList()
                })
                .ToList(),
            Idioma                 = idioma,
            NombreArchivo          = nombreArchivo,
            FechaAnalisis          = DateTime.UtcNow
        };
    }

    private static AutomataFinitoDeterminista CrearAutomata(string idioma) =>
        AutomataFinitoDeterminista.CrearDesdeLexico(
            new Dictionary<string, IEnumerable<string>>
            {
                ["pronombre"] = Pronombres[idioma],
                ["verbo"] = Verbos[idioma],
                ["conector"] = Conectores[idioma],
                ["preposición"] = Preposiciones[idioma],
                ["adjetivo"] = AdjetivosLexicos[idioma]
            });

    private static List<string> ObtenerCategoria(
        IEnumerable<TokenReconocido> reconocimientos,
        string categoria) => reconocimientos
            .Where(reconocimiento => reconocimiento.Resultado.Categorias.Contains(categoria, StringComparer.OrdinalIgnoreCase))
            .Select(reconocimiento => reconocimiento.Token)
            .OrderBy(token => token)
            .ToList();

    private static IEnumerable<TokenReconocido> EncontrarConectoresCompuestos(
        IReadOnlyList<string> tokens,
        string idioma,
        AutomataFinitoDeterminista automata)
    {
        var expresiones = Conectores[idioma]
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
                    yield return new TokenReconocido(candidato, resultado);
            }
        }
    }

    private static List<string> Tokenizar(string contenido, string idioma)
    {
        if (idioma != "chino")
        {
            return Regex.Matches(contenido, @"[\p{L}]+")
                .Select(match => match.Value)
                .ToList();
        }

        var automata = AutomatasPorIdioma["chino"];
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

    private static bool EsAdjetivo(string palabra, string idioma, HashSet<string> adjetivosLexicos)
    {
        if (adjetivosLexicos.Contains(palabra)) return true;

        return idioma switch
        {
            "español" => Regex.IsMatch(palabra, @"(oso|osa|ble|ivo|iva|ado|ada|ido|ida|ante|ente|al|ar)$"),
            "inglés" => Regex.IsMatch(palabra, @"(ful|less|ive|ous|al|ible|able|ent|ant|ary|ory|ic)$"),
            "ruso" => Regex.IsMatch(palabra, @"(ый|ий|ой|ая|яя|ое|ее|ые|ие)$"),
            _ => false
        };
    }

    private sealed record TokenReconocido(string Token, ResultadoAutomata Resultado);
}
