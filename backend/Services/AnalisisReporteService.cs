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
            page.MarginVertical(34);
            page.MarginHorizontal(42);
            page.DefaultTextStyle(style => style.FontFamily(Fonts.Arial).FontSize(9).FontColor("27364A"));

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

            page.Footer().PaddingTop(8).Element(PieDocumento);
        })).GeneratePdf();
    }

    private static void EncabezadoDocumento(IContainer contenedor)
    {
        contenedor.Column(columna =>
        {
            columna.Item().Row(fila =>
            {
                fila.ConstantItem(34).Height(30).Background("B48B21").AlignCenter().AlignMiddle()
                    .Text("UMG").Bold().FontSize(9).FontColor(Colors.White);
                fila.RelativeItem().PaddingLeft(10).Column(marca =>
                {
                    marca.Item().Text("UNIVERSIDAD MARIANO GÁLVEZ").Bold().FontSize(11).FontColor("051B2E");
                    marca.Item().Text("Sistema de análisis léxico").FontSize(8).FontColor("6B7280");
                });
                fila.ConstantItem(115).AlignRight().AlignMiddle().Text("REPORTE ACADÉMICO")
                    .Bold().FontSize(8).LetterSpacing(0.8f).FontColor("9A7418");
            });
            columna.Item().PaddingTop(9).LineHorizontal(1).LineColor("D9E0E6");
        });
    }

    private static void CabeceraReporte(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Background("F5F7F9").BorderLeft(4).BorderColor("B48B21").Padding(14).Column(columna =>
        {
            columna.Item().Text("ANÁLISIS DE DOCUMENTO").Bold().FontSize(8).LetterSpacing(0.8f).FontColor("9A7418");
            columna.Item().PaddingTop(4).Text(resultado.NombreArchivo).Bold().FontSize(18).FontColor("17324D");
            columna.Item().PaddingTop(9).Row(fila =>
            {
                fila.RelativeItem().Text(texto =>
                {
                    texto.Span("Idioma: ").FontColor("6B7280");
                    texto.Span(Titulo(resultado.Idioma)).Bold();
                });
                fila.RelativeItem().AlignRight().Text(texto =>
                {
                    texto.Span("Generado: ").FontColor("6B7280");
                    texto.Span(resultado.FechaAnalisis.ToLocalTime().ToString("dd/MM/yyyy HH:mm")).Bold();
                });
            });
        });
    }

    private static void AgregarResumen(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Resumen del documento", "Indicadores generales del procesamiento");
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
            Encabezado(column, "Frecuencia de palabras", "Términos con mayor y menor aparición en el documento");
            column.Item().Row(row =>
            {
                row.RelativeItem().PaddingRight(8).Element(celda =>
                    TablaFrecuencia(celda, "Mayor frecuencia", resultado.PalabrasMasFrecuentes.Take(8)));
                row.RelativeItem().PaddingLeft(8).Element(celda =>
                    TablaFrecuencia(celda, "Menor frecuencia", resultado.PalabrasMenosFrecuentes.Take(8)));
            });
        });
    }

    private static void TablaFrecuencia(IContainer contenedor, string titulo, IEnumerable<FrecuenciaToken> palabras)
    {
        contenedor.Column(column =>
        {
            column.Item().Background("F5F7F9").Padding(8).Text(titulo).Bold().FontColor("17324D");
            column.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn(3);
                    columnas.RelativeColumn();
                });
                tabla.Header(cabecera =>
                {
                    cabecera.Cell().Background("E7EDF2").Padding(5).Text("Palabra").Bold();
                    cabecera.Cell().Background("E7EDF2").Padding(5).AlignRight().Text("Apariciones").Bold();
                });
                foreach (var palabra in palabras)
                {
                    tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").Padding(5).Text(palabra.Token);
                    tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").Padding(5).AlignRight().Text(palabra.Frecuencia.ToString()).Bold().FontColor("9A7418");
                }
            });
        });
    }

    private static void AgregarClasificacion(IContainer contenedor, AnalisisResultadoDTO resultado)
    {
        contenedor.Column(column =>
        {
            Encabezado(column, "Clasificación lingüística", "Resultados identificados por las reglas del idioma seleccionado");
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
            Encabezado(column, "Datos detectados", "Patrones identificados en el contenido del archivo");
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

    private static void Encabezado(ColumnDescriptor columna, string titulo, string descripcion)
    {
        columna.Item().Text(titulo).Bold().FontSize(12).FontColor("051B2E");
        columna.Item().PaddingTop(2).Text(descripcion).FontSize(8).FontColor("6B7280");
    }

    private static void Dato(TableDescriptor tabla, string etiqueta, string valor) =>
        tabla.Cell().Border(1).BorderColor("D9E0E6").Padding(9).Column(columna =>
        {
            columna.Item().Text(valor).Bold().FontSize(13).FontColor("17324D");
            columna.Item().PaddingTop(2).Text(etiqueta).FontSize(8).FontColor("6B7280");
        });

    private static void FilaClasificacion(TableDescriptor tabla, string etiqueta, IEnumerable<string> valores)
    {
        var encontrados = valores.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var texto = encontrados.Any() ? string.Join(", ", encontrados.Take(24)) : "Sin coincidencias";
        if (encontrados.Count > 24)
            texto += $" (+{encontrados.Count - 24} adicionales)";

        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(6).Text(etiqueta).Bold().FontColor("17324D");
        tabla.Cell().BorderBottom(1).BorderColor("E2E8F0").PaddingVertical(6).Text(texto);
    }

    private static string Titulo(string valor) =>
        string.IsNullOrWhiteSpace(valor) ? "No indicado" : char.ToUpper(valor[0]) + valor[1..];

    private static void PieDocumento(IContainer contenedor)
    {
        contenedor.Row(fila =>
        {
            fila.RelativeItem().Text("Analizador léxico · Universidad Mariano Gálvez").FontSize(8).FontColor("6B7280");
            fila.RelativeItem().AlignRight().Text(texto =>
            {
                texto.Span("Página ").FontSize(8).FontColor("6B7280");
                texto.CurrentPageNumber().FontSize(8).FontColor("6B7280");
                texto.Span(" de ").FontSize(8).FontColor("6B7280");
                texto.TotalPages().FontSize(8).FontColor("6B7280");
            });
        });
    }
}
