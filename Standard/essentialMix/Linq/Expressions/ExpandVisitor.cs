using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Linq.Expressions
{
	internal class ExpandVisitor : ExpressionVisitor
	{
		private readonly IDictionary<ParameterExpression, Expression> _replaceVars;

		internal ExpandVisitor()
		{
		}

		private ExpandVisitor(IDictionary<ParameterExpression, Expression> replaceVars)
		{
			_replaceVars = replaceVars;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return _replaceVars != null && _replaceVars.ContainsKey(node)
						? _replaceVars[node]
						: base.VisitParameter(node);
		}

		protected override Expression VisitInvocation([NotNull] InvocationExpression node)
		{
			Expression target = node.Expression;
			if (target is MemberExpression expression) target = TransformExpr(expression);
			if (target is ConstantExpression constantExpression) target = constantExpression.Value as Expression;

			LambdaExpression lambda = (LambdaExpression)target ?? throw new InvalidOperationException("Expression target is null.");
			Dictionary<ParameterExpression, Expression> replaceVars = _replaceVars == null ? new Dictionary<ParameterExpression, Expression>() : new Dictionary<ParameterExpression, Expression>(_replaceVars);

			try
			{
				for (int i = 0; i < lambda.Parameters.Count; i++)
					replaceVars.Add(lambda.Parameters[i], node.Arguments[i]);
			}
			catch (ArgumentException ex)
			{
				throw new InvalidOperationException("Invoke cannot be called recursively. Try to use a temporary variable.", ex);
			}

			return new ExpandVisitor(replaceVars).Visit(lambda.Body) ?? throw new InvalidOperationException("Invalid expression.");
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			switch (node.Method.Name)
			{
				case "Invoke" when node.Method.DeclaringType == typeof(ExpressionExtension):
					Expression target = node.Arguments[0];
					if (target is MemberExpression expression) target = TransformExpr(expression);
					if (target is ConstantExpression constantExpression) target = constantExpression.Value as Expression;

					LambdaExpression lambda = (LambdaExpression)target ?? throw new InvalidOperationException("Target is null.");
					Dictionary<ParameterExpression, Expression> replaceVars = _replaceVars == null
						? new Dictionary<ParameterExpression, Expression>()
						: new Dictionary<ParameterExpression, Expression>(_replaceVars);

					try
					{
						for (int i = 0; i < lambda.Parameters.Count; i++)
							replaceVars.Add(lambda.Parameters[i], node.Arguments[i + 1]);
					}
					catch (ArgumentException ex)
					{
						throw new InvalidOperationException("Invoke cannot be called recursively. Try to use a temporary variable.", ex);
					}

					return new ExpandVisitor(replaceVars).Visit(lambda.Body) ?? throw new InvalidOperationException("Invalid expression.");
				case "Compile" when node.Object is MemberExpression me:
					Expression newExpr = TransformExpr(me);
					if (newExpr != me) return newExpr;
					break;
				case "AsExpandable" when node.Method.DeclaringType == typeof(ExpressionExtension):
					return node.Arguments[0];
			}

			return base.VisitMethodCall(node);
		}

		/// <inheritdoc />
		protected override Expression VisitMember(MemberExpression node)
		{
			return node.Member.DeclaringType != null && node.Member.DeclaringType.Name.StartsWith("<>")
						? TransformExpr(node)
						: base.VisitMember(node);
		}

		private Expression TransformExpr(MemberExpression node)
		{
			// Collapse captured outer variables
			if (!(node?.Member is FieldInfo) || node.Member.ReflectedType != null && (!node.Member.ReflectedType.IsNestedPrivate || !node.Member.ReflectedType.Name.StartsWith("<>")))
				return node;

			ConstantExpression expression = node.Expression as ConstantExpression;
			if (expression == null) return node;

			object obj = expression.Value;
			if (obj == null) return node;

			Type t = obj.GetType();
			if (!t.IsNestedPrivate || !t.Name.StartsWith("<>")) return node;

			FieldInfo fi = (FieldInfo)node.Member;
			object result = fi.GetValue(obj);
			return result is Expression expr ? Visit(expr) : node;
		}
	}
}