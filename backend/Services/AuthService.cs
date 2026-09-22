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
    Task<AuthResponseDTO?> LoginFacialAsync(LoginFacialDTO dto, string ip, string userAgent);
    Task<AuthResponseDTO> RegistrarAsync(RegisterDTO dto);
    Task<UsuarioPerfilDTO?> ObtenerPerfilAsync(int usuarioId);
    Task<bool> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilDTO dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService  _jwt;
    private readonly IFacialService _facial;
    private readonly ILogger<AuthService> _logger;
    private readonly bool _sinBaseDeDatos;

    public AuthService(AppDbContext db, IJwtService jwt, IFacialService facial,
        ILogger<AuthService> logger, IConfiguration configuration)
    {
        _db     = db;
        _jwt    = jwt;
        _facial = facial;
        _logger = logger;
        _sinBaseDeDatos = configuration.GetValue<bool>("Demo:SinBaseDeDatos");
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto, string ip, string userAgent)
    {
        if (_sinBaseDeDatos)
            return LoginLocal(dto);

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

    // Solo para ejecución local sin PostgreSQL. No persiste usuarios ni bitácora.
    private AuthResponseDTO? LoginLocal(LoginDTO dto)
    {
        var identificador = dto.Identificador.Trim().ToLowerInvariant();
        const string hashPrueba = "$2a$11$.BlW6Nc4T70xY/kIFpGLeuFQIgE6SZpDq7w0fNKYcgV9fmxsrywZ6";
        var identificadorValido = identificador is "admin@umg.edu.gt" or "admin_umg";

        if (!identificadorValido || !BCrypt.Net.BCrypt.Verify(dto.Password, hashPrueba))
            return null;

        _logger.LogWarning("Inicio de sesión local sin base de datos para {Usuario}", identificador);
        return CrearRespuestaSesion(new Usuario
        {
            Id = 1,
            Correo = "admin@umg.edu.gt",
            Nickname = "admin_umg",
            Rol = "ADMIN",
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        });
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

    public async Task<AuthResponseDTO?> LoginFacialAsync(LoginFacialDTO dto, string ip, string userAgent)
    {
        var (usuario, confianza) = await _facial.BuscarCoincidenciaAsync(dto.Descriptor);

        if (usuario == null) return null;

        _db.BitacoraLogins.Add(new BitacoraLogin
        {
            UsuarioId = usuario.Id,
            IpOrigen  = Limitar(ip, 45),
            UserAgent = Limitar(userAgent, 300),
            Resultado = "exitoso",
            Metodo    = "facial"
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Login facial exitoso: {Nickname} (confianza {Confianza}%)", usuario.Nickname, confianza);
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
            var nombreOriginal = $"{Guid.NewGuid()}.jpg";
            // Rutas relativas servidas bajo /uploads (ver Program.cs) — siempre con "/", nunca la del SO
            rutaFotoOriginal = $"fotos/{nombreOriginal}";
            var bytesOriginales = Convert.FromBase64String(
                dto.FotoBase64.Contains(',') ? dto.FotoBase64.Split(',')[1] : dto.FotoBase64);
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreOriginal), bytesOriginales);

            var fotoModificada = string.IsNullOrWhiteSpace(dto.FotoModificadaBase64)
                ? dto.FotoBase64
                : dto.FotoModificadaBase64;
            var nombreModificado = $"mod_{Guid.NewGuid()}.jpg";
            rutaFotoModificada = $"fotos/{nombreModificado}";
            var bytesModificados = Convert.FromBase64String(
                fotoModificada.Contains(',') ? fotoModificada.Split(',')[1] : fotoModificada);
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreModificado), bytesModificados);
        }

        var usuario = new Usuario
        {
            Correo              = correo,
            Telefono            = dto.Telefono.Trim(),
            FechaNacimiento     = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc),
            Nickname            = nickname,
            PasswordHash        = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            MetodoNotificacion  = dto.MetodoNotificacion,
            FotoOriginal        = rutaFotoOriginal,
            FotoModificada      = rutaFotoModificada,
            EncodingFacial      = dto.Descriptor is { Count: > 0 } ? JsonSerializer.Serialize(dto.Descriptor) : null,
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
            FechaRegistro      = u.FechaRegistro,
            TieneRostroEnrolado = !string.IsNullOrWhiteSpace(u.EncodingFacial)
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
            var bytes = Convert.FromBase64String(
                dto.FotoBase64.Contains(',') ? dto.FotoBase64.Split(',')[1] : dto.FotoBase64);
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreArchivo), bytes);
            usuario.FotoOriginal = $"fotos/{nombreArchivo}";
        }

        if (!string.IsNullOrEmpty(dto.FotoModificadaBase64))
        {
            var carpeta = Path.Combine("uploads", "fotos");
            Directory.CreateDirectory(carpeta);
            var nombreArchivo = $"mod_{Guid.NewGuid()}.jpg";
            var bytes = Convert.FromBase64String(
                dto.FotoModificadaBase64.Contains(',')
                    ? dto.FotoModificadaBase64.Split(',')[1]
                    : dto.FotoModificadaBase64);
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreArchivo), bytes);
            usuario.FotoModificada = $"fotos/{nombreArchivo}";
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
