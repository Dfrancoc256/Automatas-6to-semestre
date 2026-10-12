using System.Text.Json;
using System.Net;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.DTOs;
using LenguajesFormalesAPI.Interfaces;
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
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsApp;
    private readonly ILogger<AuthService> _logger;
    private readonly bool _sinBaseDeDatos;

    public AuthService(AppDbContext db, IJwtService jwt, IFacialService facial,
        IEmailService emailService, IWhatsAppService whatsApp,
        ILogger<AuthService> logger, IConfiguration configuration)
    {
        _db     = db;
        _jwt    = jwt;
        _facial = facial;
        _emailService = emailService;
        _whatsApp = whatsApp;
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
            Nombre = "Administrador",
            Apellido = "UMG",
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
        if (_sinBaseDeDatos)
            return RegistrarLocal(dto);

        var correo = dto.Correo.Trim().ToLowerInvariant();
        var nombre = NormalizarNombre(dto.Nombre);
        var apellido = NormalizarNombre(dto.Apellido);

        // Verificar unicidad
        if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
            throw new InvalidOperationException("El correo ya está registrado.");

        var nickname = await GenerarNicknameDisponibleAsync(nombre, apellido);

        // Persistir foto en uploads/
        string? rutaFotoOriginal   = null;
        string? rutaFotoModificada = null;

        if (!string.IsNullOrEmpty(dto.FotoBase64))
        {
            var carpeta = Path.Combine("uploads", "fotos");
            Directory.CreateDirectory(carpeta);
            var imagenOriginal = DecodificarFotoBase64(dto.FotoBase64);
            var nombreOriginal = $"{Guid.NewGuid()}{imagenOriginal.Extension}";
            // Rutas relativas servidas bajo /uploads (ver Program.cs) — siempre con "/", nunca la del SO
            rutaFotoOriginal = $"fotos/{nombreOriginal}";
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreOriginal), imagenOriginal.Bytes);

            var fotoModificada = string.IsNullOrWhiteSpace(dto.FotoModificadaBase64)
                ? dto.FotoBase64
                : dto.FotoModificadaBase64;
            var imagenModificada = DecodificarFotoBase64(fotoModificada);
            var nombreModificado = $"mod_{Guid.NewGuid()}{imagenModificada.Extension}";
            rutaFotoModificada = $"fotos/{nombreModificado}";
            await File.WriteAllBytesAsync(Path.Combine(carpeta, nombreModificado), imagenModificada.Bytes);
        }

        var usuario = new Usuario
        {
            Correo              = correo,
            Nombre              = nombre,
            Apellido            = apellido,
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

        _logger.LogInformation("Nuevo usuario registrado: {UsuarioId} ({Correo})", usuario.Id, usuario.Correo);
        await NotificarRegistroAsync(usuario);

        var respuesta = CrearRespuestaSesion(usuario);
        respuesta.CodigoQr = _jwt.GenerarTokenQr(usuario);
        return respuesta;
    }

    /// <summary>
    /// Envía las notificaciones elegidas sin revertir un registro que ya fue persistido.
    /// Nunca se incluye la contraseña ni información biométrica en los mensajes.
    /// </summary>
    private async Task NotificarRegistroAsync(Usuario usuario)
    {
        var nombre = WebUtility.HtmlEncode(usuario.NombreCompleto);

        if (usuario.MetodoNotificacion is "email" or "ambos")
        {
            try
            {
                await _emailService.SendEmailAsync(
                    usuario.Correo,
                    "Registro exitoso - Lenguajes Formales",
                    $"<p>¡Bienvenido, <strong>{nombre}</strong>!</p>" +
                    "<p>Tu cuenta fue creada correctamente.</p>" +
                    "<p>Por seguridad, nunca compartas tu contraseña.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de registro a {Correo}", usuario.Correo);
            }
        }

        if (usuario.MetodoNotificacion is "whatsapp" or "ambos")
        {
            var enviado = await _whatsApp.SendTextAsync(
                usuario.Telefono,
                $"¡Bienvenido a Lenguajes Formales, {usuario.NombreCompleto}! Tu cuenta fue creada correctamente. No compartas tu contraseña.");

            if (!enviado)
                _logger.LogWarning("No se pudo enviar WhatsApp de registro al usuario {UsuarioId}", usuario.Id);
        }
    }

    // Registro demostrativo para desarrollo local. No persiste correo, foto,
    // descriptor facial ni contraseña mientras PostgreSQL no esté disponible.
    private AuthResponseDTO RegistrarLocal(RegisterDTO dto)
    {
        var usuario = new Usuario
        {
            Id = 1,
            Correo = dto.Correo.Trim().ToLowerInvariant(),
            Nombre = NormalizarNombre(dto.Nombre),
            Apellido = NormalizarNombre(dto.Apellido),
            Telefono = dto.Telefono.Trim(),
            FechaNacimiento = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc),
            Nickname = GenerarBaseNickname(NormalizarNombre(dto.Nombre), NormalizarNombre(dto.Apellido)),
            Rol = "ANALISTA",
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _logger.LogWarning("Registro local sin base de datos para {Usuario}; los datos no se conservarán", usuario.Nickname);
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
            Nombre             = u.Nombre,
            Apellido           = u.Apellido,
            NombreCompleto     = u.NombreCompleto,
            Telefono           = u.Telefono,
            Nickname           = u.Nickname,
            MetodoNotificacion = u.MetodoNotificacion,
            FotoModificada     = u.FotoModificada,
            Rol                = u.Rol,
            FechaRegistro      = u.FechaRegistro,
            TieneRostroEnrolado = !string.IsNullOrWhiteSpace(u.EncodingFacial)
        };
    }

    /// <summary>Normaliza nombres sin alterar letras acentuadas o apellidos compuestos.</summary>
    private static string NormalizarNombre(string valor) =>
        string.Join(" ", valor.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private async Task<string> GenerarNicknameDisponibleAsync(string nombre, string apellido)
    {
        var baseNickname = GenerarBaseNickname(nombre, apellido);
        var candidato = baseNickname;
        var numero = 2;

        // La base protege nickname sin distinción de mayúsculas/minúsculas.
        while (await _db.Usuarios.AnyAsync(u => u.Nickname.ToLower() == candidato))
        {
            var sufijo = numero++.ToString(CultureInfo.InvariantCulture);
            candidato = $"{baseNickname[..Math.Min(baseNickname.Length, 50 - sufijo.Length)]}{sufijo}";
        }

        return candidato;
    }

    private static string GenerarBaseNickname(string nombre, string apellido)
    {
        var origen = $"{nombre}.{apellido}".Normalize(NormalizationForm.FormD);
        var limpio = new StringBuilder(origen.Length);

        foreach (var caracter in origen)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(caracter)) limpio.Append(char.ToLowerInvariant(caracter));
            else if (caracter is '.' or '-' or '_') limpio.Append(caracter);
        }

        var resultado = limpio.ToString().Trim('.', '-', '_');
        return resultado.Length >= 3 ? resultado[..Math.Min(resultado.Length, 50)] : "usuario";
    }

    /// <summary>Valida la imagen recibida desde cámara o galería antes de escribirla en disco.</summary>
    private static (byte[] Bytes, string Extension) DecodificarFotoBase64(string fotoBase64)
    {
        const int tamanoMaximoBytes = 5 * 1024 * 1024;
        var separador = fotoBase64.IndexOf(',');
        var encabezado = separador >= 0 ? fotoBase64[..separador] : string.Empty;
        var contenido = separador >= 0 ? fotoBase64[(separador + 1)..] : fotoBase64;
        var extension = encabezado.ToLowerInvariant() switch
        {
            "data:image/jpeg;base64" or "data:image/jpg;base64" => ".jpg",
            "data:image/png;base64" => ".png",
            "data:image/webp;base64" => ".webp",
            _ when string.IsNullOrEmpty(encabezado) => ".jpg",
            _ => throw new InvalidOperationException("El formato de la fotografía no está permitido.")
        };

        try
        {
            var bytes = Convert.FromBase64String(contenido);
            if (bytes.Length == 0 || bytes.Length > tamanoMaximoBytes)
                throw new InvalidOperationException("La fotografía debe tener un tamaño entre 1 byte y 5 MB.");
            return (bytes, extension);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("La fotografía no tiene un formato válido.");
        }
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
