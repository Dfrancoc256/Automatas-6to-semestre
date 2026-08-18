using System.Text;
using LenguajesFormalesAPI.Models;
using LenguajesFormalesAPI.Services;
using Xunit;

namespace LenguajesFormalesAPI.Tests;

public class CredentialServiceTests
{
    [Fact]
    public void GenerarPdf_ProduceDocumentoPdfConContenido()
    {
        var usuario = new Usuario
        {
            Id = 7,
            Correo = "qa@example.com",
            Telefono = "50255555555",
            Nickname = "qa-user",
            Rol = "ANALISTA",
            FechaRegistro = new DateTime(2026, 8, 4)
        };

        var pdf = new CredentialService().GenerarPdf(usuario, "codigo-qr-de-prueba");

        Assert.True(pdf.Length > 1_000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
