namespace LenguajesFormalesAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string to,
            string subject,
            string body,
            byte[]? adjunto = null,
            string? nombreAdjunto = null
        );
    }
}