using LenguajesFormalesAPI.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LenguajesFormalesAPI.Services;

public interface ICredentialService
{
    byte[] GenerarPdf(Usuario usuario, string codigoQr);
}

public sealed class CredentialService : ICredentialService
{
    static CredentialService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] GenerarPdf(Usuario usuario, string codigoQr)
    {
        var qr = CrearCodigoQr(codigoQr);
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A6.Landscape());
            page.Margin(20);
            page.DefaultTextStyle(style => style.FontFamily(Fonts.Arial).FontSize(10));
            page.Content().Column(column =>
            {
                column.Spacing(8);
                column.Item().AlignCenter().Text("CREDENCIAL DE ANALISTA").FontSize(17).Bold().FontColor("051B2E");
                column.Item().AlignCenter().Text("Lenguajes Formales y Autómatas · UMG 2026").FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(datos =>
                    {
                        AgregarDato(datos, "Usuario", usuario.Nickname);
                        AgregarDato(datos, "Correo", usuario.Correo);
                        AgregarDato(datos, "Rol", usuario.Rol);
                        AgregarDato(datos, "Registro", usuario.FechaRegistro.ToString("dd/MM/yyyy"));
                    });
                    row.ConstantItem(105).AlignCenter().Image(qr);
                });
                column.Item().AlignCenter().Text("Escanee este código para iniciar sesión").FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().Height(4).Background("B48B21");
            });
        })).GeneratePdf();
    }

    private static void AgregarDato(ColumnDescriptor columna, string etiqueta, string valor)
    {
        columna.Item().Row(fila =>
        {
            fila.ConstantItem(58).Text(etiqueta).Bold().FontColor("051B2E");
            fila.RelativeItem().Text(valor);
        });
    }

    private static byte[] CrearCodigoQr(string codigoQr)
    {
        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(codigoQr, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(datos).GetGraphic(8);
    }
}
