namespace LenguajesFormalesAPI.Services;

/// <summary>
/// Autómata finito determinista (AFD) construido a partir de un léxico.
/// Cada estado representa un prefijo; q0 es el estado inicial y los estados
/// finales almacenan una o más categorías léxicas.
/// </summary>
public sealed class AutomataFinitoDeterminista
{
    private readonly Dictionary<(int Estado, char Simbolo), int> _delta;
    private readonly Dictionary<int, HashSet<string>> _estadosFinales;

    public IReadOnlyCollection<int> Estados { get; }
    public IReadOnlyCollection<char> Alfabeto { get; }
    public int EstadoInicial => 0;
    public int EstadoPozo { get; }
    public int CantidadTransiciones => _delta.Count;

    private AutomataFinitoDeterminista(
        Dictionary<(int Estado, char Simbolo), int> delta,
        Dictionary<int, HashSet<string>> estadosFinales,
        HashSet<int> estados,
        HashSet<char> alfabeto,
        int estadoPozo)
    {
        _delta = delta;
        _estadosFinales = estadosFinales;
        Estados = estados;
        Alfabeto = alfabeto;
        EstadoPozo = estadoPozo;
    }

    /// <summary>Procesa la entrada símbolo por símbolo aplicando δ(q, a).</summary>
    public ResultadoAutomata Evaluar(string entrada)
    {
        var estado = EstadoInicial;
        var recorrido = new List<int> { estado };

        foreach (var simbolo in entrada.ToLowerInvariant())
        {
            if (!Alfabeto.Contains(simbolo))
            {
                recorrido.Add(EstadoPozo);
                return new ResultadoAutomata(false, Array.Empty<string>(), recorrido);
            }

            estado = _delta[(estado, simbolo)];
            recorrido.Add(estado);
        }

        var categorias = _estadosFinales.TryGetValue(estado, out var aceptadas)
            ? aceptadas.OrderBy(categoria => categoria).ToList()
            : new List<string>();

        return new ResultadoAutomata(categorias.Count > 0, categorias, recorrido);
    }

    /// <summary>
    /// Devuelve la palabra aceptada más larga que comienza en una posición.
    /// Se utiliza para segmentar chino, donde normalmente no hay espacios.
    /// </summary>
    public CoincidenciaPrefijoAutomata? BuscarPrefijoMasLargo(string texto, int inicio)
    {
        var estado = EstadoInicial;
        var recorrido = new List<int> { estado };
        CoincidenciaPrefijoAutomata? mejorCoincidencia = null;

        for (var indice = inicio; indice < texto.Length; indice++)
        {
            var simbolo = char.ToLowerInvariant(texto[indice]);
            if (!Alfabeto.Contains(simbolo)) break;

            estado = _delta[(estado, simbolo)];
            recorrido.Add(estado);
            if (estado == EstadoPozo) break;

            if (_estadosFinales.TryGetValue(estado, out var categorias))
            {
                mejorCoincidencia = new CoincidenciaPrefijoAutomata(
                    indice - inicio + 1,
                    categorias.OrderBy(categoria => categoria).ToList(),
                    recorrido.ToList());
            }
        }

        return mejorCoincidencia;
    }

    /// <summary>
    /// Construye un AFD total. Las transiciones no definidas en el trie se
    /// redirigen al estado pozo, de modo que δ está definida para todo q∈Q y a∈Σ.
    /// </summary>
    public static AutomataFinitoDeterminista CrearDesdeLexico(
        IReadOnlyDictionary<string, IEnumerable<string>> categorias)
    {
        var delta = new Dictionary<(int Estado, char Simbolo), int>();
        var finales = new Dictionary<int, HashSet<string>>();
        var estados = new HashSet<int> { 0 };
        var alfabeto = new HashSet<char>();
        var siguienteEstado = 1;

        foreach (var (categoria, palabras) in categorias)
        {
            foreach (var palabraOriginal in palabras.Where(palabra => !string.IsNullOrWhiteSpace(palabra)))
            {
                var estado = 0;
                foreach (var simbolo in palabraOriginal.ToLowerInvariant())
                {
                    alfabeto.Add(simbolo);
                    var clave = (estado, simbolo);
                    if (!delta.TryGetValue(clave, out var destino))
                    {
                        destino = siguienteEstado++;
                        delta[clave] = destino;
                        estados.Add(destino);
                    }
                    estado = destino;
                }

                if (!finales.TryGetValue(estado, out var categoriasFinales))
                {
                    categoriasFinales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    finales[estado] = categoriasFinales;
                }
                categoriasFinales.Add(categoria);
            }
        }

        var estadoPozo = siguienteEstado;
        estados.Add(estadoPozo);
        foreach (var estado in estados)
        {
            foreach (var simbolo in alfabeto)
            {
                if (!delta.ContainsKey((estado, simbolo)))
                    delta[(estado, simbolo)] = estadoPozo;
            }
        }

        return new AutomataFinitoDeterminista(delta, finales, estados, alfabeto, estadoPozo);
    }
}

public sealed record ResultadoAutomata(
    bool Aceptado,
    IReadOnlyList<string> Categorias,
    IReadOnlyList<int> EstadosRecorridos);

public sealed record CoincidenciaPrefijoAutomata(
    int Longitud,
    IReadOnlyList<string> Categorias,
    IReadOnlyList<int> EstadosRecorridos);
