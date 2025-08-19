using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces
{
    public interface IFaceIdService
    {
        Task<(bool success, string message, EmployeeFaceIdDTO? data)> CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDTO faceIdDTO);
        Task<EmployeeFaceIdDTO> GetFaceIdAsync(int employeeCode);
        Task<string> GenerateReferenceImageTokenAsync(string userId);
        Task<string> GetReferenceImageAsync(string authHeader);
    }
}
