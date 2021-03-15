using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using essentialMix.Collections;
using JetBrains.Annotations;
using essentialMix.Comparers;
using essentialMix.Delegation;
using essentialMix.Helpers;
using Other.MarcGravell;
using essentialMix.Patterns.Object;
using essentialMix.Reflection;

namespace essentialMix.Extensions
{
	public static class GenericsExtension
	{
		private const string FORMAT_STRING = "#,#0";
		private const string FORMAT_STRING_FRACTIONS = "#,#0.0#";

		private static readonly Lazy<MethodInfo> __cloneMethod = new Lazy<MethodInfo>(() => typeof(object).GetTypeInfo().GetDeclaredMethod("MemberwiseClone"), LazyThreadSafetyMode.PublicationOnly);

		private static readonly IReadOnlySet<string> __trueStrings = new ReadOnlySet<string>(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { bool.TrueString, "yes", "on" });
		private static readonly IReadOnlySet<string> __falseStrings = new ReadOnlySet<string>(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { bool.FalseString, "no", "off" });

		private static readonly IReadOnlySet<string> __disposedFieldNames = new ReadOnlySet<string>(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"disposed",
			"_disposed",
			"m_disposed",
			"mDisposed",
		});

		private static readonly ConcurrentDictionary<Type, Func<object, object>> __cachedIl = new ConcurrentDictionary<Type, Func<object, object>>();

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T As<T>(this T thisValue) { return thisValue; }

		public static T To<TSource, T>(this TSource thisValue) { return To(thisValue, default(T)); }

		public static T To<TSource, T>(this TSource thisValue, T defaultValue) { return To(thisValue, defaultValue, null); }

		public static T To<TSource, T>(this TSource thisValue, Func<string, T, T> whenFailed) { return To(thisValue, default(T), whenFailed); }

		public static T To<TSource, T>(this TSource thisValue, T defaultValue, Func<string, T, T> whenFailed) { return To(thisValue, defaultValue, null, whenFailed); }

		public static T To<TSource, T>(this TSource thisValue, T defaultValue, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			if (thisValue is T)
				return (T)(object)thisValue;
			if (thisValue.IsNull())
				return whenFailed != null ? whenFailed(null, defaultValue) : defaultValue;

			Type target = ResolveType(typeof(T)), source = AsType(thisValue, true);
			if (target.IsAssignableFrom(source))
				return (T)Convert.ChangeType(thisValue, target);

			T value;

			if (target.IsValueType && source.IsValueType)
			{
				try
				{
					value = (T)Convert.ChangeType(thisValue, target);
					return value;
				}
				catch
				{
					// ignored
				}
			}

			TypeConverter converter = TypeDescriptor.GetConverter(target);

			try
			{
				if (converter.CanConvertFrom(source))
				{
					value = (T)converter.ConvertFrom(thisValue);
					return value;
				}
			}
			catch
			{
				// ignored
			}

			string str = thisValue.AsString();
			return beforeParse != null && beforeParse(str, defaultValue, out value)
						? value
						: Parse(thisValue, defaultValue, whenFailed);
		}

