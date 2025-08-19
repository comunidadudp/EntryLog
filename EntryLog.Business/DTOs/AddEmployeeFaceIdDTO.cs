using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.DTOs
{
    public record AddEmployeeFaceIdDTO(
        int EmployeeCode,
        IFormFile Image,
        string Descriptor
    );
}
