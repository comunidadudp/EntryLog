using System.Linq.Expressions;

namespace EntryLog.Data.Specifications
{
    public abstract class Specification<TEntity> : ISpecification<TEntity>
        where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Expression { get; private set; } = _ => true;
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }


        public void AndAlso(Expression<Func<TEntity, bool>> expression)
        {
            Expression = SpecificationMethods<TEntity>.And(expression, Expression);
        }

        public void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        public void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }

        public void ApplyPagging(int take, int skip)
        {
            Take = take;
            Skip = skip;
            IsPagingEnabled = true;
        }
    }
}
