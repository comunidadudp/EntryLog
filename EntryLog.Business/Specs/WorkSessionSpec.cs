using EntryLog.Data.Specifications;
using EntryLog.Entities.POCOEntities;
using System.Linq.Expressions;

namespace EntryLog.Business.Specs
{
    internal class WorkSessionSpec : Specification<WorkSession>
    {
        public override Expression<Func<WorkSession, bool>> Expression { get; set; } = _ => true;

        public override void AndAlso(Expression<Func<WorkSession, bool>> expression)
        {
            Expression = SpecificationMethods<WorkSession>.And(expression, Expression);
        }
    }
}
