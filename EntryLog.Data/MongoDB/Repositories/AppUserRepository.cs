using EntryLog.Data.Constants;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using MongoDB.Driver;

namespace EntryLog.Data.MongoDB.Repositories
{
    internal class AppUserRepository(IMongoDatabase database) : IAppUserRepository
    {
        private readonly IMongoCollection<AppUser> _collection 
            = database.GetCollection<AppUser>(CollectionNames.Users);

        public async Task CreateAsync(AppUser user)
        {
            await _collection.InsertOneAsync(user);
        }

        public async Task<AppUser> GetByCodeAsync(int code)
        {
            return await _collection.Find(x => x.Code == code).FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetByIdAsync(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetByRecoveryTokenAsync(string token)
        {
            return await _collection.Find(x => x.RecoveryToken == token && x.RecoveryTokenActive).FirstOrDefaultAsync();
        }

        public async Task<AppUser> GetByUsernameAsync(string username)
        {
            return await _collection.Find(x => x.Email == username).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(AppUser user)
        {
            await _collection.ReplaceOneAsync(x => x.Id == user.Id, user);
        }
    }
}
