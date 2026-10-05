using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Models;
using LenguajesFormalesAPI.Interfaces;
using System.Security.Cryptography;

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
private readonly IJwtService _jwt;
private readonly IEmailService _emailService;
private readonly ILogger<AuthService> _logger;

public AuthService(
    AppDbContext db,
    IJwtService jwt,
    IEmailService emailService,
    ILogger<AuthService> logger)
{
    _db = db;
    _jwt = jwt;
    _emailService = emailService;
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
       string? rutaFotoOriginal = null;
        string? rutaFotoModificada = null;

        byte[]? bytesOriginales = null;
        byte[]? bytesModificados = null;

        if (!string.IsNullOrEmpty(dto.FotoBase64))
            {
                var carpeta = Path.Combine("uploads", "fotos");
                Directory.CreateDirectory(carpeta);

                // ── Foto original ─────────────────────────────────────────────
                var nombreOriginal = $"{Guid.NewGuid()}.jpg";

                rutaFotoOriginal = Path.Combine(
                    carpeta,
                    nombreOriginal
                );

                bytesOriginales = Convert.FromBase64String(
                    dto.FotoBase64.Contains(',')
                        ? dto.FotoBase64.Split(',')[1]
                        : dto.FotoBase64
                );

                await File.WriteAllBytesAsync(
                    rutaFotoOriginal,
                    bytesOriginales
                );

                // ── Foto modificada ───────────────────────────────────────────
                var fotoModificada =
                    string.IsNullOrWhiteSpace(dto.FotoModificadaBase64)
                        ? dto.FotoBase64
                        : dto.FotoModificadaBase64;

                var nombreModificado =
                    $"mod_{Guid.NewGuid()}.jpg";

                rutaFotoModificada = Path.Combine(
                    carpeta,
                    nombreModificado
                );

                bytesModificados = Convert.FromBase64String(
                    fotoModificada.Contains(',')
                        ? fotoModificada.Split(',')[1]
                        : fotoModificada
                );

                await File.WriteAllBytesAsync(
                    rutaFotoModificada,
                    bytesModificados
                );
            }

        var usuario = new Usuario
        {
            Correo              = correo,
            Telefono            = dto.Telefono.Trim(),
            FechaNacimiento = DateTime.SpecifyKind(
            dto.FechaNacimiento,
            DateTimeKind.Utc
            ),
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

        // ── Registrar fotografías en base de datos ───────────────────────────────
if (bytesOriginales != null &&
    bytesModificados != null &&
    rutaFotoOriginal != null &&
    rutaFotoModificada != null)
{
    var fotografiaOriginal = new FotografiaUsuario
    {
        UsuarioId = usuario.Id,
        Tipo = "original",
        UbicacionAlmacen = rutaFotoOriginal.Replace("\\", "/"),
        HashContenido = CalcularHash(bytesOriginales),
        Estado = "activa",
        FechaRegistro = DateTime.UtcNow,
        ReemplazadaEn = null
    };

    var fotografiaModificada = new FotografiaUsuario
    {
        UsuarioId = usuario.Id,
        Tipo = "modificada",
        UbicacionAlmacen = rutaFotoModificada.Replace("\\", "/"),
        HashContenido = CalcularHash(bytesModificados),
        Estado = "activa",
        FechaRegistro = DateTime.UtcNow,
        ReemplazadaEn = null
    };

    _db.FotografiasUsuario.Add(fotografiaOriginal);
    _db.FotografiasUsuario.Add(fotografiaModificada);

    await _db.SaveChangesAsync();
}

_logger.LogInformation(
    "Nuevo usuario registrado: {Nickname} ({Correo})",
    usuario.Nickname,
    usuario.Correo);

// ── Notificación por correo ───────────────────────────────────────────────
if (usuario.MetodoNotificacion == "email" ||
    usuario.MetodoNotificacion == "ambos")
{
    try
    {
        var asunto = "Registro exitoso - Lenguajes Formales";

        var cuerpo = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto;">
                
                <h2 style="color: #051B2E;">
                    ¡Bienvenido, {usuario.Nickname}!
                </h2>

                <p>
                    Tu cuenta ha sido creada correctamente en la plataforma
                    de Lenguajes Formales y Autómatas.
                </p>

                <p>
                    Tu nombre de usuario es:
                    <strong>{usuario.Nickname}</strong>
                </p>

                <p>
                    Tu correo registrado es:
                    <strong>{usuario.Correo}</strong>
                </p>

                <hr>

                <p style="font-size: 12px; color: #666;">
                    Este es un mensaje automático. No compartas tu contraseña
                    ni tus datos de acceso con otras personas.
                </p>

            </div>
            """;

        await _emailService.SendEmailAsync(
            usuario.Correo,
            asunto,
            cuerpo
        );

        _logger.LogInformation(
            "Correo de registro enviado a {Correo}",
            usuario.Correo);
    }
    catch (Exception ex)
    {
        // El usuario YA fue registrado.
        // Un fallo del correo no debe cancelar la creación de la cuenta.
        _logger.LogError(
            ex,
            "No se pudo enviar el correo de registro a {Correo}",
            usuario.Correo);
    }
}

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

    private static string CalcularHash(byte[] contenido)
{
    var hash = SHA256.HashData(contenido);
    return Convert.ToHexString(hash);
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
