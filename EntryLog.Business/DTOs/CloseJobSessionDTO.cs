using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs
{
    public record CloseJobSessionDTO(
        string SessionId,
        string UserId,
        string Latitude,
        string Longitude,
        IFormFile Image,
        string? Notes);
}
