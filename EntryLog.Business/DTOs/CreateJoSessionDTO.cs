using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs
{
    public record CreateJoSessionDTO(
        string UserId,
        string Method,
        string DeviceName,
        string Latitude,
        string Longitude,
        string IpAddress,
        IFormFile Image,
        string? Notes);
}
