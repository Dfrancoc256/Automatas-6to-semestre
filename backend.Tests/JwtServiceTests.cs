using System.Security.Claims;
using LenguajesFormalesAPI.Models;
using LenguajesFormalesAPI.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LenguajesFormalesAPI.Tests;

public class JwtServiceTests
{
    private readonly Usuario _usuario = new()
    {
        Id = 42,
        Correo = "analista@example.com",
        Nickname = "analista",
        Rol = "ANALISTA"
    };

    [Fact]
    public void TokenSesion_SoloSeValidaComoSesion()
    {
        var service = CrearServicio();
        var token = service.GenerarToken(_usuario);

        var principal = service.ValidarToken(token);

        Assert.Equal("42", principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Null(service.ValidarTokenQr(token));
    }

    [Fact]
    public void TokenQr_SoloSeValidaComoCredencialQr()
    {
        var service = CrearServicio();
        var token = service.GenerarTokenQr(_usuario);

        var principal = service.ValidarTokenQr(token);

        Assert.Equal("42", principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Null(service.ValidarToken(token));
    }

    [Fact]
    public void TokenAlterado_EsRechazado()
    {
        var service = CrearServicio();
        var token = service.GenerarTokenQr(_usuario);
        var segmentos = token.Split('.');
        segmentos[2] = (segmentos[2][0] == 'a' ? 'b' : 'a') + segmentos[2][1..];
        var alterado = string.Join('.', segmentos);

        Assert.Null(service.ValidarTokenQr(alterado));
    }

    private static JwtService CrearServicio()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "clave-de-pruebas-con-mas-de-32-caracteres-seguros",
            ["Jwt:Issuer"] = "LenguajesFormalesAPI.Tests",
            ["Jwt:Audience"] = "LenguajesFormalesFrontend.Tests",
            ["Jwt:QrExpirationDays"] = "30"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new JwtService(config);
    }
}
