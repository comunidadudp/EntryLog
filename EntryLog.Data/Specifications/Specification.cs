using System.Linq.Expressions;

namespace EntryLog.Data.Specifications
{
    public abstract class Specification<TEntity> where TEntity : class
    {
        public abstract Expression<Func<TEntity,bool>> Expression { get; set; }
        public abstract void AndAlso(Expression<Func<TEntity, bool>> expression);
    }
}
