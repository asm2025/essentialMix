using System;
using System.Linq.Expressions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Other.MarcGravell.Nullable;

// ReSharper disable once CheckNamespace
namespace Other.MarcGravell;

/// <summary>
/// Provides standard operators (such as addition) over a single type
/// </summary>
/// <seealso cref="Operator{T}" />
/// <seealso cref="Operator{TValue,TResult}" />
public static class Operator<T>
{
	static Operator()
	{
		Add = ExpressionHelper.Function<T, T, T>(Expression.Add);
		Increment = ExpressionHelper.Function<T, T>(Expression.Increment);
		Subtract = ExpressionHelper.Function<T, T, T>(Expression.Subtract);
		Decrement = ExpressionHelper.Function<T, T>(Expression.Decrement);
		Divide = ExpressionHelper.Function<T, T, T>(Expression.Divide);
		Multiply = ExpressionHelper.Function<T, T, T>(Expression.Multiply);

		GreaterThan = ExpressionHelper.Function<T, T, bool>(Expression.GreaterThan);
		GreaterThanOrEqual = ExpressionHelper.Function<T, T, bool>(Expression.GreaterThanOrEqual);
		LessThan = ExpressionHelper.Function<T, T, bool>(Expression.LessThan);
		LessThanOrEqual = ExpressionHelper.Function<T, T, bool>(Expression.LessThanOrEqual);
		Equal = ExpressionHelper.Function<T, T, bool>(Expression.Equal);
		NotEqual = ExpressionHelper.Function<T, T, bool>(Expression.NotEqual);

		Negate = ExpressionHelper.Function<T, T>(Expression.Negate);
		And = ExpressionHelper.Function<T, T, T>(Expression.And);
		AndAlso = ExpressionHelper.Function<T, T, T>(Expression.AndAlso);
		Or = ExpressionHelper.Function<T, T, T>(Expression.Or);
		Not = ExpressionHelper.Function<T, T>(Expression.Not);
		Xor = ExpressionHelper.Function<T, T, T>(Expression.ExclusiveOr);

		Type typeT = typeof(T);

		if (typeT.IsValueType && typeT.IsGenericType && typeT.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			// get the *inner* zero (not a null Nullable<TValue>, but default(TValue))
			Type nullType = typeT.GetGenericArguments()[0];
			Zero = (T)Activator.CreateInstance(nullType);
			NullOp = (INullOp<T>)Activator.CreateInstance(typeof(StructNullOp<>).MakeGenericType(nullType));
		}
		else
		{
			Zero = default(T);
			NullOp = typeT.IsValueType
						? (INullOp<T>)Activator.CreateInstance(typeof(StructNullOp<>).MakeGenericType(typeT))
						: (INullOp<T>)Activator.CreateInstance(typeof(ClassNullOp<>).MakeGenericType(typeT));
		}
	}

	/// <summary>
	/// Returns the zero value for value-types (even full Nullable&lt;TInner&gt;) - or null for reference types
	/// </summary>
	public static T Zero { get; }

	[NotNull]
	public static Func<T, T> Abs
	{
		get
		{
			return arg => LessThan(arg, default)
							? Negate(arg)
							: arg;
		}
	}

	/// <summary>
	/// Returns a delegate to evaluate unary negation (-) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T> Negate { get; }

	/// <summary>
	/// Returns a delegate to evaluate bitwise not (~) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T> Not { get; }

	/// <summary>
	/// Returns a delegate to evaluate bitwise or (|) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Or { get; }

	/// <summary>
	/// Returns a delegate to evaluate bitwise and (&amp;) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> And { get; }

	/// <summary>
	/// Returns a delegate to evaluate the second operand only if the first evaluates to true
	/// this delegate will throw an InvalidOperationException if the type T does not provide this operator, 
	/// or for Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> AndAlso { get; }

	/// <summary>
	/// Returns a delegate to evaluate bitwise xor (^) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Xor { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary addition (+) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Add { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary addition (++) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T> Increment { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary subtraction (-) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Subtract { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary subtraction (--) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T> Decrement { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary multiplication (*) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Multiply { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary division (/) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, T> Divide { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary equality (==) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> Equal { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary inequality (!=) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> NotEqual { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary greater-then (&gt;) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> GreaterThan { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary less-than (&lt;) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> LessThan { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary greater-than-or-equal (&gt;=) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> GreaterThanOrEqual { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary less-than-or-equal (&lt;=) for the given type; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<T, T, bool> LessThanOrEqual { get; }

	internal static INullOp<T> NullOp { get; }
}

/// <summary>
/// Provides standard operators (such as addition) that operate over operands of
/// different types. For operators, the return type is assumed to match the first
/// operand.
/// </summary>
/// <seealso cref="Operator{T}" />
/// <seealso cref="Operator{TValue,TResult}" />
public static class Operator<TValue, TResult>
{
	static Operator()
	{
		Convert = ExpressionHelper.Function<TValue, TResult>(body => Expression.Convert(body, typeof(TResult)));
		Add = ExpressionHelper.Function<TResult, TValue, TResult>(Expression.Add);
		Subtract = ExpressionHelper.Function<TResult, TValue, TResult>(Expression.Subtract);
		Multiply = ExpressionHelper.Function<TResult, TValue, TResult>(Expression.Multiply);
		Divide = ExpressionHelper.Function<TResult, TValue, TResult>(Expression.Divide);
	}

	/// <summary>
	/// Returns a delegate to __convert a value between two types; this delegate will throw
	/// an InvalidOperationException if the type T does not provide a suitable cast, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this cast.
	/// </summary>
	public static Func<TValue, TResult> Convert { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary addition (+) for the given types; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<TResult, TValue, TResult> Add { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary subtraction (-) for the given types; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<TResult, TValue, TResult> Subtract { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary multiplication (*) for the given types; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<TResult, TValue, TResult> Multiply { get; }

	/// <summary>
	/// Returns a delegate to evaluate binary division (/) for the given types; this delegate will throw
	/// an InvalidOperationException if the type T does not provide this operator, or for
	/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
	/// </summary>
	public static Func<TResult, TValue, TResult> Divide { get; }
}