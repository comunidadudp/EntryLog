using EntryLog.Business.DTOs;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces
{
    public interface IWorkSessionServices
    {
        Task<(bool success, string message, GetWorkSessionDTO? data)> OpenSessionAsync(OpenSessionDTO sessionDTO);
        Task<(bool success, string message, GetWorkSessionDTO? data)> CloseSessionAsync(CloseSessionDTO sessionDTO);
        Task<PaginatedResult<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
        Task<GetWorkSessionDTO> GetSessionByIdAsync(string id);
        Task<bool> HasActiveAnySessionAsync(int employeeCode);
        Task<IEnumerable<GetLocationDTO>> GetLastLocationsByEmployeeAsync(int employeeCode);
    }
}
