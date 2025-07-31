using EntryLog.Entities.POCOEntities;

namespace EntryLog.Data.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByCodeAsync(int code);
    }
}
