using EntryLog.Business.DTOs;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces
{
    public interface IWorkSessionServices
    {
        Task<(bool success, string message)> OpenJobSession(CreateJoSessionDTO sessionDTO);
        Task<(bool success, string message)> CloseJobSession(CloseJobSessionDTO sessionDTO);
        Task<IEnumerable<GetWorkSessionDTO>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
    }
}