		public static T To<TSource, T, TCheck>(this TSource thisValue, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
		{
			return To(thisValue, defaultValue, beforeConvert, null, whenFailed);
		}

		public static T To<TSource, T, TCheck>(this TSource thisValue, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert,
			OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			if (thisValue is T)
				return (T)(object)thisValue;
			if (thisValue.IsNull())
				return whenFailed != null ? whenFailed(null, defaultValue) : defaultValue;

			Type target = ResolveType(typeof(T)), source = AsType(thisValue, true);
			if (target.IsAssignableFrom(source))
				return (T)Convert.ChangeType(thisValue, target);

			return beforeConvert != null && thisValue is TCheck && beforeConvert((TCheck)(object)thisValue, defaultValue, out T value)
						? value
						: To(thisValue, defaultValue, beforeParse, whenFailed);
		}

		public static bool TryConvert<TSource, TTarget>(this TSource thisValue, out TTarget result)
			where TSource : IConvertible
		{
			if (thisValue is TTarget)
			{
				result = (TTarget)(object)thisValue;
				return true;
			}

			if (TryConvert(thisValue, typeof(TTarget), out object converted))
			{
				result = (TTarget)converted;
				return true;
			}

			result = default(TTarget);
			return false;
		}

		public static bool TryConvert<TSource>(this TSource thisValue, [NotNull] Type type, out object result)
			where TSource : IConvertible
		{
			if (ReferenceEquals(thisValue, null))
			{
				result = null;
				return false;
			}

			try
			{
				result = Convert.ChangeType(thisValue, type, CultureInfo.InvariantCulture);
				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		public static T Parse<TSource, T>(this TSource thisValue) { return Parse(thisValue, default(T)); }

		public static T Parse<TSource, T>(this TSource thisValue, Func<string, T, T> whenFailed) { return Parse(thisValue, default(T), whenFailed); }

		public static T Parse<TSource, T>(this TSource thisValue, T defaultValue) { return Parse(thisValue, defaultValue, null); }

		public static T Parse<TSource, T>(this TSource thisValue, T defaultValue, Func<string, T, T> whenFailed)
		{
			if (thisValue is T)
				return (T)(object)thisValue;
			if (thisValue.IsNull())
				return whenFailed != null ? whenFailed(null, defaultValue) : defaultValue;

			Type target = ResolveType(typeof(T)), source = AsType(thisValue, true);
			if (target.IsAssignableFrom(source))
				return (T)Convert.ChangeType(thisValue, target);

			string str = thisValue.AsString();
			if (string.IsNullOrEmpty(str))
				return defaultValue;

			MethodInfo method = target.GetMethod("Parse", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
			if (method == null || method.ReturnType != target && method.ReturnType != typeof(T))
				return defaultValue;

			T value = defaultValue;
			bool converted = false;

			try
			{
				value = (T)method.Invoke(null, BindingFlags.InvokeMethod, null, new object[] { str }, CultureInfoHelper.Default);
				converted = true;
			}
			catch
			{
				// ignored
			}

			if (!converted && whenFailed != null)
				value = whenFailed(str, defaultValue);
			return value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static T[] GetRange<T>([NotNull] this T[] thisValue, int startIndex, int count)
		{
			return ArrayHelper.ValidateAndGetRange(thisValue, ref startIndex, ref count);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Exists<T>([NotNull] this T[] thisValue, [NotNull] Predicate<T> predicate) { return Array.Exists(thisValue, predicate); }

		public static bool Contains<T>([NotNull] this T[] thisValue, T[] values)
		{
			if (thisValue.Rank != 1)
				throw new RankException();
			if (values == null)
				return false;
			if (values.Rank != 1)
				throw new RankException();
			if (thisValue.Length == 0 && values.Length == 0)
				return true;
			return values.Length <= thisValue.Length && values.All(e => thisValue.FastContains(e, 0, thisValue.Length));
		}

		public static bool Contains<T>([NotNull] this T[] thisValue, T[] values, [NotNull] IEqualityComparer<T> comparer)
		{
			if (thisValue.Rank != 1)
				throw new RankException();
			if (values == null)
				return false;
			if (values.Rank != 1)
				throw new RankException();
			if (thisValue.Length == 0 && values.Length == 0)
				return true;
			return values.Length <= thisValue.Length && values.All(v => thisValue.Contains(v, comparer));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool FastContains<T>([NotNull] this T[] thisValue, T value)
		{
			return FastContains(thisValue, value, 0, thisValue.Length);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool FastContains<T>([NotNull] this T[] thisValue, T value, int startIndex)
		{
			return FastContains(thisValue, value, startIndex, thisValue.Length - startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool FastContains<T>([NotNull] this T[] thisValue, T value, int startIndex, int count)
		{
			return Array.IndexOf(thisValue, value, startIndex, count) > -1;
		}

		[NotNull]
		public static T[] Duplicate<T>([NotNull] this T[] thisValue)
		{
			if (thisValue.Length == 0)
				return Array.Empty<T>();

			T[] newValues = new T[thisValue.Length];
			Array.Copy(thisValue, 0, newValues, 0, newValues.Length);
			return newValues;
		}

		[NotNull]
		public static T[] Prepend<T>([NotNull] this T[] thisValue, [NotNull] params T[] items)
		{
			if (thisValue.Length == 0)
				return items;
			if (items.Length == 0)
				return thisValue;

			int n = items.Length;
			Array.Resize(ref items, thisValue.Length + items.Length);
			Array.Copy(thisValue, 0, items, n, thisValue.Length);
			return items;
		}

		[NotNull]
		public static T[] Append<T>([NotNull] this T[] thisValue, [NotNull] params T[] value)
		{
			if (value.Length == 0)
				return thisValue;

			int n = thisValue.Length;
			Array.Resize(ref thisValue, thisValue.Length + value.Length);
			Array.Copy(value, 0, thisValue, n, value.Length);
			return thisValue;
		}

		[NotNull]
		public static T[] Insert<T>([NotNull] this T[] thisValue, int index, [NotNull] params T[] value)
		{
			if (!index.InRange(0, thisValue.Length))
				throw new ArgumentOutOfRangeException(nameof(index));
			if (value.Length == 0)
				return thisValue;

			int n;
			int tvl = thisValue.Length;
			Array.Resize(ref thisValue, thisValue.Length + value.Length);

			if (index == thisValue.Length)
			{
				n = thisValue.Length;
			}
			else if (index == 0)
			{
				n = 0;
				Array.Copy(thisValue, 0, thisValue, value.Length, tvl);
			}
			else
			{
				n = index;
				Array.Copy(thisValue, n, thisValue, n + value.Length, tvl - n);
			}

			Array.Copy(value, 0, thisValue, n, value.Length);
			return thisValue;
		}

		[NotNull]
		public static T[] Remove<T>([NotNull] this T[] thisValue, int startIndex, int length)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length))
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > thisValue.Length)
				throw new ArgumentOutOfRangeException(nameof(length));
			if (length == 0)
				return thisValue;
			if (length == thisValue.Length)
				return Array.Empty<T>();

			T[] newValues = new T[thisValue.Length - length];
			if (startIndex > 0)
				Array.Copy(thisValue, 0, newValues, 0, startIndex);

			int limit = startIndex + length;
			int remaining = thisValue.Length - limit;
			if (remaining > 0)
				Array.Copy(thisValue, limit, newValues, startIndex, remaining);
			return newValues;
		}

		public static byte[] SerializeBinary<T>(this T thisValue)
		{
			if (ReferenceEquals(thisValue, null))
				return null;

			using (MemoryStream stream = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, thisValue);
				return stream.ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool SerializeBinary<T>([NotNull] this T thisValue, [NotNull] Stream stream, StreamingContextStates contextStates = StreamingContextStates.Persistence, ISurrogateSelector selector = null)
		{
			return stream.SerializeBinary(thisValue, contextStates, selector);
		}

		[NotNull]
		public static byte[] SerializeRaw<T>(this T thisValue)
			where T : struct
		{
			int rawSize = Marshal.SizeOf(thisValue);
			IntPtr buffer = Marshal.AllocHGlobal(rawSize);
			Marshal.StructureToPtr(thisValue, buffer, false);
			byte[] rawData = new byte[rawSize];
			Marshal.Copy(buffer, rawData, 0, rawSize);
			Marshal.FreeHGlobal(buffer);
			return rawData;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsValueType<T>([NotNull] this T thisValue) { return ResolveType(thisValue).IsValueType; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsClass<T>([NotNull] this T thisValue) { return ResolveType(thisValue).IsClass; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsIntegral<T>([NotNull] this T thisValue) { return ResolveType(thisValue).IsIntegral(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsFloater<T>([NotNull] this T thisValue) { return ResolveType(thisValue).IsFloater(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNumeric<T>([NotNull] this T thisValue, bool parse = false)
		{
			return ResolveType(thisValue).IsNumeric() || parse && ToNumber(thisValue, (double?)null) != null;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TResult NullSafe<TSource, TResult>(this TSource thisValue, [NotNull] Func<TSource, TResult> onNotNull, TResult defaultValue = default(TResult))
		{
			return ReferenceEquals(thisValue, null)
						? defaultValue
						: onNotNull(thisValue);
		}

		public static T ToNumber<TSource, T>([NotNull] this TSource thisValue, T defaultValue = default(T))
		{
			if (!thisValue.AsType().IsPrimitive()) return defaultValue;
			Type target = ResolveType(typeof(T));
			return !target.IsValueType ? defaultValue : To(thisValue, defaultValue, null, null);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Type ResolveType<T>([NotNull] this T thisValue) { return thisValue.AsType().ResolveType(); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static Type AsType<T>(this T thisValue) { return AsType(thisValue, false); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static Type AsType<T>(this T thisValue, bool resolveGenerics)
		{
			return (resolveGenerics
						? thisValue.AsType().ResolveType()
						: thisValue as Type ?? thisValue?.GetType()) ?? typeof(T);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TypeCode AsTypeCode<T>([NotNull] this T thisValue) { return AsTypeCode(thisValue, false); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TypeCode AsTypeCode<T>(this T thisValue, bool resolveGenerics)
		{
			TypeCode? typeCode = thisValue as TypeCode?;
			return typeCode ?? thisValue.AsType(resolveGenerics).AsTypeCode(resolveGenerics);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static string AsString<T>([NotNull] this T thisValue) { return thisValue as string ?? Convert.ToString(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T AsInterface<TSource, T>([NotNull] this TSource thisValue) where T : class
		{
			if (!typeof(T).IsInterface)
				throw new InvalidCastException("T is not an interface type.");
			return thisValue as T;
		}

		public static bool ToBoolean<T>([NotNull] this T thisValue, bool defaultValue = default(bool))
			where T : IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return thisValue.To(defaultValue,
				(string s, bool d, out bool r) =>
				{
					r = __trueStrings.Contains(s) || __falseStrings.Contains(s) || s.IsDigits();
					return r;
				},
				(string s, bool d, out bool r) =>
				{
					r = __trueStrings.Contains(s) || __falseStrings.Contains(s) || s.IsDigits();
					return r;
				},
				(s, d) => __trueStrings.Contains(s) || s.ToNumber(0L) != 0L || d);
		}

		/// <summary>
		/// Indicates if the supplied value is non-null,
		/// for reference-types or Nullable&lt;T&gt;
		/// </summary>
		/// <returns>True for non-null values, else false</returns>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool HasValue<T>(this T thisValue) { return typeof(T).IsValueType || !ReferenceEquals(thisValue, null); }

		/// <summary>
		/// Evaluates unary negation (-) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Negate<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Negate(thisValue);
		}

		/// <summary>
		/// Evaluates bitwise not (~) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Not<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return Operator<T>.Not(thisValue);
		}

		/// <summary>
		/// Evaluates bitwise or (|) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Or<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return Operator<T>.Or(thisValue, value);
		}

		/// <summary>
		/// Evaluates bitwise and (&amp;) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T And<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return Operator<T>.And(thisValue, value);
		}

		/// <summary>
		/// Returns a delegate to evaluate the second operand only if the first evaluates to true
		/// this delegate will throw an InvalidOperationException if the type T does not provide this operator, 
		/// or for Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T AndAlso<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return Operator<T>.AndAlso(thisValue, value);
		}

		/// <summary>
		/// Evaluates bitwise xor (^) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Xor<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			return Operator<T>.Xor(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary addition (++) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Increment<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Increment(thisValue);
		}

		/// <summary>
		/// Evaluates binary subtraction (--) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Decrement<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Decrement(thisValue);
		}

		/// <summary>
		/// Evaluates binary addition (+) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Add<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Add(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary addition (+) for the given type(s); this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T AddAlt<T, TArg2>(this T thisValue, TArg2 value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
			where TArg2 : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<TArg2, T>.Add(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary subtraction (-) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Subtract<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Subtract(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary subtraction(-) for the given type(s); this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T SubtractAlt<T, TArg2>(this T thisValue, TArg2 value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
			where TArg2 : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<TArg2, T>.Subtract(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary multiplication (*) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Multiply<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Multiply(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary multiplication (*) for the given type(s); this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T MultiplyAlt<T, TArg2>(this T thisValue, TArg2 value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<TArg2, T>.Multiply(thisValue, value);
		}

		/// <summary>
		/// Evaluates binary division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Divide<T>(this T thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Divide(thisValue, value);
		}

		/// <summary>
		/// Evaluates integer division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		/// <remarks>
		/// This operation is particularly useful for computing averages and
		/// similar aggregates.
		/// </remarks>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideInt32<T>(this T thisValue, int divisor)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<int, T>.Divide(thisValue, divisor);
		}

		/// <summary>
		/// Evaluates integer division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		/// <remarks>
		/// This operation is particularly useful for computing averages and
		/// similar aggregates.
		/// </remarks>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideInt64<T>(this T thisValue, long divisor)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<long, T>.Divide(thisValue, divisor);
		}

		/// <summary>
		/// Evaluates integer division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		/// <remarks>
		/// This operation is particularly useful for computing averages and
		/// similar aggregates.
		/// </remarks>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideFlt<T>(this T thisValue, float divisor)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<float, T>.Divide(thisValue, divisor);
		}

		/// <summary>
		/// Evaluates integer division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		/// <remarks>
		/// This operation is particularly useful for computing averages and
		/// similar aggregates.
		/// </remarks>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideDbl<T>(this T thisValue, double divisor)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<double, T>.Divide(thisValue, divisor);
		}

		/// <summary>
		/// Evaluates integer division (/) for the given type; this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		/// <remarks>
		/// This operation is particularly useful for computing averages and
		/// similar aggregates.
		/// </remarks>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideDec<T>(this T thisValue, decimal divisor)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<decimal, T>.Divide(thisValue, divisor);
		}

		/// <summary>
		/// Evaluates binary division (/) for the given type(s); this will throw
		/// an InvalidOperationException if the type T does not provide this operator, or for
		/// Nullable&lt;TInner&gt; if TInner does not provide this operator.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T DivideAlt<T, TArg2>(this T thisValue, TArg2 value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<TArg2, T>.Divide(thisValue, value);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Abs<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Operator<T>.Abs(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T AbsObj<T>(this T thisValue)
			where T : IComparable
		{
			return Operator<T>.Abs(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsPositive<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Abs(thisValue).CompareTo(thisValue) == 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNegative<T>(this T thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Abs(thisValue).CompareTo(thisValue) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsPositiveObj<T>(this T thisValue)
			where T : IComparable
		{
			return !ReferenceEquals(thisValue, null) && AbsObj(thisValue).CompareTo(thisValue) == 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNegativeObj<T>(this T thisValue)
			where T : IComparable
		{
			return !ReferenceEquals(thisValue, null) && AbsObj(thisValue).CompareTo(thisValue) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InDelta<T>(this T thisValue, T value, T delta)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return IsLessThanOrEqual(Abs(Subtract(thisValue, value)), delta);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool In<T>(this T thisValue, [NotNull] params T[] list)
			where T : struct, IComparable
		{
			return list.Contains(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) == 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsLessThan<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsLessThanOrEqual<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsGreaterThan<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) > 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsGreaterThanOrEqual<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) >= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRange<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(minimum) >= 0 && thisValue.CompareTo(maximum) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeEx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(minimum) > 0 && thisValue.CompareTo(maximum) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeLx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(minimum) > 0 && thisValue.CompareTo(maximum) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeRx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(minimum) >= 0 && thisValue.CompareTo(maximum) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Within<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable
		{
			if (minimum.CompareTo(maximum) > 0) throw new InvalidOperationException($"{nameof(minimum)} cannot be greater than {nameof(maximum)}.");
			return thisValue.CompareTo(minimum) < 0
						? minimum
						: thisValue.CompareTo(maximum) > 0
							? maximum
							: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinEx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Within(thisValue, minimum.Increment(), maximum.Decrement());
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinLx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Within(thisValue, minimum.Increment(), maximum);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinRx<T>(this T thisValue, T minimum, T maximum)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return Within(thisValue, minimum, maximum.Decrement());
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T NotBelow<T>(this T thisValue, T minimum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(minimum) < 0
						? minimum
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T NotAbove<T>(this T thisValue, T maximum)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(maximum) > 0
						? maximum
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Minimum<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) < 0
						? thisValue
						: other;
		}

		public static T Minimum<T>(this T thisValue, [NotNull] params T[] values)
			where T : struct, IComparable
		{
			if (values.Length == 0) return thisValue;

			T v = thisValue;

			foreach (T m in values)
			{
				// skip of v <= m
				if (v.CompareTo(m) <= 0) continue;
				v = m;
			}

			return v;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Maximum<T>(this T thisValue, T other)
			where T : struct, IComparable
		{
			return thisValue.CompareTo(other) > 0
						? thisValue
						: other;
		}

		public static T Maximum<T>(this T thisValue, [NotNull] params T[] values)
			where T : struct, IComparable
		{
			if (values.Length == 0) return thisValue;

			T v = thisValue;

			foreach (T m in values)
			{
				// skip of v >= m
				if (v.CompareTo(m) >= 0) continue;
				v = m;
			}

			return v;
		}

		public static T MinimumNotBelow<T>(this T thisValue, T bound, [NotNull] params T[] values)
			where T : struct, IComparable
		{
			T v = NotBelow(thisValue, bound);
			if (values.IsNullOrEmpty() || v.CompareTo(bound) == 0) return v;

			foreach (T m in values)
			{
				// skip if v <= m
				if (v.CompareTo(m) <= 0) continue;
				v = m;
				// stop if at bound
				if (v.CompareTo(bound) == 0) break;
			}

			return v;
		}

		public static T MaximumNotAbove<T>(this T thisValue, T bound, [NotNull] params T[] values)
			where T : struct, IComparable
		{
			T v = NotBelow(thisValue, bound);
			if (values.IsNullOrEmpty() || v.CompareTo(bound) == 0) return v;

			foreach (T m in values)
			{
				// skip if v >= m
				if (v.CompareTo(m) >= 0) continue;
				v = m;
				// stop if at bound
				if (v.CompareTo(bound) == 0) break;
			}

			return v;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNull<T>(this T thisValue) { return ReferenceEquals(thisValue, null) || thisValue is DBNull; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNull<T>(this T? thisValue)
			where T : struct, IComparable
		{
			return thisValue == null || IsNull(thisValue.Value);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNull<T>(this T thisValue, T trueResponse) { return IfNull(thisValue, trueResponse, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNull<T>(this T thisValue, T trueResponse, T falseResponse)
		{
			return IsNull(thisValue)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNull<T>(this T thisValue, Func<T> func)
		{
			return IsNull(thisValue)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotNull<T>(this T thisValue, T trueResponse) { return IfNotNull(thisValue, trueResponse, thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotNull<T>(this T thisValue, T trueResponse, T falseResponse)
		{
			return !IsNull(thisValue)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotNull<T>(this T thisValue, Func<T> func)
		{
			return !IsNull(thisValue)
						? func()
						: default(T);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfEqual<T>(this T thisValue, T value, T trueResponse)
			where T : struct, IComparable
		{
			return IfEqual(thisValue, value, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfEqual<TSource, T>(this TSource thisValue, TSource value, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return IsEqual(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfEqual<T>(this T thisValue, T value, Func<T> func)
			where T : struct, IComparable
		{
			return IsEqual(thisValue, value)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotEqual<T>(this T thisValue, T value, T trueResponse)
			where T : struct, IComparable
		{
			return IfNotEqual(thisValue, value, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotEqual<TSource, T>(this TSource thisValue, TSource value, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return !IsEqual(thisValue, value)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfNotEqual<T>(this T thisValue, T value, Func<T> func)
			where T : struct, IComparable
		{
			return !IsEqual(thisValue, value)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThan<T>(this T thisValue, T maximum, T trueResponse)
			where T : struct, IComparable
		{
			return IfGreaterThan(thisValue, maximum, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThan<TSource, T>(this TSource thisValue, TSource maximum, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return IsGreaterThan(thisValue, maximum)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThan<T>(this T thisValue, T maximum, Func<T> func)
			where T : struct, IComparable
		{
			return IsGreaterThan(thisValue, maximum)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThanOrEqual<T>(this T thisValue, T maximum, T trueResponse)
			where T : struct, IComparable
		{
			return IfGreaterThanOrEqual(thisValue, maximum, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThanOrEqual<TSource, T>(this TSource thisValue, TSource maximum, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return IsGreaterThanOrEqual(thisValue, maximum)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfGreaterThanOrEqual<T>(this T thisValue, T maximum, Func<T> func)
			where T : struct, IComparable
		{
			return IsGreaterThanOrEqual(thisValue, maximum)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThan<T>(this T thisValue, T minimum, T trueResponse)
			where T : struct, IComparable
		{
			return IfLessThan(thisValue, minimum, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThan<TSource, T>(this TSource thisValue, TSource minimum, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return IsLessThan(thisValue, minimum)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThan<T>(this T thisValue, T minimum, Func<T> func)
			where T : struct, IComparable
		{
			return IsLessThan(thisValue, minimum)
						? func()
						: thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThanOrEqual<T>(this T thisValue, T minimum, T trueResponse)
			where T : struct, IComparable
		{
			return IfLessThanOrEqual(thisValue, minimum, trueResponse, thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThanOrEqual<TSource, T>(this TSource thisValue, TSource minimum, T trueResponse, T falseResponse)
			where TSource : struct, IComparable
		{
			return IsLessThanOrEqual(thisValue, minimum)
						? trueResponse
						: falseResponse;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T IfLessThanOrEqual<T>(this T thisValue, T minimum, Func<T> func)
			where T : struct, IComparable
		{
			return IsLessThanOrEqual(thisValue, minimum)
						? func()
						: thisValue;
		}

		[NotNull]
		public static string PadLeft<T>(this T thisValue, int totalWidth, char paddingChar = ' ')
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// T : struct, thisValue cannot be null
			return thisValue.ToString().PadLeft(totalWidth, paddingChar);
		}

		[NotNull]
		public static string PadRight<T>(this T thisValue, int totalWidth, char paddingChar = ' ')
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// T : struct, thisValue cannot be null
			return thisValue.ToString().PadRight(totalWidth, paddingChar);
		}

		[NotNull]
		public static string Format<T>(this T thisValue, bool bFractions = true, string suffix = null)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return string.Format(GetFormat(bFractions, suffix), thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>(this T[] thisValue) { return thisValue == null || thisValue.Length == 0; }

		public static bool IsNullOrUninitialized<T>(this T[] thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return IsNullOrUninitialized(thisValue, 0, -1);
		}

		public static bool IsNullOrUninitialized<T>(this T[] thisValue, int arrayIndex)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			return IsNullOrUninitialized(thisValue, arrayIndex, -1);
		}

		public static bool IsNullOrUninitialized<T>(this T[] thisValue, int arrayIndex, int count)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (thisValue == null || thisValue.Length == 0)
				return true;
			thisValue.Length.ValidateRange(arrayIndex, ref count);

			T def = default(T);
			int last = arrayIndex + count;

			for (int i = arrayIndex; i < last; i++)
			{
				if (!thisValue[i].Equals(def))
					return false;
			}

			return true;
		}

		public static void FastInitialize<T>([NotNull] this T[] thisValue, T value)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			FastInitialize(thisValue, value, 0, thisValue.Length);
		}

		public static void FastInitialize<T>([NotNull] this T[] thisValue, T value, int startIndex)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			FastInitialize(thisValue, value, startIndex, thisValue.Length);
		}

		public static void FastInitialize<T>([NotNull] this T[] thisValue, T value, int startIndex, int count)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible
		{
			const int BLOCK_SIZE = 32;

			/*
			 * The basic idea came from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c/54278956#54278956
			 * but the original code never worked! Nevertheless, the idea itself is great. It gives a very fast pace.
			 */
			int lo = startIndex, hi = Math.Min(BLOCK_SIZE, count);

			while (lo < hi) 
				thisValue[lo++] = value;

			if (lo == startIndex + count) return;

			int itemSize;
	
			switch (typeof(T).AsTypeCode())
			{
				case TypeCode.Boolean:
					itemSize = sizeof(bool);
					break;
				case TypeCode.Byte:
				case TypeCode.SByte:
					itemSize = sizeof(byte);
					break;
				case TypeCode.Char:
					itemSize = sizeof(char);
					break;
				case TypeCode.Int16:
				case TypeCode.UInt16:
					itemSize = sizeof(short);
					break;
				case TypeCode.Int32:
				case TypeCode.UInt32:
					itemSize = sizeof(int);
					break;
				case TypeCode.Int64:
				case TypeCode.UInt64:
					itemSize = sizeof(long);
					break;
				case TypeCode.Single:
					itemSize = sizeof(float);
					break;
				case TypeCode.Double:
					itemSize = sizeof(double);
					break;
				case TypeCode.Decimal:
					itemSize = sizeof(decimal);
					break;
				default:
					throw new NotSupportedException();
			}

			int block = BLOCK_SIZE;
			// convert everything to byte size
			startIndex *= itemSize;
			lo *= itemSize;
			hi = count * itemSize;
	
			for (; lo < hi; lo += block, block = Math.Min(block * 2, hi - lo))
				Buffer.BlockCopy(thisValue, startIndex, thisValue, lo, block);
		}

		public static void Initialize<T>([NotNull] this T[] thisValue, T value, int startIndex = 0, int count = -1)
			where T : struct, IComparable, IComparable<T>
		{
			if (thisValue.Length == 0) return;
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length == 0) return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = value;
		}

		public static void Initialize<T>([NotNull] this T[] thisValue, [NotNull] Func<int, T> func, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = func(i);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int IndexOf<T>([NotNull] this T[] thisValue, T value)
		{
			return IndexOf(thisValue, value, 0, thisValue.Length);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int IndexOf<T>([NotNull] this T[] thisValue, T value, int startIndex)
		{
			return IndexOf(thisValue, value, startIndex, thisValue.Length - startIndex);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int IndexOf<T>([NotNull] this T[] thisValue, T value, int startIndex, int count)
		{
			return Array.IndexOf(thisValue, value, startIndex, count);
		}

		public static int IndexOf<T>([NotNull] this T[] thisValue, [NotNull] Predicate<T> comparison, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length == 0)
				return -1;

			int x = -1;
			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
			{
				if (!comparison(thisValue[i])) continue;
				x = i;
				break;
			}

			return x;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Is<T, TType>(this T thisValue) { return Is(thisValue, typeof(TType)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Is<T>(this T thisValue, [NotNull] Type type) { return !ReferenceEquals(thisValue, null) && thisValue.AsType(true).Is(type); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAbstractOf<T, TType>([NotNull] this T thisValue) { return IsAbstractOf(thisValue, typeof(TType)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAbstractOf<T>(this T thisValue, Type type) { return !ReferenceEquals(thisValue, null) && thisValue.AsType(true).IsAbstractOf(type); }

		public static bool IsConcreteOf<T, TType>([NotNull] this T thisValue) { return IsConcreteOf(thisValue, typeof(TType)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsConcreteOf<T>(this T thisValue, Type type) { return !ReferenceEquals(thisValue, null) && thisValue.AsType(true).IsConcreteOf(type); }

		[NotNull]
		public static Type[] Types<T>(this T[] thisValue)
		{
			if (thisValue == null)
				return Type.EmptyTypes;

			Type[] types = new Type[thisValue.Length];
			thisValue.ForEach((o, i) => types[i] = o?.AsType() ?? typeof(object));
			return types;
		}

		public static void ForEach<T>([NotNull] this T[] thisValue, [NotNull] Action<T> action)
		{
			if (thisValue.Length == 0)
				return;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static void ForEach<T>([NotNull] this T[] thisValue, [NotNull] Action<T, int> action)
		{
			if (thisValue.Length == 0)
				return;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current, enumerator.Position);
			}
		}

		public static void ForEach<T>([NotNull] this T[] thisValue, [NotNull] Func<T, bool> action)
		{
			if (thisValue.Length == 0)
				return;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current))
						break;
				}
			}
		}

		public static void ForEach<T>([NotNull] this T[] thisValue, [NotNull] Func<T, int, bool> action)
		{
			if (thisValue.Length == 0)
				return;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current, enumerator.Position))
						break;
				}
			}
		}

		public static bool All<T>([NotNull] this T[] thisValue, [NotNull] Func<T, bool> predicate)
		{
			if (thisValue.Length == 0)
				return false;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!predicate(enumerator.Current))
						return false;
				}
			}

			return true;
		}

		public static bool All<T>([NotNull] this T[] thisValue, [NotNull] Func<T, int, bool> predicate)
		{
			if (thisValue.Length == 0)
				return false;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!predicate(enumerator.Current, enumerator.Position))
						return false;
				}
			}

			return true;
		}

		public static bool Any<T>([NotNull] this T[] thisValue, [NotNull] Func<T, bool> predicate)
		{
			if (thisValue.Length == 0)
				return false;

			using (Enumerator<T> enumerator = new Enumerator<T>(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (predicate(enumerator.Current))
						return true;
				}
			}

			return false;
		}

		public static bool IsDisposed<T>(this T thisValue, string fieldName = null)
			where T : class
		{
			switch (thisValue)
			{
				case null:
					return true;
				case Disposable disposableBase:
					return disposableBase.GetPropertyValue("IsDisposedOrDisposing", out bool propDisposed, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE) && propDisposed;
				default:
					if (!(thisValue is IDisposable disposable))
						return false;
					fieldName = fieldName?.Trim();

					Type type = thisValue.AsType();
					Type rtType = typeof(bool);
					FieldInfo fieldInfo = null;

					if (!string.IsNullOrEmpty(fieldName))
					{
						fieldInfo = type.GetField(fieldName, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, rtType);
					}

					if (fieldInfo != null) return (bool)fieldInfo.GetValue(disposable);

					foreach (string fn in __disposedFieldNames)
					{
						fieldInfo = type.GetField(fn, Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, rtType);
						if (fieldInfo != null)
							break;
					}

					return fieldInfo != null && (bool)fieldInfo.GetValue(disposable);
			}
		}

		public static object ToUnsigned<T>(this T thisValue)
			where T : IComparable
		{
			switch (thisValue.AsTypeCode(true))
			{
				case TypeCode.SByte:
					return (byte)Convert.ToSByte(thisValue);
				case TypeCode.Int16:
					return (ushort)Convert.ToInt16(thisValue);
				case TypeCode.Int32:
					return (uint)Convert.ToInt32(thisValue);
				case TypeCode.Int64:
					return (ulong)Convert.ToInt64(thisValue);
				case TypeCode.Single:
					return (uint)Convert.ToSingle(thisValue);
				case TypeCode.Double:
					return (ulong)Convert.ToDouble(thisValue);
				case TypeCode.Decimal:
					return (ulong)Convert.ToDecimal(thisValue);
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return thisValue;
				default:
					throw new InvalidOperationException();
			}
		}

		public static long UnboxToLong<T>(this T thisValue, bool round)
			where T : IComparable
		{
			switch (thisValue.AsTypeCode(true))
			{
				case TypeCode.Boolean:
					return Convert.ToBoolean(thisValue) ? 1 : 0;
				case TypeCode.Char:
					return Convert.ToChar(thisValue);
				case TypeCode.SByte:
					return Convert.ToSByte(thisValue);
				case TypeCode.Byte:
					return Convert.ToByte(thisValue);
				case TypeCode.Int16:
					return Convert.ToInt16(thisValue);
				case TypeCode.UInt16:
					return Convert.ToUInt16(thisValue);
				case TypeCode.Int32:
					return Convert.ToInt32(thisValue);
				case TypeCode.UInt32:
					return Convert.ToUInt32(thisValue);
				case TypeCode.Int64:
					return Convert.ToInt64(thisValue);
				case TypeCode.UInt64:
					return (long)Convert.ToUInt64(thisValue);
				case TypeCode.Single:
					return (long)(round ? Math.Round(Convert.ToSingle(thisValue)) : Convert.ToSingle(thisValue));
				case TypeCode.Double:
					return (long)(round ? Math.Round(Convert.ToDouble(thisValue)) : Convert.ToDouble(thisValue));
				case TypeCode.Decimal:
					return (long)(round ? Math.Round(Convert.ToDecimal(thisValue)) : Convert.ToDecimal(thisValue));
				default:
					return 0;
			}
		}

		public static T CloneSerializable<T>([NotNull] this T thisValue)
		{
			if (!typeof(T).IsSerializable)
				throw new ArgumentException("Type must be serializable.", nameof(thisValue));

			IFormatter formatter = new BinaryFormatter();

			using (Stream stream = new MemoryStream())
			{
				formatter.Serialize(stream, thisValue);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}

		[NotNull]
		public static string ToRangeString<T>(this T thisValue, T maximum, IComparer<T> comparer = null)
			where T : struct, IComparable
		{
			comparer ??= Comparer<T>.Default;

			switch (comparer.Compare(thisValue, maximum))
			{
				case 0:
					return Convert.ToString(thisValue);
				default:
					return $"{thisValue}-{maximum}";
			}
		}

		public static string AssemblyPath<T>([NotNull] this T thisValue) { return thisValue.AsType(true).AssemblyPath(); }

		[NotNull]
		public static NameValueCollection ToNameValueCollection<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			NameValueCollection rootCollection = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
			if (thisValue.IsNull())
				return rootCollection;
			EnumerateProperties(thisValue, type, (source, key, property) =>
			{
				if (property.PropertyType.IsPrimitive())
				{
					object value = property.GetValue(source);
					rootCollection.Add(key, Convert.ToString(value));
				}
				else if (property.PropertyType.IsArray)
				{
					object value = property.GetValue(source);
					rootCollection.Add(key, value is Array array
																? string.Join(", ", array)
																: string.Empty);
				}

				return true;
			}, (source, key, field) =>
				{
					if (field.FieldType.IsPrimitive())
					{
						object value = field.GetValue(source);
						rootCollection.Add(key, Convert.ToString(value));
					}
					else if (field.FieldType.IsArray)
					{
						object value = field.GetValue(source);
						rootCollection.Add(key, value is Array array
													? string.Join(", ", array)
													: string.Empty);
					}

					return true;
				});
			return rootCollection;
		}

		[NotNull]
		public static IDictionary<string, object> ToDictionary<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			IDictionary<string, object> rootDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (thisValue.IsNull())
				return rootDictionary;
			EnumerateProperties(thisValue
								, type
								, (source, key, property) =>
								{
									if (property.PropertyType.IsPrimitive())
									{
										object value = property.GetValue(source);
										rootDictionary.Add(key, value);
									}
									else if (property.PropertyType.IsArray)
									{
										object value = property.GetValue(source);
										rootDictionary.Add(key, value);
									}

									return true;
								}
								, (source, key, field) =>
								{
									object value = field.GetValue(source);
									rootDictionary.Add(key, value);
									return true;
								});
			return rootDictionary;
		}

		[NotNull]
		public static IReadOnlyDictionary<string, Type> ToSchema<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			Dictionary<string, Type> rootDictionary = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
			if (thisValue.IsNull())
				return rootDictionary;
			EnumerateProperties(thisValue
								, type
								, (source, key, property) =>
								{
									rootDictionary.Add(key, property.PropertyType);
									return true;
								}
								, (source, key, field) =>
								{
									rootDictionary.Add(key, field.FieldType);
									return true;
								});
			return new ReadOnlyDictionary<string, Type>(rootDictionary);
		}

		[NotNull]
		public static IReadOnlyDictionary<string, PropertyInfo> ToSchemaProperties<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			Dictionary<string, PropertyInfo> rootDictionary = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
			if (thisValue.IsNull())
				return rootDictionary;
			EnumerateProperties(thisValue, type, (source, key, property) =>
					{
						rootDictionary.Add(key, property);
						return true;
					}, null);
			return new ReadOnlyDictionary<string, PropertyInfo>(rootDictionary);
		}

		[NotNull]
		public static IReadOnlyDictionary<string, MemberInfo> ToSchemaInfo<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			Dictionary<string, MemberInfo> rootDictionary = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);
			if (thisValue.IsNull())
				return rootDictionary;
			EnumerateProperties(thisValue
								, type
								, (source, key, property) =>
								{
									rootDictionary.Add(key, property);
									return true;
								}
								, (source, key, field) =>
								{
									rootDictionary.Add(key, field);
									return true;
								});
			return new ReadOnlyDictionary<string, MemberInfo>(rootDictionary);
		}

		[NotNull]
		public static IReadOnlyCollection<(string Key, object Source, MemberInfo Member, bool ShouldSerialize)> GetNameValueMembers<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			List<(string Key, object Source, MemberInfo Member, bool ShouldSerialize)> list = new List<(string Key, object Source, MemberInfo Member, bool ShouldSerialize)>();
			EnumeratePropertyDescriptors(thisValue, type, (source, key, property) =>
				{
					if (property.PropertyType.IsPrimitive())
						list.Add((key, source, property.ComponentType.GetProperty(property.Name), false));
					else if (property.PropertyType.IsSerializable && property.ShouldSerializeValue(source))
						list.Add((key, source, property.ComponentType.GetProperty(property.Name), true));
					else
						list.Add((key, source, property.ComponentType.GetProperty(property.Name), false));

					return true;
				}, (source, key, field) =>
					{
						if (field.FieldType.IsPrimitive())
							list.Add((key, source, field, false));
						else if (field.FieldType.IsSerializable)
							list.Add((key, source, field, true));
						else
							list.Add((key, source, field, false));

						return true;
					});
			return list.AsReadOnly();
		}

		[NotNull]
		public static IEnumerable<PropertyDescriptor> GetNameValuePropertyDescriptors<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			if (thisValue.IsNull())
				return Enumerable.Empty<PropertyDescriptor>();

			IEnumerable<PropertyDescriptor> enumerable = TypeDescriptor.GetProperties(thisValue)
																		.Cast<PropertyDescriptor>()
																		.Where(e => e.IsBrowsable && !e.DesignTimeOnly);
			return type == PropertyInfoType.Default
						? enumerable
						: enumerable.Where(e =>
			{
				PropertyInfo property = e.ComponentType.GetProperty(e.Name, Constants.BF_PUBLIC_INSTANCE);
				return property != null &&
						(!type.HasFlag(PropertyInfoType.Get) || property.CanRead) &&
						(!type.HasFlag(PropertyInfoType.Set) || property.CanWrite);
			});
		}

		[NotNull]
		public static IEnumerable<PropertyInfo> GetNameValuePropertyInfo<T>(this T thisValue, PropertyInfoType type = PropertyInfoType.All)
		{
			if (thisValue.IsNull())
				return Enumerable.Empty<PropertyInfo>();

			IEnumerable<PropertyInfo> enumerable = GetNameValuePropertyDescriptors(thisValue, PropertyInfoType.Default)
														.Select(e => e.ComponentType.GetProperty(e.Name, Constants.BF_PUBLIC_INSTANCE))
														.Where(e => e != null);
			return type == PropertyInfoType.Default
						? enumerable
						: enumerable.Where(e => (!type.HasFlag(PropertyInfoType.Get) || e.CanRead) &&
												(!type.HasFlag(PropertyInfoType.Set) || e.CanWrite));
		}

		[NotNull]
		public static IEnumerable<FieldInfo> GetNameValueFields<T>(this T thisValue)
		{
			return thisValue.IsNull()
						? Enumerable.Empty<FieldInfo>()
						: thisValue.GetType().GetFields(Constants.BF_PUBLIC_INSTANCE).Where(e => !e.IsInitOnly);
		}

		public static void EnumeratePropertyDescriptors(object source, PropertyInfoType propertyInfoType, Func<object, string, PropertyDescriptor, bool> onProperty, Func<object, string, FieldInfo, bool> onField)
		{
			if (source.IsNull())
				return;
			EnumeratePropertyDescriptorsInternal(new HashSet<string>(StringComparer.OrdinalIgnoreCase), source, null, propertyInfoType, onProperty, onField);

			static void EnumeratePropertyDescriptorsInternal(ISet<string> keys, object sourceInternal, string rootInternal, PropertyInfoType propertyInfoTypeInternal, Func<object, string, PropertyDescriptor, bool> onPropertyInternal, Func<object, string, FieldInfo, bool> onFieldInternal)
			{
				rootInternal ??= string.Empty;
				if (!string.IsNullOrEmpty(rootInternal))
					rootInternal += ".";

				if (onPropertyInternal != null)
				{
					foreach (PropertyDescriptor property in GetNameValuePropertyDescriptors(sourceInternal, propertyInfoTypeInternal))
					{
						string key = $"{rootInternal}{property.Name}";
						if (!keys.Add(key))
							continue;

						if (property.PropertyType.IsPrimitive() || property.PropertyType.IsArray)
						{
							if (!onPropertyInternal(sourceInternal, key, property))
								return;
							continue;
						}

						object value = property.GetValue(sourceInternal) ?? TypeHelper.CreateInstance(property.PropertyType);
						EnumeratePropertyDescriptorsInternal(keys, value, key, propertyInfoTypeInternal, onPropertyInternal, onFieldInternal);
					}
				}

				if (onFieldInternal != null)
				{
					foreach (FieldInfo field in GetNameValueFields(sourceInternal))
					{
						string key = $"{rootInternal}{field.Name}";
						if (!keys.Add(key))
							continue;

						if (field.FieldType.IsPrimitive() || field.FieldType.IsArray)
						{
							if (!onFieldInternal(sourceInternal, key, field))
								return;
							continue;
						}

						object value = field.GetValue(sourceInternal) ?? TypeHelper.CreateInstance(field.FieldType);
						EnumeratePropertyDescriptorsInternal(keys, value, key, propertyInfoTypeInternal, onPropertyInternal, onFieldInternal);
					}
				}
			}
		}

		public static void EnumerateProperties(object source, PropertyInfoType propertyInfoType, Func<object, string, PropertyInfo, bool> onProperty, Func<object, string, FieldInfo, bool> onField)
		{
			if (source.IsNull())
				return;
			EnumeratePropertiesInternal(new HashSet<string>(StringComparer.OrdinalIgnoreCase), source, null, propertyInfoType, onProperty, onField);

			static void EnumeratePropertiesInternal(ISet<string> keys, object sourceInternal, string rootInternal, PropertyInfoType propertyInfoTypeInternal, Func<object, string, PropertyInfo, bool> onPropertyInternal, Func<object, string, FieldInfo, bool> onFieldInternal)
			{
				rootInternal ??= string.Empty;
				if (!string.IsNullOrEmpty(rootInternal))
					rootInternal += ".";

				if (onPropertyInternal != null)
				{
					foreach (PropertyInfo property in GetNameValuePropertyInfo(sourceInternal, propertyInfoTypeInternal))
					{
						string key = $"{rootInternal}{property.Name}";
						if (!keys.Add(key))
							continue;

						if (property.PropertyType.IsPrimitive() || property.PropertyType.IsArray)
						{
							if (!onPropertyInternal(sourceInternal, key, property))
								return;
							continue;
						}

						object value = property.GetValue(sourceInternal) ?? TypeHelper.CreateInstance(property.PropertyType);
						EnumeratePropertiesInternal(keys, value, key, propertyInfoTypeInternal, onPropertyInternal, onFieldInternal);
					}
				}

				if (onFieldInternal != null)
				{
					foreach (FieldInfo field in GetNameValueFields(sourceInternal))
					{
						string key = $"{rootInternal}{field.Name}";
						if (!keys.Add(key))
							continue;

						if (field.FieldType.IsPrimitive() || field.FieldType.IsArray)
						{
							if (!onFieldInternal(sourceInternal, key, field))
								return;
							continue;
						}

						object value = field.GetValue(sourceInternal) ?? TypeHelper.CreateInstance(field.FieldType);
						EnumeratePropertiesInternal(keys, value, key, propertyInfoTypeInternal, onPropertyInternal, onFieldInternal);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Coalesce<T>(this T thisValue, params T[] values)
			where T : class
		{
			return !ReferenceEquals(thisValue, null) || values.IsNullOrEmpty()
						? thisValue
						: values.FirstOrDefault(value => !ReferenceEquals(value, null));
		}

		/// <summary>
		/// It's an efficient method to make a deep clone
		/// but unfortunately every class has to support ISerializable
		/// interface or be marked with Serializable attribute for this
		/// to work; If the object is not marked with Serializable attribute
		/// or does not implement the interface ISerializable, then it'll not
		/// be copied.
		/// If it'll will work for your purposes, then fine but otherwise
		/// I don't recommend this as a solution because it is slow (!!!)
		/// I've gone WTF!!! when I first discovered the problem as well.
		/// </summary>
		/// <param name="thisValue"></param>
		/// <param name="contextStates"></param>
		/// <param name="selector"></param>
		/// <returns>object</returns>
		public static T CloneBinary<T>([NotNull] this T thisValue, StreamingContextStates contextStates = StreamingContextStates.Clone, ISurrogateSelector selector = null)
		{
			Type type = thisValue.GetType();
			if (type.IsPrimitive())
				return thisValue;
			if (!type.IsSerializable)
				throw new ArgumentException($"Type {type.Name} is not serializable.", nameof(thisValue));

			IFormatter formatter = new BinaryFormatter { Context = new StreamingContext(contextStates) };
			if (selector != null)
				formatter.SurrogateSelector = selector;

			using (Stream stream = new MemoryStream())
			{
				formatter.Serialize(stream, thisValue);
				stream.Position = 0;
				return (T)formatter.Deserialize(stream);
			}
		}

		/// <summary>
		/// Reasonable to use for deep cloning.
		/// The normal MemberwiseClone makes a shallow copy,
		/// this different because it creates nested loops
		/// and recursive calls to make a complete deep copy.
		/// </summary>
		/// <param name="thisValue"></param>
		/// <returns>object</returns>
		public static T CloneMemberwise<T>([NotNull] this T thisValue)
		{
			Type type = thisValue.GetType();
			return type.IsDelegate()
						? default(T)
						: type.IsPrimitive()
							? thisValue
							: (T)CloneLocal(thisValue, new Dictionary<object, object>(ReferenceComparer.Default));

			object CloneLocal(object value, IDictionary<object, object> visited)
			{
				if (value == null) return null;

				Type typeToReflect = value.GetType();
				if (typeToReflect.IsDelegate()) return null;
				if (typeToReflect.IsPrimitive()) return value;
				if (visited.TryGetValue(value, out object obj)) return obj;

				object cloneObject = __cloneMethod.Value.Invoke(value, null);

				if (typeToReflect.IsArray)
				{
					Type arrayType = typeToReflect.GetElementType();

					if (arrayType != null && !arrayType.IsPrimitive())
					{
						Array clonedArray = (Array)cloneObject;
						clonedArray.ForEach((item, index) => clonedArray.SetValue(CloneLocal(item, visited), index));
					}
				}

				visited.Add(value, cloneObject);
				CopyFields(value, cloneObject, typeToReflect, visited);
				CopyPrivateFields(value, cloneObject, typeToReflect, visited);
				return cloneObject;
			}

			void CopyFields(object originalObject, object cloneObject, Type typeToReflect, IDictionary<object, object> visited, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE | BindingFlags.FlattenHierarchy, Predicate<FieldInfo> filter = null)
			{
				IEnumerable<FieldInfo> enumerable = typeToReflect.GetFields(bindingFlags)
															.Where(e => !e.FieldType.IsPrimitive());
				if (filter != null) enumerable = enumerable.Where(e => filter(e));

				foreach (FieldInfo fieldInfo in enumerable)
				{
					object originalFieldValue = fieldInfo.GetValue(originalObject);
					object clonedFieldValue = CloneLocal(originalFieldValue, visited);
					fieldInfo.SetValue(cloneObject, clonedFieldValue);
				}
			}

			void CopyPrivateFields(object originalObject, object cloneObject, Type typeToReflect, IDictionary<object, object> visited)
			{
				if (typeToReflect.BaseType == null) return;
				CopyPrivateFields(originalObject, cloneObject, typeToReflect.BaseType, visited);
				CopyFields(originalObject, cloneObject, typeToReflect.BaseType, visited, Constants.BF_NON_PUBLIC_INSTANCE, info => info.IsPrivate);
			}
		}

		/// <summary>
		/// OK, this is the fastest way to make a shallow clone
		///		but it requires that the class has a default ctor
		/// </summary>
		/// <param name="thisValue"></param>
		/// <returns>object</returns>
		public static T CloneIL<T>([NotNull] this T thisValue)
		{
			Type target = thisValue.GetType();
			Func<object, object> cloneFn = __cachedIl.GetOrAdd(target, CreateILClone);
			return (T)cloneFn?.Invoke(thisValue);

			static Func<object, object> CreateILClone(Type type)
			{
				ConstructorInfo defaultCtor = type.GetConstructor(Type.EmptyTypes);
				if (defaultCtor == null) return null;

				DynamicMethod cloneMethod = new DynamicMethod("CloneImplementation", type, new[] { type }, true);
				ILGenerator generator = cloneMethod.GetILGenerator();
				LocalBuilder builder = generator.DeclareLocal(type);

				generator.Emit(OpCodes.Newobj, defaultCtor);
				generator.Emit(OpCodes.Stloc, builder);

				FieldInfo[] fields = type.GetFields(Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE);

				foreach (FieldInfo field in fields)
				{
					generator.Emit(OpCodes.Ldloc, builder);
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Ldfld, field);
					generator.Emit(OpCodes.Stfld, field);
				}

				generator.Emit(OpCodes.Ldloc, builder);
				generator.Emit(OpCodes.Ret);

				return (Func<object, object>)cloneMethod.CreateDelegate(typeof(Func<object, object>));
			}
		}

		[NotNull]
		private static string GetFormat(bool bFractions, string suffix)
		{
			return string.IsNullOrEmpty(suffix)
						? bFractions
							? FORMAT_STRING_FRACTIONS
							: FORMAT_STRING
						: string.Concat(bFractions
											? FORMAT_STRING_FRACTIONS
											: FORMAT_STRING, suffix);
		}
	}
}