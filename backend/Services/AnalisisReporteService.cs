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
    static AnalisisReporteService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] GenerarPdf(AnalisisResultadoDTO resultado)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(38);
            page.DefaultTextStyle(style => style.FontFamily(Fonts.Arial).FontSize(9));

            page.Header().Column(header =>
            {
                header.Item().Text("UNIVERSIDAD MARIANO GÁLVEZ").Bold().FontSize(14).FontColor("051B2E");
                header.Item().Text("Reporte de análisis léxico").FontSize(11).FontColor("7A5E12");
                header.Item().PaddingTop(8).LineHorizontal(1).LineColor("D9E0E6");
            });

            page.Content().PaddingTop(18).Column(column =>
            {
                column.Spacing(14);
                column.Item().Text(resultado.NombreArchivo).Bold().FontSize(16).FontColor("17324D");
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Idioma: {Titulo(resultado.Idioma)}");
                    row.RelativeItem().AlignRight().Text($"Generado: {resultado.FechaAnalisis.ToLocalTime():dd/MM/yyyy HH:mm}");
                });

                column.Item().Element(contenedor => AgregarResumen(contenedor, resultado));
                column.Item().Element(contenedor => AgregarFrecuencias(contenedor, resultado));
                column.Item().Element(contenedor => AgregarClasificacion(contenedor, resultado));
                column.Item().Element(contenedor => AgregarPatrones(contenedor, resultado));
            });

            page.Footer().AlignCenter().Text("Analizador léxico · Universidad Mariano Gálvez")
                .FontSize(8).FontColor("6B7280");
        })).GeneratePdf();
    }

    private static void AgregarResumen(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Resumen");
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
                Dato(tabla, "Promedio", $"{resultado.PromedioLongitudPalabra} caracteres");
            });
        });
    }

    private static void AgregarFrecuencias(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Frecuencia de palabras");
            column.Item().Row(row =>
            {
                row.RelativeItem().PaddingRight(8).Element(celda =>
                    TablaFrecuencia(celda, "Más repetidas", resultado.PalabrasMasFrecuentes.Take(10)));
                row.RelativeItem().PaddingLeft(8).Element(celda =>
                    TablaFrecuencia(celda, "Menos repetidas", resultado.PalabrasMenosFrecuentes.Take(10)));
            });
        });
    }

    private static void TablaFrecuencia(IContainer contenedor, string titulo, IEnumerable<FrecuenciaToken> palabras)
    {
        contenedor.Column(column =>
        {
            column.Item().Text(titulo).Bold().FontColor("17324D");
            column.Item().PaddingTop(5).Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn(3);
                    columnas.RelativeColumn();
                });
                tabla.Header(cabecera =>
                {
                    cabecera.Cell().Background("EEF2F5").Padding(5).Text("Palabra").Bold();
                    cabecera.Cell().Background("EEF2F5").Padding(5).Text("Apariciones").Bold();
                });
                foreach (var palabra in palabras)
                {
                    tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").Padding(5).Text(palabra.Token);
                    tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").Padding(5).Text(palabra.Frecuencia.ToString());
                }
            });
        });
    }

    private static void AgregarClasificacion(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Clasificación");
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

    private static void Encabezado(ColumnDescriptor columna, string texto) =>
        columna.Item().Text(texto).Bold().FontSize(11).FontColor("051B2E");

    private static void Dato(TableDescriptor tabla, string etiqueta, string valor) =>
        tabla.Cell().Border(1).BorderColor("D9E0E6").Padding(8).Column(columna =>
        {
            columna.Item().Text(valor).Bold().FontSize(13).FontColor("17324D");
            columna.Item().Text(etiqueta).FontSize(8).FontColor("6B7280");
        });

    private static void FilaClasificacion(TableDescriptor tabla, string etiqueta, IEnumerable<string> valores)
    {
        var texto = valores.Any() ? string.Join(", ", valores.Take(35)) : "Sin coincidencias";
        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(5).Text(etiqueta).Bold();
        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(5).Text(texto);
    }

    private static string Titulo(string valor) =>
        string.IsNullOrWhiteSpace(valor) ? "No indicado" : char.ToUpper(valor[0]) + valor[1..];
}
