using EntryLog.Business.DTOs;
using EntryLog.Business.QueryFilters;
using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Interfaces
{
    public interface IWorkSessionServices
    {
        Task<(bool success, string message)> OpenJobSession(CreateJobSessionDTO sessionDTO);
        Task<(bool success, string message)> CloseJobSession(CloseJobSessionDTO sessionDTO);
        Task<IEnumerable<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
        Task<(bool success, string message)> ImageTestAsync(IFormFile image);
    }
}
