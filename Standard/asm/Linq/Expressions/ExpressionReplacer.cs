using System.Linq.Expressions;
using JetBrains.Annotations;

namespace asm.Linq.Expressions
{
	internal class ExpressionReplacer : ExpressionVisitor
	{
		private readonly Expression _from;
		private readonly Expression _to;

		public ExpressionReplacer([NotNull] Expression from, [NotNull] Expression to)
		{
			_from = from;
			_to = to;
		}
		public override Expression Visit(Expression node)
		{
			return node == _from
						? _to
						: base.Visit(node);
		}
	}
}