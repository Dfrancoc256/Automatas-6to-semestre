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
    private readonly string _carpetaUploads;
    private readonly string _rutaLogo;

    static CredentialService() => QuestPDF.Settings.License = LicenseType.Community;

    public CredentialService(IWebHostEnvironment environment)
    {
        _carpetaUploads = Path.Combine(environment.ContentRootPath, "uploads");
        _rutaLogo = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
            "images", "logo-umg-oficial.png");
    }

    public byte[] GenerarPdf(Usuario usuario, string codigoQr)
    {
        var qr = CrearCodigoQr(codigoQr);
        var foto = LeerImagenUsuario(usuario.FotoModificada) ?? LeerImagenUsuario(usuario.FotoOriginal);
        var logo = LeerArchivo(_rutaLogo);

        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A6.Landscape());
            page.Margin(0);
            page.DefaultTextStyle(style => style.FontFamily(Fonts.Arial).FontSize(9));
            page.PageColor("F6F8FA");

            page.Header().Height(75).Background("052E4C").PaddingHorizontal(20).PaddingVertical(9).Row(cabecera =>
            {
                if (logo != null)
                    cabecera.ConstantItem(57).Height(57).Image(logo).FitArea();
                else
                    cabecera.ConstantItem(57).Height(57).Border(1).BorderColor("C59A1C").AlignCenter().AlignMiddle().Text("UMG").Bold().FontColor("FFFFFF");

                cabecera.RelativeItem().PaddingLeft(12).Column(texto =>
                {
                    texto.Item().Text("UNIVERSIDAD MARIANO GÁLVEZ").Bold().FontSize(12).FontColor("FFFFFF");
                    texto.Item().Text("Guatemala · Lenguajes Formales y Autómatas").FontSize(7.5f).FontColor("D8E5EE");
                    texto.Item().PaddingTop(4).Text("CREDENCIAL DE ANALISTA").Bold().FontSize(13).FontColor("D0A126");
                });
            });

            page.Content().Column(column =>
            {
                column.Item().Padding(16).Row(row =>
                {
                    row.ConstantItem(104).Height(132).Border(2).BorderColor("C59A1C").Background("FFFFFF").Padding(4).AlignCenter().AlignMiddle().Element(contenedor =>
                    {
                        if (foto != null)
                            contenedor.Image(foto).FitArea();
                        else
                            contenedor.Column(vacio =>
                            {
                                vacio.Item().AlignCenter().Text("FOTO").Bold().FontColor("6A7B88");
                                vacio.Item().PaddingTop(4).AlignCenter().Text("No disponible").FontSize(7).FontColor("6A7B88");
                            });
                    });

                    row.RelativeItem().PaddingHorizontal(13).PaddingTop(4).Column(datos =>
                    {
                        datos.Spacing(5);
                        datos.Item().Text(usuario.NombreCompleto).Bold().FontSize(14).FontColor("052E4C");
                        AgregarDato(datos, "Usuario", usuario.Nickname);
                        AgregarDato(datos, "Correo", usuario.Correo);
                        AgregarDato(datos, "Rol", usuario.Rol);
                        AgregarDato(datos, "Registro", usuario.FechaRegistro.ToString("dd/MM/yyyy"));
                    });

                    row.ConstantItem(108).Column(codigo =>
                    {
                        codigo.Item().AlignCenter().Text("ESCANEA PARA INGRESAR").Bold().FontSize(6.5f).FontColor("052E4C");
                        codigo.Item().PaddingTop(4).Border(1).BorderColor("C59A1C").Background("FFFFFF").Padding(4).Image(qr).FitArea();
                        codigo.Item().PaddingTop(3).AlignCenter().Text("Código personal").FontSize(6.5f).FontColor("6A7B88");
                    });
                });
            });

            page.Footer().Height(18).Background("C59A1C").AlignCenter().AlignMiddle()
                .Text("Documento personal · No compartas tu código QR").FontSize(7).FontColor("FFFFFF");
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

    private byte[]? LeerImagenUsuario(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;

        var relativa = rutaRelativa.Replace('\\', '/').TrimStart('/');
        if (!relativa.StartsWith("fotos/", StringComparison.Ordinal)) return null;

        var rutaCompleta = Path.GetFullPath(Path.Combine(_carpetaUploads,
            relativa.Replace('/', Path.DirectorySeparatorChar)));
        var raiz = Path.GetFullPath(_carpetaUploads) + Path.DirectorySeparatorChar;
        return rutaCompleta.StartsWith(raiz, StringComparison.Ordinal) ? LeerArchivo(rutaCompleta) : null;
    }

    private static byte[]? LeerArchivo(string ruta)
    {
        try { return File.Exists(ruta) ? File.ReadAllBytes(ruta) : null; }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }
}
