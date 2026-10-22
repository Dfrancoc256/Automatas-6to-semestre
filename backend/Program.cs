using System.Text;
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
            options.Events.OnCreatingTicket = context =>
            {
                if (!context.User.TryGetProperty("email_verified", out var verificado)
                    || verificado.ValueKind != System.Text.Json.JsonValueKind.True)
                {
                    context.Fail("Google no confirmó el correo de la cuenta.");
                }
                return Task.CompletedTask;
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

app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequiredMiddleware>();
app.UseAuthorization();

// Servir archivos estáticos (uploads de fotos, incluida la foto de enrolamiento facial)
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

app.MapControllers();
// Esquema: backend/database/schema.sql.


app.Run();


