using EntryLog.Entities.POCOEntities;

namespace EntryLog.Data.Interfaces
{
    public interface IAppUserRepository
    {
        Task CreateAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task<AppUser> GetByUsernameAsync(string username);
        Task<AppUser> GetByIdAsync(Guid id);
        Task<AppUser> GetByCodeAsync(int code);
        Task<AppUser> GetByRecoveryTokenAsync(string token);
    }
}
