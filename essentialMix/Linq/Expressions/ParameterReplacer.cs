using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using essentialMix.Extensions;

namespace essentialMix.Linq.Expressions;

/// <inheritdoc />
internal class ParameterReplacer : ExpressionVisitor
{
	/// <inheritdoc />
	public ParameterReplacer(string parameterName, [NotNull] Expression replacement)
	{
		ParameterName = parameterName.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(parameterName));
		Replacement = replacement;
	}

	[NotNull]
	public string ParameterName { get; }

	[NotNull]
	public Expression Replacement { get; }

	/// <inheritdoc />
	protected override Expression VisitLambda<T>(Expression<T> node)
	{
		// create new lambda expression without the replaced parameter
		Expression expression = Visit(node.Body) ?? throw new InvalidOperationException("Invalid expression.");
		return Expression.Lambda(expression, node.Parameters.Where(x => x.Name != ParameterName).ToArray());
	}

	/// <inheritdoc />
	protected override Expression VisitParameter(ParameterExpression node)
	{
		return node.Name == ParameterName
					? Replacement
					: node;
	}
}