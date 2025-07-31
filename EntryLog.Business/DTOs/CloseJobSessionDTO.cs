using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs
{
    public record CloseJobSessionDTO(
        string SessionId,
        string UserId,
        string Method,
        string DeviceName,
        string Latitude,
        string Longitude,
        string IpAddress,
        IFormFile Image,
        string? Notes);
}
