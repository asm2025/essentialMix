using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class MethodInfoExtension
{
	[NotNull]
	public static TDelegate CreateDelegate<TDelegate>([NotNull] this MethodInfo thisValue) { return CreateDelegate<TDelegate>(thisValue, null); }

	/// <summary>
	/// Creates a delegate of a specified type that represents the specified
	/// static or instance method, with the specified first argument.
	/// Conversions are done when possible.
	/// https://codereview.stackexchange.com/questions/1070/generic-advanced-delegate-createdelegate-using-expression-trees
	/// </summary>
	/// <typeparam name = "TDelegate">The type for the delegate.</typeparam>
	/// <param name = "thisValue">
	/// The MethodInfo describing the static or
	/// instance method the delegate is to represent.
	/// </param>
	/// <param name = "instance">
	/// The object to which the delegate is bound,
	/// or null to treat method as static
	/// </param>
	[NotNull]
	public static TDelegate CreateDelegate<TDelegate>([NotNull] this MethodInfo thisValue, object instance)
	{
		MethodInfo delegateInfo = typeof(TDelegate).GetMethodInfoFromDelegate();
		ParameterInfo[] delegateParameters = delegateInfo.GetParameters();
		IEnumerable<Type> delegateTypes = delegateParameters.Select(d => d.ParameterType);
		ParameterExpression[] delegateArguments = delegateParameters.Select(d => Expression.Parameter(d.ParameterType)).ToArray();
		IEnumerable<Type> methodTypes = thisValue.GetParameters().Select(m => m.ParameterType);

		// Convert the arguments from the delegate argument type
		// to the method argument type when necessary.
		IEnumerable<Expression> convertedArguments = methodTypes.Zip(
																	delegateTypes, delegateArguments,
																	(methodType, delegateType, delegateArgument) =>
																		methodType != delegateType
																			? (Expression)Expression.Convert(delegateArgument, methodType)
																			: delegateArgument);

		// Create method call.
		MethodCallExpression methodCall = Expression.Call(instance == null ? null : Expression.Constant(instance), thisValue, convertedArguments);

		// Convert return type when necessary.
		Expression convertedMethodCall = delegateInfo.ReturnType == thisValue.ReturnType
											? methodCall
											: Expression.Convert(methodCall, delegateInfo.ReturnType);
		return Expression.Lambda<TDelegate>(convertedMethodCall, delegateArguments).Compile();
	}
}