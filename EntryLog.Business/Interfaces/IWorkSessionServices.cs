using EntryLog.Business.DTOs;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces
{
    public interface IWorkSessionServices
    {
        Task<(bool success, string message, GetWorkSessionDTO? data)> OpenJobSessionAsync(CreateWorkSessionDTO sessionDTO);
        Task<(bool success, string message)> CloseJobSessionAsync(CloseJobSessionDTO sessionDTO);
        Task<PaginatedResult<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
    }
}
