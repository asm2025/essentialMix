using System.Linq.Expressions;
using JetBrains.Annotations;

namespace essentialMix.Linq.Expressions;

internal class ExpressionReplacer([NotNull] Expression from, [NotNull] Expression to) : ExpressionVisitor
{
	private readonly Expression _from = from;
	private readonly Expression _to = to;

	public override Expression Visit(Expression node)
	{
		return node == _from
					? _to
					: base.Visit(node);
	}
}