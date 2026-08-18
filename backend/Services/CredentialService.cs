using iTextSharp.text;
using iTextSharp.text.pdf;
using LenguajesFormalesAPI.Models;
using QRCoder;

namespace LenguajesFormalesAPI.Services;

public interface ICredentialService
{
    byte[] GenerarPdf(Usuario usuario, string codigoQr);
}

public class CredentialService : ICredentialService
{
    public byte[] GenerarPdf(Usuario usuario, string codigoQr)
    {
        using var output = new MemoryStream();
        using var document = new Document(PageSize.A6.Rotate(), 28, 28, 24, 24);
        PdfWriter.GetInstance(document, output).CloseStream = false;
        document.Open();

        var azul = new BaseColor(28, 114, 165);
        var acento = new BaseColor(176, 134, 58);
        var titulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, azul);
        var subtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);
        var etiqueta = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, azul);
        var valor = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

        document.Add(new Paragraph("CREDENCIAL DE ANALISTA", titulo)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 2
        });
        document.Add(new Paragraph("Lenguajes Formales y Autómatas · UMG 2026", subtitulo)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 14
        });

        var tabla = new PdfPTable([1.2f, 2.8f]) { WidthPercentage = 100 };
        tabla.DefaultCell.Border = Rectangle.NO_BORDER;
        tabla.DefaultCell.PaddingBottom = 6;

        AgregarDato(tabla, "Usuario", usuario.Nickname, etiqueta, valor);
        AgregarDato(tabla, "Correo", usuario.Correo, etiqueta, valor);
        AgregarDato(tabla, "Rol", usuario.Rol, etiqueta, valor);
        AgregarDato(tabla, "Registro", usuario.FechaRegistro.ToString("dd/MM/yyyy"), etiqueta, valor);
        document.Add(tabla);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(codigoQr, QRCodeGenerator.ECCLevel.Q);
        var qrBytes = new PngByteQRCode(data).GetGraphic(8);
        var qr = Image.GetInstance(qrBytes);
        qr.ScaleAbsolute(105, 105);
        qr.Alignment = Element.ALIGN_CENTER;
        qr.SpacingBefore = 4;
        document.Add(qr);

        document.Add(new Paragraph("Escanee este código para iniciar sesión", subtitulo)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingBefore = 2
        });

        var linea = new PdfPTable(1) { WidthPercentage = 100, SpacingBefore = 8 };
        linea.DefaultCell.BackgroundColor = acento;
        linea.DefaultCell.FixedHeight = 4;
        linea.DefaultCell.Border = Rectangle.NO_BORDER;
        linea.AddCell(string.Empty);
        document.Add(linea);

        document.Close();
        return output.ToArray();
    }

    private static void AgregarDato(PdfPTable tabla, string nombre, string contenido,
        Font etiqueta, Font valor)
    {
        tabla.AddCell(new PdfPCell(new Phrase(nombre, etiqueta))
        {
            Border = Rectangle.NO_BORDER,
            PaddingBottom = 6
        });
        tabla.AddCell(new PdfPCell(new Phrase(contenido, valor))
        {
            Border = Rectangle.NO_BORDER,
            PaddingBottom = 6
        });
    }
}
