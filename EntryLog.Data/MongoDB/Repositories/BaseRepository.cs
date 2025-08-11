using EntryLog.Data.Evaluators;
using EntryLog.Data.Specifications;
using MongoDB.Driver.Linq;

namespace EntryLog.Data.MongoDB.Repositories
{
    internal static class BaseRepository<TEntity> where TEntity : class
    {
        public static async Task<int> CountAsync
            (IQueryable<TEntity> query, ISpecification<TEntity> specification)
            => await ApplySpecification(query, specification).CountAsync();

        public static async Task<IEnumerable<TEntity>> GetAllBySpecificationAsync
            (IQueryable<TEntity> query, ISpecification<TEntity> specification)
            => await ApplySpecification(query, specification).ToListAsync();

        private static IQueryable<TEntity> ApplySpecification
            (IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
            => SpecificationEvaluator<TEntity>.GetQuery(inputQuery, specification);
    }
}
