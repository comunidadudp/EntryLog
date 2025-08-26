namespace EntryLog.Web.Models
{
    public record CloseWorkSessionViewModel(
        string SessionId,
        string Latitude,
        string Longitude,
        IFormFile Image,
        string? Notes,
        string Descriptor
    );
}
