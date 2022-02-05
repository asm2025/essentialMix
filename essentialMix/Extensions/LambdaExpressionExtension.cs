using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using essentialMix.Linq.Expressions;
using essentialMix.Reflection;

namespace essentialMix.Extensions;

/// <summary>
/// This is largely based on Joseph and Ben Albahari and Eric Johannsen
/// <see href="http://www.albahari.com/nutshell/predicatebuilder.aspx">predicate builder</see>
/// </summary>
public static class LambdaExpressionExtension
{
	public static Expression Expand([NotNull] this LambdaExpression thisValue) { return new ExpandVisitor().Visit(thisValue); }

	[NotNull]
	public static LambdaExpression Or([NotNull] this LambdaExpression thisValue, [NotNull] LambdaExpression value)
	{
		// http://www.albahari.com/nutshell/predicatebuilder.aspx
		InvocationExpression invokedExpr = Expression.Invoke(value, thisValue.Parameters);
		return Expression.Lambda(Expression.OrElse(thisValue.Body, invokedExpr), thisValue.Parameters);
	}

	[NotNull]
	public static LambdaExpression And([NotNull] this LambdaExpression thisValue, [NotNull] LambdaExpression value)
	{
		// http://www.albahari.com/nutshell/predicatebuilder.aspx
		InvocationExpression invokedExpr = Expression.Invoke(value, thisValue.Parameters);
		return Expression.Lambda(Expression.AndAlso(thisValue.Body, invokedExpr), thisValue.Parameters);
	}

	[NotNull]
	public static LambdaExpression Combine([NotNull] this LambdaExpression thisValue, [NotNull] LambdaExpression inner)
	{
		InvocationExpression invoke = Expression.Invoke(inner, thisValue.Body);
		return Expression.Lambda(invoke, thisValue.Parameters);
	}

	public static LambdaExpression ReplaceExpression([NotNull] this LambdaExpression thisValue, [NotNull] LambdaExpression searchExpr, [NotNull] LambdaExpression replaceExpr)
	{
		return (LambdaExpression)new ExpressionReplacer(searchExpr, replaceExpr).Visit(thisValue);
	}

	public static LambdaExpression ReplaceParameter<T>([NotNull] this LambdaExpression thisValue, [NotNull] string parameterName, T value)
	{
		return (LambdaExpression)new ParameterReplacer(parameterName, Expression.Constant(value)).Visit(thisValue);
	}

	public static LambdaExpression ToExpression(this LambdaExpression thisValue) { return thisValue; }

	[NotNull]
	public static LambdaExpression Cast<TTo>([NotNull] this LambdaExpression thisValue)
	{
		Expression castExpr = Expression.ConvertChecked(thisValue.Body, typeof(TTo));
		return Expression.Lambda(castExpr, thisValue.Parameters);
	}

	[NotNull]
	public static PropertyPath GetSimplePropertyAccess([NotNull] this LambdaExpression thisValue)
	{
		PropertyPath propertyPath = thisValue.Parameters.Single().MatchSimplePropertyAccess(thisValue.Body);
		return propertyPath ?? throw new InvalidOperationException();
	}

	[NotNull]
	public static PropertyPath GetComplexPropertyAccess([NotNull] this LambdaExpression thisValue)
	{
		PropertyPath propertyPath = thisValue.Parameters.Single().MatchComplexPropertyAccess(thisValue.Body);
		return propertyPath ?? throw new InvalidOperationException();
	}

	[NotNull]
	public static IEnumerable<PropertyPath> GetSimplePropertyAccessList([NotNull] this LambdaExpression thisValue)
	{
		IEnumerable<PropertyPath> propertyPaths = MatchPropertyAccessList(thisValue, (p, e) => e.MatchSimplePropertyAccess(p));
		return propertyPaths ?? throw new InvalidOperationException();
	}

	[NotNull]
	public static IReadOnlyList<PropertyPath> GetComplexPropertyAccessList([NotNull] this LambdaExpression thisValue)
	{
		IReadOnlyList<PropertyPath> propertyPaths = MatchPropertyAccessList(thisValue, (p, e) => e.MatchComplexPropertyAccess(p));
		return propertyPaths ?? throw new InvalidOperationException();
	}

	private static IReadOnlyList<PropertyPath> MatchPropertyAccessList([NotNull] LambdaExpression expression, Func<Expression, Expression, PropertyPath> propertyMatcher)
	{
		if (expression.RemoveConvert() is NewExpression newExpression)
		{
			ParameterExpression parameterExpression = expression.Parameters.Single();
			List<PropertyPath> propertyPaths = newExpression.Arguments
															.Select(a => propertyMatcher(a, parameterExpression))
															.Where(p => p != null)
															.ToList();
				
			if (propertyPaths.Count == newExpression.Arguments.Count)
			{
				return !newExpression.HasDefaultMembersOnly(propertyPaths)
							? null
							: propertyPaths;
			}
		}

		PropertyPath propertyPath = propertyMatcher(expression.Body, expression.Parameters.Single());
		return propertyPath == null
					? null
					: new[]
					{
						propertyPath
					};
	}
}