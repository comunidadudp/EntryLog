namespace EntryLog.Web.Models
{
    public record OpenWorkSessionViewModel(
        string Latitude,
        string Longitude,
        IFormFile Image,
        string? Notes,
        string Descriptor,
        string Country,
        string City,
        string Neighbourhood
    );
}
