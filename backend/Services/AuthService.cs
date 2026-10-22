using System.Text.Json;
using System.Net;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
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
    Task<AuthResponseDTO?> CambiarPasswordTemporalAsync(int usuarioId, string nuevoPassword);
    Task<AuthResponseDTO?> LoginGoogleAsync(string googleSubject, string correo, string ip, string userAgent);
    Task<bool> SolicitarRestablecimientoPasswordAsync(string correo);
    Task<bool> RestablecerPasswordAsync(string token, string nuevoPassword);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService  _jwt;
    private readonly IFacialService _facial;
    private readonly IEmailService _emailService;
    private readonly IWhatsAppService _whatsApp;
    private readonly ICredentialService _credential;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly bool _sinBaseDeDatos;

    public AuthService(AppDbContext db, IJwtService jwt, IFacialService facial,
        IEmailService emailService, IWhatsAppService whatsApp,
        ICredentialService credential,
        ILogger<AuthService> logger, IConfiguration configuration)
    {
        _db     = db;
        _jwt    = jwt;
        _facial = facial;
        _emailService = emailService;
        _whatsApp = whatsApp;
        _credential = credential;
        _logger = logger;
        _configuration = configuration;
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

    // Registro temporal para desarrollo local.
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

    public async Task<AuthResponseDTO?> LoginGoogleAsync(string googleSubject, string correo, string ip, string userAgent)
    {
        var correoNormalizado = correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Activo
            && (u.GoogleSubject == googleSubject || u.Correo == correoNormalizado));
        if (usuario == null) return null;

        if (string.IsNullOrWhiteSpace(usuario.GoogleSubject))
        {
            usuario.GoogleSubject = googleSubject;
            await _db.SaveChangesAsync();
        }

        _db.BitacoraLogins.Add(new BitacoraLogin
        {
            UsuarioId = usuario.Id,
            IpOrigen = Limitar(ip, 45),
            UserAgent = Limitar(userAgent, 300),
            Resultado = "exitoso",
            Metodo = "google"
        });
        await _db.SaveChangesAsync();
        return CrearRespuestaSesion(usuario);
    }

    public async Task<bool> SolicitarRestablecimientoPasswordAsync(string correo)
    {
        if (_sinBaseDeDatos)
            return true;

        var correoNormalizado = correo.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Activo && u.Correo == correoNormalizado);

        // No se revela si una dirección está registrada.
        if (usuario == null)
            return true;

        var ahora = DateTime.UtcNow;
        var tokensPrevios = await _db.PasswordResetTokens
            .Where(t => t.UsuarioId == usuario.Id && t.UsadoEn == null)
            .ToListAsync();
        foreach (var previo in tokensPrevios)
            previo.UsadoEn = ahora;

        var token = CrearTokenSeguro();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = HashToken(token),
            ExpiraEn = ahora.AddMinutes(30),
            CreadoEn = ahora
        });
        await _db.SaveChangesAsync();

        var urlBase = (_configuration["App:PublicUrl"] ?? "https://analizador-umg.duckdns.org").TrimEnd('/');
        var enlace = $"{urlBase}/restablecer-password?token={Uri.EscapeDataString(token)}";
        var nombre = WebUtility.HtmlEncode(usuario.NombreCompleto);
        var enlaceHtml = WebUtility.HtmlEncode(enlace);

        try
        {
            await _emailService.SendEmailAsync(
                usuario.Correo,
                "Restablece tu contraseña - Lenguajes Formales",
                $"<p>Hola, <strong>{nombre}</strong>.</p>" +
                "<p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>" +
                $"<p><a href=\"{enlaceHtml}\">Restablecer contraseña</a></p>" +
                "<p>El enlace vence en 30 minutos y solo puede utilizarse una vez. Si no solicitaste este cambio, ignora este correo.</p>");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo de restablecimiento a {Correo}", usuario.Correo);
            return false;
        }
    }

    public async Task<bool> RestablecerPasswordAsync(string token, string nuevoPassword)
    {
        if (_sinBaseDeDatos || string.IsNullOrWhiteSpace(token))
            return false;

        var ahora = DateTime.UtcNow;
        var tokenHash = HashToken(token);
        var registro = await _db.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsadoEn == null && t.ExpiraEn > ahora);
        if (registro?.Usuario is not { Activo: true } usuario)
            return false;

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoPassword);
        usuario.DebeCambiarPassword = false;
        registro.UsadoEn = ahora;

        var otrosTokens = await _db.PasswordResetTokens
            .Where(t => t.UsuarioId == usuario.Id && t.Id != registro.Id && t.UsadoEn == null)
            .ToListAsync();
        foreach (var otro in otrosTokens)
            otro.UsadoEn = ahora;

        await _db.SaveChangesAsync();
        return true;
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

        var googleSubject = string.Empty;
        var correo = dto.Correo.Trim().ToLowerInvariant();
        var nombre = NormalizarNombre(dto.Nombre);
        var apellido = NormalizarNombre(dto.Apellido);

        if (!string.IsNullOrWhiteSpace(dto.GoogleRegistroToken))
        {
            var principal = _jwt.ValidarTokenRegistroGoogle(dto.GoogleRegistroToken);
            googleSubject = principal?.FindFirst("google_subject")?.Value ?? string.Empty;
            var correoGoogle = principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var nombreGoogle = principal?.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            if (string.IsNullOrWhiteSpace(googleSubject) || string.IsNullOrWhiteSpace(correoGoogle) || string.IsNullOrWhiteSpace(nombreGoogle))
                throw new InvalidOperationException("La verificación de Google venció. Selecciona tu cuenta nuevamente.");

            correo = correoGoogle.Trim().ToLowerInvariant();
            nombre = NormalizarNombre(nombreGoogle);
        }

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

        var passwordTemporal = string.IsNullOrWhiteSpace(googleSubject) ? GenerarPasswordTemporal() : null;
        var usuario = new Usuario
        {
            Correo              = correo,
            Nombre              = nombre,
            Apellido            = apellido,
            Telefono            = dto.Telefono.Trim(),
            FechaNacimiento     = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc),
            Nickname            = nickname,
            PasswordHash        = BCrypt.Net.BCrypt.HashPassword(passwordTemporal ?? GenerarPasswordTemporal()),
            DebeCambiarPassword = string.IsNullOrWhiteSpace(googleSubject),
            GoogleSubject       = string.IsNullOrWhiteSpace(googleSubject) ? null : googleSubject,
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
        var codigoQr = _jwt.GenerarCodigoQrCredencial(usuario);
        await NotificarRegistroAsync(usuario, codigoQr, passwordTemporal);

        var respuesta = CrearRespuestaSesion(usuario, omitirCambioPassword: true);
        respuesta.CodigoQr = codigoQr;
        return respuesta;
    }

    /// <summary>Envía las notificaciones del registro.</summary>
    private async Task NotificarRegistroAsync(Usuario usuario, string codigoQr, string? passwordTemporal)
    {
        var nombre = WebUtility.HtmlEncode(usuario.NombreCompleto);
        var correo = WebUtility.HtmlEncode(usuario.Correo);
        var nickname = WebUtility.HtmlEncode(usuario.Nickname);
        var urlConfigurada = _configuration["App:PublicUrl"];
        var urlWeb = (string.IsNullOrWhiteSpace(urlConfigurada)
            ? "https://analizador-umg.duckdns.org"
            : urlConfigurada).TrimEnd('/');
        var urlHtml = WebUtility.HtmlEncode(urlWeb);
        var nombreArchivo = $"credencial-{usuario.Nickname}.pdf";
        byte[]? credencial = null;

        try
        {
            credencial = _credential.GenerarPdf(usuario, codigoQr);
        }
        catch (Exception ex)
        {
            // El registro no se revierte si falla la credencial.
            _logger.LogError(ex, "No se pudo generar la credencial inicial del usuario {UsuarioId}", usuario.Id);
        }

        var mensajeAcceso = passwordTemporal is null
            ? "Tu acceso se realiza con la cuenta de Google que seleccionaste durante el registro."
            : "Revisa tu correo para obtener la contraseña temporal de acceso.";
        var mensajeWhatsApp =
            $"¡Bienvenido/a, {usuario.NombreCompleto}!\n\n" +
            $"Ingresa en: {urlWeb}\n" +
            $"Correo: {usuario.Correo}\n" +
            $"Nickname: {usuario.Nickname}\n\n" +
            mensajeAcceso;

        try
        {
            await _emailService.SendEmailAsync(
                usuario.Correo,
                "Registro exitoso - Lenguajes Formales",
                $"<p>¡Bienvenido, <strong>{nombre}</strong>!</p>" +
                "<p>Tu cuenta fue creada correctamente. Estos son tus datos de acceso:</p>" +
                $"<ul><li><strong>Enlace:</strong> <a href=\"{urlHtml}\">{urlHtml}</a></li>" +
                $"<li><strong>Correo:</strong> {correo}</li>" +
                $"<li><strong>Nickname:</strong> {nickname}</li></ul>" +
                (passwordTemporal is null
                    ? "<p>Tu acceso se realiza con la cuenta de Google que seleccionaste durante el registro.</p>"
                    : $"<p><strong>Contraseña temporal:</strong> {WebUtility.HtmlEncode(passwordTemporal)}</p>" +
                      "<p>Úsala para tu próximo ingreso. Después deberás crear una contraseña personal antes de continuar.</p>") +
                "<p>Adjuntamos tu credencial PDF con código QR.</p>",
                credencial,
                credencial is null ? null : nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo de registro a {Correo}", usuario.Correo);
        }

        if (usuario.MetodoNotificacion is "whatsapp" or "ambos")
        {
            var enviado = credencial is { Length: > 0 }
                ? await _whatsApp.SendPdfAsync(usuario.Telefono, credencial, nombreArchivo, mensajeWhatsApp)
                : await _whatsApp.SendTextAsync(usuario.Telefono, mensajeWhatsApp);

            if (!enviado)
                _logger.LogWarning("No se pudo enviar WhatsApp de registro al usuario {UsuarioId}", usuario.Id);
        }
    }

    // Registro temporal para desarrollo.
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
        respuesta.CodigoQr = _jwt.GenerarCodigoQrCredencial(usuario);
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
            usuario.DebeCambiarPassword = false;
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

    public async Task<AuthResponseDTO?> CambiarPasswordTemporalAsync(int usuarioId, string nuevoPassword)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo);
        if (usuario == null) return null;

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoPassword);
        usuario.DebeCambiarPassword = false;
        await _db.SaveChangesAsync();
        return CrearRespuestaSesion(usuario);
    }

    private static string GenerarPasswordTemporal()
    {
        const string mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minusculas = "abcdefghijkmnopqrstuvwxyz";
        const string numeros = "23456789";
        const string simbolos = "!@#$%*";
        const string caracteres = mayusculas + minusculas + numeros + simbolos;
        var password = new List<char>
        {
            mayusculas[RandomNumberGenerator.GetInt32(mayusculas.Length)],
            minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)],
            numeros[RandomNumberGenerator.GetInt32(numeros.Length)],
            simbolos[RandomNumberGenerator.GetInt32(simbolos.Length)]
        };

        while (password.Count < 14)
            password.Add(caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)]);

        return string.Concat(password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)));
    }

    private static string CrearTokenSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private AuthResponseDTO CrearRespuestaSesion(Usuario usuario, bool omitirCambioPassword = false) => new()
    {
        Token = _jwt.GenerarToken(usuario, usuario.DebeCambiarPassword && !omitirCambioPassword),
        Rol = usuario.Rol,
        Nickname = usuario.Nickname,
        Nombre = usuario.Nombre,
        FotoModificada = usuario.FotoModificada,
        Expiracion = DateTime.UtcNow.AddHours(8),
        DebeCambiarPassword = usuario.DebeCambiarPassword && !omitirCambioPassword
    };
}
