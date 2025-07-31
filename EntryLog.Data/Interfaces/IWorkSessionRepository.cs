using EntryLog.Data.Specifications;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Data.Interfaces
{
    public interface IWorkSessionRepository
    {
        Task CreateAsync(WorkSession workSession);
        Task UpdateAsync(WorkSession workSession);
        Task<WorkSession> GetByIdAsync(Guid id);
        Task<WorkSession> GetByEmployeeId(int id);
        Task<IEnumerable<WorkSession>> GetAllAsync(Specification<WorkSession> spec);
        Task<WorkSession> GetActiveSessionByEmployeeIdAsync(int id);
    }
}
