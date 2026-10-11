using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.Models;

namespace LenguajesFormalesAPI.Services;

public interface IFacialService
{
    Task<bool> EnrolarAsync(int usuarioId, List<double> descriptor);
    Task<bool> TieneRostroAsync(int usuarioId);
    Task<(Usuario? usuario, double confianza)> BuscarCoincidenciaAsync(List<double> descriptor);
}

/// <summary>
/// Reconocimiento facial 1:N por comparación de descriptores (distancia euclidiana),
/// el mismo enfoque que el servicio externo de biometrico (persona.encoding_facial),
/// solo que aquí el descriptor de 128 valores se calcula en el navegador con face-api.js
/// en vez de un microservicio Python externo, y la comparación ocurre en este backend.
/// </summary>
public class FacialService : IFacialService
{
    // face-api.js recomienda 0.6 como umbral de coincidencia para su descriptor de 128-d.
    private const double UmbralCoincidencia = 0.55;
    private const int LargoDescriptorEsperado = 128;

    private readonly AppDbContext _db;
    private readonly ILogger<FacialService> _logger;

    public FacialService(AppDbContext db, ILogger<FacialService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> EnrolarAsync(int usuarioId, List<double> descriptor)
    {
        if (descriptor.Count != LargoDescriptorEsperado)
        {
            _logger.LogWarning("Descriptor facial con longitud inesperada: {Longitud}", descriptor.Count);
            return false;
        }

        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return false;

        usuario.EncodingFacial = JsonSerializer.Serialize(descriptor);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TieneRostroAsync(int usuarioId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        return !string.IsNullOrWhiteSpace(usuario?.EncodingFacial);
    }

    public async Task<(Usuario? usuario, double confianza)> BuscarCoincidenciaAsync(List<double> descriptor)
    {
        if (descriptor.Count != LargoDescriptorEsperado)
            return (null, 0);

        var candidatos = await _db.Usuarios
            .Where(u => u.Activo && u.EncodingFacial != null)
            .ToListAsync();

        Usuario? mejorUsuario = null;
        double mejorDistancia = double.MaxValue;

        foreach (var candidato in candidatos)
        {
            List<double>? candidatoDescriptor;
            try
            {
                candidatoDescriptor = JsonSerializer.Deserialize<List<double>>(candidato.EncodingFacial!);
            }
            catch (JsonException)
            {
                continue;
            }

            if (candidatoDescriptor == null || candidatoDescriptor.Count != LargoDescriptorEsperado)
                continue;

            var distancia = DistanciaEuclidiana(descriptor, candidatoDescriptor);
            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejorUsuario = candidato;
            }
        }

        if (mejorUsuario == null || mejorDistancia >= UmbralCoincidencia)
            return (null, 0);

        var confianza = Math.Round((1 - mejorDistancia / UmbralCoincidencia) * 100, 1);
        return (mejorUsuario, Math.Clamp(confianza, 0, 100));
    }

    private static double DistanciaEuclidiana(List<double> a, List<double> b)
    {
        double suma = 0;
        for (int i = 0; i < a.Count; i++)
        {
            var diferencia = a[i] - b[i];
            suma += diferencia * diferencia;
        }
        return Math.Sqrt(suma);
    }
}
