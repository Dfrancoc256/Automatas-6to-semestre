using LenguajesFormalesAPI.Services;
using Xunit;

namespace LenguajesFormalesAPI.Tests;

public class AnalisisLexicoServiceTests
{
    private readonly AnalisisLexicoService _service = new();

    [Fact]
    public void Analizar_ContenidoVacio_DevuelveContadoresEnCero()
    {
        var resultado = _service.Analizar(string.Empty, "español", "vacio.txt");

        Assert.Equal(0, resultado.TotalPalabras);
        Assert.Equal(0, resultado.TotalOraciones);
        Assert.Equal(0, resultado.TotalParrafos);
        Assert.Equal(0, resultado.PromedioLongitudPalabra);
    }

    [Fact]
    public void Analizar_TextoEspanol_CuentaYClasificaTokens()
    {
        var resultado = _service.Analizar(
            "Yo puedo leer y escribir. Tú puedes leer.", "español", "ejemplo.txt");

        Assert.Equal(8, resultado.TotalPalabras);
        Assert.Equal(2, resultado.TotalOraciones);
        Assert.Contains("yo", resultado.Pronombres);
        Assert.Contains("tú", resultado.Pronombres);
        Assert.Contains("leer", resultado.Verbos);
        Assert.Contains(resultado.PalabrasMasFrecuentes,
            token => token.Token == "leer" && token.Frecuencia == 2);
    }

    [Fact]
    public void Analizar_PatronesEspeciales_LosDetectaSinDuplicados()
    {
        const string texto = "Contacto qa@example.com a las 10:30 del 04/08/2026. " +
                             "Visite https://example.com y escriba a qa@example.com.";

        var resultado = _service.Analizar(texto, "español", "patrones.txt");

        Assert.Equal(["qa@example.com"], resultado.CorreosEncontrados);
        Assert.Equal(["https://example.com"], resultado.UrlsEncontradas);
        Assert.Equal(["04/08/2026"], resultado.FechasEncontradas);
        Assert.Equal(["10:30"], resultado.HorasEncontradas);
    }

    [Fact]
    public void Analizar_Unicode_ReconoceIdiomasConfigurados()
    {
        var ruso = _service.Analizar("я и ты", "ruso", "ruso.txt");
        var chino = _service.Analizar("我 你", "chino", "chino.txt");
        var arabe = _service.Analizar("أنا أنت", "árabe", "arabe.txt");

        Assert.Contains("я", ruso.Pronombres);
        Assert.Contains("我", chino.Pronombres);
        Assert.Contains("أنا", arabe.Pronombres);
    }
}
