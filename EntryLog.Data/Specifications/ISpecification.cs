using System.Linq.Expressions;

namespace EntryLog.Data.Specifications
{
    public interface ISpecification<TEntity> where TEntity : class
    {
        Expression<Func<TEntity, bool>> Expression { get; }
        Expression<Func<TEntity, object>>? OrderBy { get; }
        Expression<Func<TEntity, object>>? OrderByDescending { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
