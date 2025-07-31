namespace EntryLog.Business.DTOs
{
    public record GetWorkSessionDTO(
        string Id,
        int EmployeeId,
        GetCheckDTO CheckIn,
        GetCheckDTO? CheckOut,
        TimeSpan? TotalWorked,
        string Status
        );

    public record GetCheckDTO(
        string Method,
        string? DeviceName,
        DateTime Date,
        GetLocationDTO Location,
        string PhotoUrl,
        string? Notes
        );

    public record GetLocationDTO(
        string Latitude,
        string Longtitude,
        string IpAddress);
}
