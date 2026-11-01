using System.Security.Claims;

namespace LenguajesFormalesAPI.Middleware;

public class PasswordChangeRequiredMiddleware
{
    private readonly RequestDelegate _next;

    public PasswordChangeRequiredMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var requiereCambio = context.User.Identity?.IsAuthenticated == true
            && context.User.FindFirst("debe_cambiar_password")?.Value == "true";
        var ruta = context.Request.Path;

        if (requiereCambio
            && !ruta.StartsWithSegments("/api/auth/login")
            && !ruta.StartsWithSegments("/api/auth/cambiar-password-temporal")
            && !ruta.StartsWithSegments("/api/auth/validar-enlace-inicial")
            && !ruta.StartsWithSegments("/api/auth/cambiar-password-inicial")
            && !ruta.StartsWithSegments("/api/auth/solicitar-restablecimiento")
            && !ruta.StartsWithSegments("/api/auth/restablecer-password")
            && !ruta.StartsWithSegments("/api/auth/credencial")
            && !ruta.StartsWithSegments("/api/auth/logout"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Debes crear una contraseña personal antes de continuar."
            });
            return;
        }

        await _next(context);
    }
}
