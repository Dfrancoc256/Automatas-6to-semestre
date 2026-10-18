using LenguajesFormalesAPI.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LenguajesFormalesAPI.Services;

public interface IAnalisisReporteService
{
    byte[] GenerarPdf(AnalisisResultadoDTO resultado);
}

public sealed class AnalisisReporteService : IAnalisisReporteService
{
    private static readonly string RutaLogo = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "logo-umg-oficial.png");

    static AnalisisReporteService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] GenerarPdf(AnalisisResultadoDTO resultado)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.MarginVertical(34);
            page.MarginHorizontal(42);
            page.DefaultTextStyle(style => style.FontFamily(Fonts.TimesNewRoman).FontSize(10).FontColor("111111"));

            page.Header().Element(contenedor => EncabezadoDocumento(contenedor));

            page.Content().PaddingTop(18).Column(column =>
            {
                column.Spacing(16);
                column.Item().Element(contenedor => CabeceraReporte(contenedor, resultado));
                column.Item().Element(contenedor => AgregarResumen(contenedor, resultado));
                column.Item().Element(contenedor => AgregarFrecuencias(contenedor, resultado));
                column.Item().PageBreak();
                column.Item().Element(contenedor => AgregarClasificacion(contenedor, resultado));
                column.Item().Element(contenedor => AgregarPatrones(contenedor, resultado));
            });

        })).GeneratePdf();
    }

    private static void EncabezadoDocumento(IContainer contenedor)
    {
        contenedor.Column(columna =>
        {
            columna.Item().Row(fila =>
            {
                var logo = fila.ConstantItem(39).Height(39).AlignCenter().AlignMiddle();
                if (File.Exists(RutaLogo))
                    logo.Image(RutaLogo).FitArea();

                fila.RelativeItem().PaddingLeft(9).Column(marca =>
                {
                    marca.Item().Text("Universidad Mariano Gálvez").Bold().FontSize(13).FontColor("111111");
                    marca.Item().PaddingTop(2).Text("Sistema de análisis léxico").FontSize(9).FontColor("444444");
                });
            });
        });
    }

    private static void CabeceraReporte(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.PaddingTop(12).Column(columna =>
        {
            columna.Item().Text("Reporte de análisis léxico").Bold().FontSize(17).FontColor("111111");
            columna.Item().PaddingTop(5).Text(resultado.NombreArchivo).FontSize(12).FontColor("222222");
            columna.Item().PaddingTop(7).Row(fila =>
            {
                fila.RelativeItem().Text(texto =>
                {
                    texto.Span("Idioma: ").FontColor("444444");
                    texto.Span(Titulo(resultado.Idioma)).Bold();
                });
                fila.RelativeItem().AlignRight().Text(texto =>
                {
                    texto.Span("Generado: ").FontColor("444444");
                    texto.Span(resultado.FechaAnalisis.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Bold();
                });
            });
        });
    }

    private static void AgregarResumen(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Resumen del documento");
            column.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn();
                    columnas.RelativeColumn();
                    columnas.RelativeColumn();
                    columnas.RelativeColumn();
                });

                Dato(tabla, "Palabras", resultado.TotalPalabras.ToString("N0"));
                Dato(tabla, "Oraciones", resultado.TotalOraciones.ToString("N0"));
                Dato(tabla, "Párrafos", resultado.TotalParrafos.ToString("N0"));
                Dato(tabla, "Promedio por palabra", $"{resultado.PromedioLongitudPalabra:0.##} caracteres");
            });
        });
    }

    private static void AgregarFrecuencias(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Frecuencia de palabras");
            column.Item().Border(1).BorderColor("C8CED4").Padding(10).Row(row =>
            {
                row.RelativeItem().PaddingRight(10).Element(celda =>
                    TablaFrecuencia(celda, "Mayor frecuencia", resultado.PalabrasMasFrecuentes.Take(8)));
                row.RelativeItem().PaddingLeft(10).Element(celda =>
                    TablaFrecuencia(celda, "Menor frecuencia", resultado.PalabrasMenosFrecuentes.Take(8)));
            });
        });
    }

    private static void TablaFrecuencia(IContainer contenedor, string titulo, IEnumerable<FrecuenciaToken> palabras)
    {
        contenedor.Column(column =>
        {
            column.Item().PaddingBottom(5).Text(titulo).Bold().FontColor("111111");
            column.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn(3);
                    columnas.RelativeColumn();
                });
                tabla.Header(cabecera =>
                {
                    cabecera.Cell().BorderBottom(1).BorderColor("9FA8B2").PaddingBottom(5).Text("Palabra").Bold();
                    cabecera.Cell().BorderBottom(1).BorderColor("9FA8B2").PaddingBottom(5).AlignRight().Text("Apariciones").Bold();
                });
                foreach (var palabra in palabras)
                {
                    tabla.Cell().BorderBottom(1).BorderColor("D7DCE1").PaddingVertical(5).Text(palabra.Token);
                    tabla.Cell().BorderBottom(1).BorderColor("D7DCE1").PaddingVertical(5).AlignRight().Text(palabra.Frecuencia.ToString()).Bold().FontColor("111111");
                }
            });
        });
    }

    private static void AgregarClasificacion(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Clasificación lingüística");
            column.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.ConstantColumn(130);
                    columnas.RelativeColumn();
                });

                FilaClasificacion(tabla, "Pronombres", resultado.Pronombres);
                FilaClasificacion(tabla, "Verbos", resultado.Verbos);
                FilaClasificacion(tabla, "Sustantivos", resultado.Sustantivos);
                FilaClasificacion(tabla, "Adjetivos", resultado.Adjetivos);
                FilaClasificacion(tabla, "Nombres de personas", resultado.NombresPersonas);
                FilaClasificacion(tabla, "Conectores", resultado.Conectores);
                FilaClasificacion(tabla, "Preposiciones", resultado.Preposiciones);
            });
        });
    }

    private static void AgregarPatrones(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Datos detectados");
            column.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.ConstantColumn(130);
                    columnas.RelativeColumn();
                });

                FilaClasificacion(tabla, "Correos electrónicos", resultado.CorreosEncontrados);
                FilaClasificacion(tabla, "Enlaces web", resultado.UrlsEncontradas);
                FilaClasificacion(tabla, "Fechas", resultado.FechasEncontradas);
                FilaClasificacion(tabla, "Números", resultado.Numeros);
            });
        });
    }

    private static void Encabezado(ColumnDescriptor columna, string titulo)
    {
        columna.Item().Text(titulo).Bold().FontSize(13).FontColor("111111");
    }

    private static void Dato(TableDescriptor tabla, string etiqueta, string valor) =>
        tabla.Cell().Border(1).BorderColor("D9E0E6").Padding(9).Column(columna =>
        {
            columna.Item().Text(valor).Bold().FontSize(13).FontColor("111111");
            columna.Item().PaddingTop(2).Text(etiqueta).FontSize(8).FontColor("444444");
        });

    private static void FilaClasificacion(TableDescriptor tabla, string etiqueta, IEnumerable<string> valores)
    {
        var encontrados = valores.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var texto = encontrados.Any() ? string.Join(", ", encontrados.Take(24)) : "Sin coincidencias";
        if (encontrados.Count > 24)
            texto += $" (+{encontrados.Count - 24} adicionales)";

        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(6).Text(etiqueta).Bold().FontColor("111111");
        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(6).Text(texto);
    }

    private static string Titulo(string valor) =>
        string.IsNullOrWhiteSpace(valor) ? "No indicado" : char.ToUpper(valor[0]) + valor[1..];

}
