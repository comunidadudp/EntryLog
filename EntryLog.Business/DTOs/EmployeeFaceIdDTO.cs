namespace EntryLog.Business.DTOs
{
    public record EmployeeFaceIdDTO(
        string Base64Image,
        string RegisterDate,
        bool Active
    );
}
