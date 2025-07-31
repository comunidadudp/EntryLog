using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntryLog.Data.Specifications
{
    public class RewriterVisitor : ExpressionVisitor
    {
        private readonly Expression _from, _to;

        public RewriterVisitor(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            return node == _from ? _to : base.Visit(node);
        }
    }
}
