using System.Text;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LenguajesFormalesAPI.Data;
using LenguajesFormalesAPI.Middleware;
using LenguajesFormalesAPI.Services;
using LenguajesFormalesAPI.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// ── Base de datos ─────────────────────────────────────────────────────────
if (string.IsNullOrWhiteSpace(connectionString))
{
    // Base temporal para desarrollo.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("LenguajesFormalesDev"));
}
else
{
    // Base de datos PostgreSQL real.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// ── JWT Authentication ────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("Jwt:SecretKey debe contener al menos 32 bytes");

var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
var authentication = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sesión por cookie o encabezado Authorization.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue("umg_session", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };
    });

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authentication.AddCookie("GoogleExternal")
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SignInScheme = "GoogleExternal";
            options.CallbackPath = "/api/auth/google/callback";
            options.Scope.Add("openid");
            options.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");
            options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
            options.SaveTokens = true;
            options.Events.OnCreatingTicket = async context =>
            {
                if (!context.User.TryGetProperty("email_verified", out var verificado)
                    || verificado.ValueKind != System.Text.Json.JsonValueKind.True)
                {
                    context.Fail("Google no confirmó el correo de la cuenta.");
                }

                // El perfil OIDC estándar entrega nombre y correo. People API aporta
                // teléfono y fecha de nacimiento solo si el usuario los autoriza.
                try
                {
                    using var solicitud = new HttpRequestMessage(
                        HttpMethod.Get,
                        "https://people.googleapis.com/v1/people/me?personFields=names,phoneNumbers,birthdays");
                    solicitud.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                    using var respuesta = await context.Backchannel.SendAsync(solicitud, context.HttpContext.RequestAborted);
                    if (!respuesta.IsSuccessStatusCode) return;

                    using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                    var identidad = context.Principal?.Identity as ClaimsIdentity;
                    if (identidad == null) return;

                    var raiz = documento.RootElement;
                    if (raiz.TryGetProperty("names", out var nombres))
                    {
                        var apellido = nombres.EnumerateArray()
                            .Select(n => n.TryGetProperty("familyName", out var valor) ? valor.GetString() : null)
                            .FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor));
                        if (!string.IsNullOrWhiteSpace(apellido))
                            identidad.AddClaim(new Claim(ClaimTypes.Surname, apellido));
                    }

                    if (raiz.TryGetProperty("phoneNumbers", out var telefonos))
                    {
                        var telefono = telefonos.EnumerateArray()
                            .Select(t => t.TryGetProperty("value", out var valor) ? valor.GetString() : null)
                            .FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor));
                        if (!string.IsNullOrWhiteSpace(telefono))
                            identidad.AddClaim(new Claim("google_phone", telefono));
                    }

                    if (raiz.TryGetProperty("birthdays", out var fechas))
                    {
                        var fecha = fechas.EnumerateArray()
                            .Select(f => f.TryGetProperty("date", out var valor) ? valor : default)
                            .FirstOrDefault(f => f.ValueKind == JsonValueKind.Object);
                        if (fecha.ValueKind == JsonValueKind.Object
                            && fecha.TryGetProperty("year", out var ano)
                            && fecha.TryGetProperty("month", out var mes)
                            && fecha.TryGetProperty("day", out var dia)
                            && DateOnly.TryParse($"{ano.GetInt32():D4}-{mes.GetInt32():D2}-{dia.GetInt32():D2}", out var fechaNacimiento))
                        {
                            identidad.AddClaim(new Claim("google_birthdate", fechaNacimiento.ToString("yyyy-MM-dd")));
                        }
                    }
                }
                catch
                {
                    // El acceso básico con Google continúa si People API no está habilitada
                    // o el usuario no tiene esos campos en su perfil.
                }
            };
        });
}

// Los endpoints requieren sesión por defecto.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("facial-login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
        _ => new FixedWindowRateLimiterOptions
        {
            // Cada CAPTCHA habilita un máximo de tres intentos faciales.
            // Este límite adicional frena reintentos masivos desde una IP.
            PermitLimit = 9,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

// ── CORS (permite Nuxt frontend) ──────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"] ?? "http://localhost:5000",
                "http://localhost:3000",
                "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Servicios de aplicación ───────────────────────────────────────────────
builder.Services.AddScoped<IJwtService,             JwtService>();
builder.Services.AddScoped<IAuthService,            AuthService>();
builder.Services.AddScoped<IAnalisisLexicoService,  AnalisisLexicoService>();
builder.Services.AddScoped<IAnalisisReporteService, AnalisisReporteService>();
builder.Services.AddScoped<IRecaptchaService,       RecaptchaService>();
builder.Services.AddScoped<ICredentialService,      CredentialService>();
builder.Services.AddScoped<IFacialService,          FacialService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService,           EmailService>();
builder.Services.AddScoped<IWhatsAppService,        WhatsAppService>();

// ── Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Lenguajes Formales API",
        Version     = "v1",
        Description = "Sistema de Análisis Léxico — UMG 2026"
    });

    // JWT en Swagger UI
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Reference    = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseMiddleware<AuditMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lenguajes Formales API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

// Servir archivos estáticos (uploads de fotos, incluida la foto de enrolamiento facial)
// antes de autenticación: son recursos públicos y deben poder cargarse aun
// cuando una cuenta tenga pendiente el cambio de contraseña.
var carpetaUploads = Path.Combine(builder.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(carpetaUploads);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(carpetaUploads),
    RequestPath  = "/uploads",
    OnPrepareResponse = contexto =>
    {
        // No cachear fotografías.
        contexto.Context.Response.Headers["Cache-Control"] = "private, no-store, max-age=0";
    }
});

app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequiredMiddleware>();
app.UseAuthorization();

app.MapControllers();
// Esquema: backend/database/schema.sql.


app.Run();


