using System.Linq.Expressions;

namespace EntryLog.Data.Specifications
{
    public static class SpecificationMethods<TEntity> where TEntity : class
    {
        public static Expression<Func<TEntity,bool>> And(
            Expression<Func<TEntity, bool>> from,
            Expression<Func<TEntity, bool>> to)
        {
            return Expression.Lambda<Func<TEntity, bool>>(
                        Expression.AndAlso(
                        new RewriterVisitor(from.Parameters[0], to.Parameters[0])
                            .Visit(from.Body), to.Body), to.Parameters);
        }
    }
}
