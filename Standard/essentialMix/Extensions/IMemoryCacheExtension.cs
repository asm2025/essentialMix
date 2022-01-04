using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using essentialMix.Exceptions.Caching;
using essentialMix.Exceptions.Expressions;
using essentialMix.Patterns.Key;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace essentialMix.Extensions;

public static class IMemoryCacheExtension
{
	public static T InvokeCached<T>([NotNull] this IMemoryCache thisValue, [NotNull] Expression<Func<T>> expression)
	{
		(string key, MethodInfo method, object instance, object[] arguments) = ParseExpression(expression);
		if (thisValue.TryGetValue(key, out T cachedValue)) return cachedValue;

		T computedValue = (T)method.Invoke(instance, arguments);
		if (computedValue is { } and not IConvertible) throw new NotSafeToCacheException(computedValue);
			
		ICacheEntry entry = thisValue.CreateEntry(key);
		entry.Value = computedValue;
		return computedValue;
	}

	public static async Task<T> InvokeCachedAsync<T>([NotNull] this IMemoryCache thisValue, [NotNull] Expression<Func<Task<T>>> expression)
	{
		(string key, MethodInfo method, object instance, object[] arguments) = ParseExpression(expression);
		if (thisValue.TryGetValue(key, out T cachedValue)) return cachedValue;

		T computedValue = await ((Task<T>)method.Invoke(instance, arguments)).ConfigureAwait(false);
		if (computedValue is { } and not IConvertible) throw new NotSafeToCacheException(computedValue);
			
		ICacheEntry entry = thisValue.CreateEntry(key);
		entry.Value = computedValue;
		return computedValue;
	}

	private static (string Key, MethodInfo Method, object Instance, object[] Arguments) ParseExpression([NotNull] LambdaExpression expression)
	{
		if (expression.Body is not MethodCallExpression methodCall) throw new ArgumentNotMethodExpressionException(nameof(expression));

		MethodInfo method = methodCall.Method;
		object instance = methodCall.Object != null ? GetValue(methodCall.Object) : null;
		object[] arguments = new object[methodCall.Arguments.Count];

		KeyBuilder keyBuilder = new KeyBuilder();
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
				object instance = memberExpression.Expression != null
									? GetValue(memberExpression.Expression)
									: null;
				return memberExpression.Member switch
				{
					FieldInfo fi => fi.GetValue(instance),
					PropertyInfo pi => pi.GetValue(instance),
					MethodInfo mi => mi.Invoke(instance, memberExpression.GetValues()),
					_ => throw new InvalidOperationException("Member expression is not valid for this operation.")
				};
			default:
				// this should always work if the expression CAN be evaluated (it can't if it references unbound parameters, for example)
				// however, compilation is slow so the cases above provide valuable performance
				Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)));
				return lambda.Compile()();
		}
	}
}