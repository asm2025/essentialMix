using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace essentialMix.Linq.Expressions
{
	//https://stackoverflow.com/questions/1717444/combining-two-lambda-expressions-in-c-sharp/1720642#1720642
	// missing some cases that need more work
	[Obsolete]
	internal class ExpressionWriter
	{
		private readonly ConcurrentDictionary<Expression, Expression> _subst;
		private bool _isLocked, _inline;

		public ExpressionWriter()
		{
			_subst = new ConcurrentDictionary<Expression, Expression>();
		}

		private ExpressionWriter([NotNull] ExpressionWriter parent)
		{
			if (parent == null)
				throw new ArgumentNullException(nameof(parent));
			_subst = new ConcurrentDictionary<Expression, Expression>(parent._subst);
			_inline = parent._inline;
		}

		[NotNull]
		public ExpressionWriter Subst([NotNull] Expression from, Expression to)
		{
			CheckLocked();
			_subst.TryAdd(from, to);
			return this;
		}

		[NotNull]
		public ExpressionWriter Inline()
		{
			CheckLocked();
			_inline = true;
			return this;
		}

		public Expression Apply(Expression expression)
		{
			_isLocked = true;
			return Walk(expression) ?? expression;
		}

		internal Expression AutoInline([NotNull] InvocationExpression expression)
		{
			_isLocked = true;
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			LambdaExpression lambda = (LambdaExpression)expression.Expression;
			ExpressionWriter childScope = new ExpressionWriter(this);
			ReadOnlyCollection<ParameterExpression> lambdaParams = lambda.Parameters;
			ReadOnlyCollection<Expression> invokeArgs = expression.Arguments;
			if (lambdaParams.Count != invokeArgs.Count)
				throw new InvalidOperationException("Lambda/invoke mismatch.");

			for (int i = 0; i < lambdaParams.Count; i++)
				childScope.Subst(lambdaParams[i], invokeArgs[i]);

			return childScope.Apply(lambda.Body);
		}

		private void CheckLocked()
		{
			if (!_isLocked)
				return;
			throw new InvalidOperationException("Cannot alter the writer after Apply method has been called.");
		}

		private Expression[] Walk(IEnumerable<Expression> expressions) { return expressions?.Select(Walk).ToArray(); }

		// returns null if no need to rewrite that branch, otherwise
		// returns a re-written branch
		private Expression Walk(Expression expression)
		{
			if (expression == null)
				return null;

			if (_subst.TryGetValue(expression, out Expression tmp))
				return tmp;

			switch (expression.NodeType)
			{
				case ExpressionType.Constant:
				case ExpressionType.Parameter:
				{
					return expression; // never need to rewrite if not already matched
				}
				case ExpressionType.MemberAccess:
				{
					MemberExpression me = (MemberExpression)expression;
					Expression target = Walk(me.Expression);
					return target == null ? null : Expression.MakeMemberAccess(target, me.Member);
				}
				case ExpressionType.Add:
				case ExpressionType.Divide:
				case ExpressionType.Multiply:
				case ExpressionType.Subtract:
				case ExpressionType.AddChecked:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.SubtractChecked:
				case ExpressionType.And:
				case ExpressionType.Or:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.AndAlso:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.Modulo:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.RightShift:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				{
					BinaryExpression binExp = (BinaryExpression)expression;
					Expression left = Walk(binExp.Left), right = Walk(binExp.Right);
					return left == null && right == null
						? null
						: Expression.MakeBinary(binExp.NodeType, left ?? binExp.Left, right ?? binExp.Right, binExp.IsLiftedToNull, binExp.Method, binExp.Conversion);
				}
				case ExpressionType.Not:
				case ExpressionType.UnaryPlus:
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.TypeAs:
				case ExpressionType.ArrayLength:
				{
					UnaryExpression unExp = (UnaryExpression)expression;
					Expression operand = Walk(unExp.Operand);
					return operand == null
						? null
						: Expression.MakeUnary(unExp.NodeType, operand,
							unExp.Type, unExp.Method);
				}
				case ExpressionType.Conditional:
				{
					ConditionalExpression ce = (ConditionalExpression)expression;
					Expression test = Walk(ce.Test), ifTrue = Walk(ce.IfTrue), ifFalse = Walk(ce.IfFalse);
					return test == null && ifTrue == null && ifFalse == null
								? null
								: Expression.Condition(test ?? ce.Test, ifTrue ?? ce.IfTrue, ifFalse ?? ce.IfFalse);
				}
				case ExpressionType.Call:
				{
					MethodCallExpression mce = (MethodCallExpression)expression;
					Expression instance = Walk(mce.Object);
					Expression[] args = Walk(mce.Arguments);
					return instance == null && !HasValue(args)
								? null
								: Expression.Call(instance, mce.Method, CoalesceTerms(args, mce.Arguments));
				}
				case ExpressionType.TypeIs:
				{
					TypeBinaryExpression tbe = (TypeBinaryExpression)expression;
					tmp = Walk(tbe.Expression);
					return tmp == null ? null : Expression.TypeIs(tmp, tbe.TypeOperand);
				}
				case ExpressionType.New:
				{
					NewExpression ne = (NewExpression)expression;
					Expression[] args = Walk(ne.Arguments);
					return HasValue(args)
								? null
								: ne.Members == null
									? Expression.New(ne.Constructor, CoalesceTerms(args, ne.Arguments))
									: Expression.New(ne.Constructor, CoalesceTerms(args, ne.Arguments), ne.Members);
				}
				case ExpressionType.ListInit:
				{
					ListInitExpression lie = (ListInitExpression)expression;
					NewExpression ctor = (NewExpression)Walk(lie.NewExpression);
					var inits = lie.Initializers.Select(init => new
					{
						Original = init,
						NewArgs = Walk(init.Arguments)
					}).ToArray();
					if (ctor == null && !inits.Any(init => HasValue(init.NewArgs)))
						return null;
					ElementInit[] initArr = inits.Select(init => Expression.ElementInit(
						init.Original.AddMethod, CoalesceTerms(init.NewArgs, init.Original.Arguments))).ToArray();
					return Expression.ListInit(ctor ?? lie.NewExpression, initArr);
				}
				case ExpressionType.Lambda:
				{
					LambdaExpression lambda = (LambdaExpression)expression;
					return Walk(lambda.Body);
				}
				case ExpressionType.NewArrayBounds:
				case ExpressionType.NewArrayInit:
				//{
					// todo not quite right... leave as not-implemented for now
					//NewArrayExpression nae = (NewArrayExpression)expression;
					//Expression[] expr = Walk(nae.Expressions);
					//if (!HasValue(expr)) return null;
					//return expression.NodeType == ExpressionType.NewArrayBounds
					//    ? Expression.NewArrayBounds(nae.Type, CoalesceTerms(expr, nae.Expressions))
					//    : Expression.NewArrayInit(nae.Type, CoalesceTerms(expr, nae.Expressions));
				//}
				case ExpressionType.Invoke:
				case ExpressionType.MemberInit:
				case ExpressionType.Quote:
					throw new NotImplementedException($"Node type {expression.NodeType} is not implemented.");
				default:
					throw new NotSupportedException($"Node type {expression.NodeType} is not supported.");
			}
		}

		private static IEnumerable<Expression> CoalesceTerms(IEnumerable<Expression> sourceWithNulls, IEnumerable<Expression> replacements)
		{
			if (sourceWithNulls == null || replacements == null)
				yield break;

			using (IEnumerator<Expression> left = sourceWithNulls.GetEnumerator())
			{
				using (IEnumerator<Expression> right = replacements.GetEnumerator())
				{
					while (left.MoveNext() && right.MoveNext())
					{
						yield return left.Current ?? right.Current;
					}
				}
			}
		}

		private static bool HasValue(Expression[] expressions) { return expressions != null && expressions.Any(expr => expr != null); }
	}
}