using EntryLog.Data.Specifications;

namespace EntryLog.Data.Evaluators
{
    internal class SpecificationEvaluator<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery
            (IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
        {
            if (specification == null)
            {
                return inputQuery;
            }

            var query = inputQuery.Where(specification.Expression);

            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query
                    .Skip(specification.Skip)
                    .Take(specification.Take);
            }

            return query;
        }
    }
}
