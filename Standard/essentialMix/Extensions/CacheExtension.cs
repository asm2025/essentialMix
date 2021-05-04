using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using essentialMix.Caching;
using essentialMix.Exceptions.Caching;
using essentialMix.Exceptions.Expressions;

namespace essentialMix.Extensions
{
	public static class CacheExtension
	{
		public static TResult InvokeCached<TResult>([NotNull] this Cache thisValue, [NotNull] Expression<Func<TResult>> expression)
		{
			(string Key, MethodInfo Method, object Instance, object[] Arguments) exp = ParseExpression(expression);
			if (thisValue.TryGetValue(exp.Key, out TResult cachedValue)) return cachedValue;

			TResult computedValue = (TResult)exp.Method.Invoke(exp.Instance, exp.Arguments);
			if (computedValue is { } and not IConvertible and not ICacheable)
				throw new NotSafeToCacheException(computedValue);
			ICacheEntry entry = thisValue.CreateEntry(exp.Key);
			entry.Value = computedValue;
			return computedValue;
		}

		public static async Task<TResult> InvokeCachedAsync<TResult>([NotNull] this Cache thisValue, [NotNull] Expression<Func<Task<TResult>>> expression)
		{
			(string Key, MethodInfo Method, object Instance, object[] Arguments) exp = ParseExpression(expression);
			if (thisValue.TryGetValue(exp.Key, out TResult cachedValue)) return cachedValue;

			TResult computedValue = await ((Task<TResult>)exp.Method.Invoke(exp.Instance, exp.Arguments)).ConfigureAwait(false);
			if (computedValue is { } and not IConvertible and not ICacheable)
				throw new NotSafeToCacheException(computedValue);
			ICacheEntry entry = thisValue.CreateEntry(exp.Key);
			entry.Value = computedValue;
			return computedValue;
		}

		private static (string Key, MethodInfo Method, object Instance, object[] Arguments) ParseExpression([NotNull] LambdaExpression expression)
		{
			if (expression.Body is not MethodCallExpression methodCall)
				throw new ArgumentNotMethodExpressionException(nameof(expression));

			MethodInfo method = methodCall.Method;
			object instance = methodCall.Object != null ? GetValue(methodCall.Object) : null;
			object[] arguments = new object[methodCall.Arguments.Count];

			CacheKeyBuilder keyBuilder = new CacheKeyBuilder();
			keyBuilder.By(method.DeclaringType).By(method.MetadataToken).By(method.GetGenericArguments()).By(instance);

			for (int i = 0; i < methodCall.Arguments.Count; ++i)
				keyBuilder.By(arguments[i] = GetValue(methodCall.Arguments[i]));

			string cacheKey = keyBuilder.ToString();
			return (cacheKey, method, instance, arguments);
		}

		private static object GetValue([NotNull] Expression expression)
		{
			switch (expression.NodeType)
			{
				// we special-case constant and member access because these handle the majority of common cases.
				// For example, passing a local variable as an argument translates to a field reference on the closure
				// object in expression land
				case ExpressionType.Constant:
					return ((ConstantExpression)expression).Value;
				case ExpressionType.MemberAccess:
					MemberExpression memberExpression = (MemberExpression)expression;
					object instance = memberExpression.Expression != null ? GetValue(memberExpression.Expression) : null;
					FieldInfo field = memberExpression.Member as FieldInfo;
					return field != null
								? field.GetValue(instance)
								: ((PropertyInfo)memberExpression.Member).GetValue(instance);
				default:
					// this should always work if the expression CAN be evaluated (it can't if it references unbound parameters, for example)
					// however, compilation is slow so the cases above provide valuable performance
					Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)));
					return lambda.Compile()();
			}
		}
	}
}