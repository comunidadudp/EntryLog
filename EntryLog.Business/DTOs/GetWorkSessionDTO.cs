namespace EntryLog.Business.DTOs
{
    public record GetWorkSessionDTO(
        string Id,
        int EmployeeId,
        GetCheckDTO CheckIn,
        GetCheckDTO? CheckOut,
        string? TotalWorked,
        string Status
        );

    public record GetCheckDTO(
        string Method,
        string? DeviceName,
        string Date,
        GetLocationDTO Location,
        string PhotoUrl,
        string? Notes
        );

    public record GetLocationDTO(
        string Latitude,
        string Longitude,
        string Country,
        string City,
        string Neighbourhood,
        string IpAddress);
}
