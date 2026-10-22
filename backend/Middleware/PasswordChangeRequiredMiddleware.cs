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

        // Estas rutas permiten recuperar el formulario tras recargar, volver a
        // autenticarse con la contraseña temporal y completar el cambio.
        var rutaPermitida = ruta.StartsWithSegments("/api/auth/cambiar-password-temporal")
            || ruta.StartsWithSegments("/api/auth/logout")
            || ruta.StartsWithSegments("/api/auth/login")
            || ruta.StartsWithSegments("/api/auth/login-facial")
            || ruta.StartsWithSegments("/api/auth/sesion");

        if (requiereCambio
            && !rutaPermitida)
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
