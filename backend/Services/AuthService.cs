using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Models;

namespace LenguajesFormalesAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDTO?> LoginAsync(LoginDTO dto, string ip, string userAgent);
    Task<AuthResponseDTO?> LoginQrAsync(LoginQrDTO dto, string ip, string userAgent);
    Task<AuthResponseDTO> RegistrarAsync(RegisterDTO dto);
    Task<UsuarioPerfilDTO?> ObtenerPerfilAsync(int usuarioId);
    Task<bool> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService  _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, IJwtService jwt, ILogger<AuthService> logger)
    {
        _db     = db;
        _jwt    = jwt;
        _logger = logger;
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto, string ip, string userAgent)
    {
        var identificador = dto.Identificador.Trim();
        var correoNormalizado = identificador.ToLowerInvariant();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u =>
                (u.Correo == correoNormalizado || u.Nickname == identificador)
                && u.Activo);

        var exitoso = usuario != null && BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

        // Bitácora siempre
        if (usuario != null)
        {
            _db.BitacoraLogins.Add(new BitacoraLogin
            {
                UsuarioId = usuario.Id,
                IpOrigen  = Limitar(ip, 45),
                UserAgent = Limitar(userAgent, 300),
                Resultado = exitoso ? "exitoso" : "fallido",
                Metodo    = "password"
            });
            await _db.SaveChangesAsync();
        }

        if (!exitoso || usuario == null) return null;

        return CrearRespuestaSesion(usuario);
    }

    public async Task<AuthResponseDTO?> LoginQrAsync(LoginQrDTO dto, string ip, string userAgent)
    {
        var principal = _jwt.ValidarTokenQr(dto.CodigoQr.Trim());
        var idTexto = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idTexto, out var usuarioId))
            return null;

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo);
        if (usuario == null)
            return null;

        _db.BitacoraLogins.Add(new BitacoraLogin
        {
            UsuarioId = usuario.Id,
            IpOrigen = Limitar(ip, 45),
            UserAgent = Limitar(userAgent, 300),
            Resultado = "exitoso",
            Metodo = "qr"
        });
        await _db.SaveChangesAsync();

        return CrearRespuestaSesion(usuario);
    }

    public async Task<AuthResponseDTO> RegistrarAsync(RegisterDTO dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var nickname = dto.Nickname.Trim();

        // Verificar unicidad
        if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
            throw new InvalidOperationException("El correo ya está registrado.");

        if (await _db.Usuarios.AnyAsync(u => u.Nickname == nickname))
            throw new InvalidOperationException("El nickname ya está en uso.");

        // Persistir foto en uploads/
        string? rutaFotoOriginal   = null;
        string? rutaFotoModificada = null;

        if (!string.IsNullOrEmpty(dto.FotoBase64))
        {
            var carpeta = Path.Combine("uploads", "fotos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{Guid.NewGuid()}.jpg";
            rutaFotoOriginal   = Path.Combine(carpeta, nombreArchivo);
            var bytes = Convert.FromBase64String(
                dto.FotoBase64.Contains(',') ? dto.FotoBase64.Split(',')[1] : dto.FotoBase64);
            await File.WriteAllBytesAsync(rutaFotoOriginal, bytes);
            rutaFotoModificada = rutaFotoOriginal; // misma foto por defecto
        }

        var usuario = new Usuario
        {
            Correo              = correo,
            Telefono            = dto.Telefono.Trim(),
            FechaNacimiento     = dto.FechaNacimiento,
            Nickname            = nickname,
            PasswordHash        = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            MetodoNotificacion  = dto.MetodoNotificacion,
            FotoOriginal        = rutaFotoOriginal,
            FotoModificada      = rutaFotoModificada,
            Rol                 = "ANALISTA",
            Activo              = true,
            FechaRegistro       = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nuevo usuario registrado: {Nickname} ({Correo})", usuario.Nickname, usuario.Correo);

        var respuesta = CrearRespuestaSesion(usuario);
        respuesta.CodigoQr = _jwt.GenerarTokenQr(usuario);
        return respuesta;
    }

    public async Task<UsuarioPerfilDTO?> ObtenerPerfilAsync(int usuarioId)
    {
        var u = await _db.Usuarios.FindAsync(usuarioId);
        if (u == null) return null;

        return new UsuarioPerfilDTO
        {
            Id                 = u.Id,
            Correo             = u.Correo,
            Telefono           = u.Telefono,
            Nickname           = u.Nickname,
            MetodoNotificacion = u.MetodoNotificacion,
            FotoModificada     = u.FotoModificada,
            Rol                = u.Rol,
            FechaRegistro      = u.FechaRegistro
        };
    }

    public async Task<bool> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return false;

        if (!string.IsNullOrEmpty(dto.Telefono))
            usuario.Telefono = dto.Telefono;

        if (!string.IsNullOrEmpty(dto.MetodoNotificacion))
            usuario.MetodoNotificacion = dto.MetodoNotificacion;

        if (!string.IsNullOrEmpty(dto.PasswordActual) && !string.IsNullOrEmpty(dto.NuevoPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
                throw new InvalidOperationException("La contraseña actual es incorrecta.");
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevoPassword);
        }

        if (!string.IsNullOrEmpty(dto.FotoBase64))
        {
            var carpeta = Path.Combine("uploads", "fotos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"{Guid.NewGuid()}.jpg";
            var ruta  = Path.Combine(carpeta, nombreArchivo);
            var bytes = Convert.FromBase64String(
                dto.FotoBase64.Contains(',') ? dto.FotoBase64.Split(',')[1] : dto.FotoBase64);
            await File.WriteAllBytesAsync(ruta, bytes);
            usuario.FotoOriginal = ruta;
        }

        if (!string.IsNullOrEmpty(dto.FotoModificadaBase64))
        {
            var carpeta = Path.Combine("uploads", "fotos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"mod_{Guid.NewGuid()}.jpg";
            var ruta  = Path.Combine(carpeta, nombreArchivo);
            var bytes = Convert.FromBase64String(
                dto.FotoModificadaBase64.Contains(',')
                    ? dto.FotoModificadaBase64.Split(',')[1]
                    : dto.FotoModificadaBase64);
            await File.WriteAllBytesAsync(ruta, bytes);
            usuario.FotoModificada = ruta;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    private static string Limitar(string valor, int longitudMaxima) =>
        valor.Length <= longitudMaxima ? valor : valor[..longitudMaxima];

    private AuthResponseDTO CrearRespuestaSesion(Usuario usuario) => new()
    {
        Token = _jwt.GenerarToken(usuario),
        Rol = usuario.Rol,
        Nickname = usuario.Nickname,
        FotoModificada = usuario.FotoModificada,
        Expiracion = DateTime.UtcNow.AddHours(8)
    };
}
