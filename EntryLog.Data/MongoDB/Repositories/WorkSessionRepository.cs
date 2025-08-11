using EntryLog.Data.Constants;
using EntryLog.Data.Interfaces;
using EntryLog.Data.Specifications;
using EntryLog.Entities.POCOEntities;
using MongoDB.Driver;

namespace EntryLog.Data.MongoDB.Repositories
{
    internal class WorkSessionRepository(IMongoDatabase database) : IWorkSessionRepository
    {
        private readonly IMongoCollection<WorkSession> _collection
            = database.GetCollection<WorkSession>(CollectionNames.WorkSessions);

        public async Task<int> CountAsync(ISpecification<WorkSession> spec)
        {
            return await BaseRepository<WorkSession>.CountAsync(_collection.AsQueryable(), spec);
        }

        public async Task CreateAsync(WorkSession workSession)
        {
            await _collection.InsertOneAsync(workSession);
        }

        public async Task<WorkSession> GetActiveSessionByEmployeeIdAsync(int id)
        {
            return await _collection.Find(
                x => x.EmployeeId == id &&
                x.Status == Entities.Enums.SessionStatus.InProgress).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WorkSession>> GetAllAsync(ISpecification<WorkSession> spec)
        {
            return await BaseRepository<WorkSession>.GetAllBySpecificationAsync(_collection.AsQueryable(), spec);
        }

        public async Task<WorkSession> GetByEmployeeId(int id)
        {
            return await _collection.Find(x => x.EmployeeId == id).FirstOrDefaultAsync();
        }

        public async Task<WorkSession> GetByIdAsync(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(WorkSession workSession)
        {
            await _collection.ReplaceOneAsync(x => x.Id == workSession.Id, workSession);
        }
    }
}
