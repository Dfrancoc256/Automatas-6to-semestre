namespace LenguajesFormalesAPI.Middleware;

/// <summary>
/// Middleware que registra en los logs cada petición HTTP con información de auditoría.
/// No expone datos sensibles en la respuesta.
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "-";
            var user = context.User.Identity?.Name ?? "anónimo";

            _logger.LogInformation(
                "[AUDIT] {Method} {Path} → {Status} | Usuario: {User} | IP: {Ip} | {Elapsed:0}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode,
                user, ip, elapsed);
        }
    }
}
