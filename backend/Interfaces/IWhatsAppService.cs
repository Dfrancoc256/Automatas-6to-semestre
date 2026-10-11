namespace LenguajesFormalesAPI.Interfaces
{
    public interface IWhatsAppService
    {
        /// <summary>
        /// Envía un mensaje de texto por WhatsApp (vía Evolution API).
        /// Devuelve false —sin lanzar excepción— si el servicio no está configurado o falla el envío.
        /// </summary>
        Task<bool> SendTextAsync(string telefono, string mensaje);

        /// <summary>Envía un PDF como documento adjunto por WhatsApp. Mismas reglas que SendTextAsync.</summary>
        Task<bool> SendPdfAsync(string telefono, byte[] pdf, string nombreArchivo, string? leyenda = null);
    }
}
