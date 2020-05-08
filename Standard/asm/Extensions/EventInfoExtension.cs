using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class EventInfoExtension
	{
		private const string INVOKE = "Invoke";

		public static Delegate CreateDelegate([NotNull] this EventInfo thisValue, [NotNull] EventHandler handler)
		{
			MethodInfo eventInvoke = thisValue.EventHandlerType.GetMethodInfoFromEventDelegate();
			MethodInfo actionInvoke = handler.GetType().GetMethod(INVOKE) ?? throw new ArgumentException($"Cannot get {INVOKE} method.", nameof(handler));
			ParameterExpression[] parameters = eventInvoke.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
			MethodCallExpression expr = Expression.Call(Expression.Constant(handler), actionInvoke, parameters.Cast<Expression>());
			LambdaExpression lambdaExpr = Expression.Lambda(expr, parameters);
			return Delegate.CreateDelegate(thisValue.EventHandlerType, lambdaExpr.Compile(), INVOKE, false);
		}

		public static Delegate CreateDelegate<TArg>([NotNull] this EventInfo thisValue, [NotNull] EventHandler<TArg> handler)
			where TArg : EventArgs
		{
			MethodInfo eventInvoke = thisValue.EventHandlerType.GetMethodInfoFromEventDelegate();
			MethodInfo actionInvoke = handler.GetType().GetMethod(INVOKE) ?? throw new ArgumentException($"Cannot get {INVOKE} method.", nameof(handler));
			ParameterExpression[] parameters = eventInvoke.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
			MethodCallExpression expr = Expression.Call(Expression.Constant(handler), actionInvoke, parameters.Cast<Expression>());
			LambdaExpression lambdaExpr = Expression.Lambda(expr, parameters);
			return Delegate.CreateDelegate(thisValue.EventHandlerType, lambdaExpr.Compile(), INVOKE, false);
		}
	}
}