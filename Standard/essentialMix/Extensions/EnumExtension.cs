using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class EnumExtension
	{
		public static bool HasMoreThan<T>(this T thisValue, int count = 0)
			where T : struct, Enum, IComparable
		{
			IEnumerable<T> flags = EnumHelper<T>.GetValues();
			int lo = 0, hi = count + 1;
			return flags.Count(e => FastHasFlag(thisValue, e) && ++lo < hi) >= count;
		}

		public static int Count<T>(this T thisValue, [NotNull] params T[] flags)
			where T : struct, Enum, IComparable
		{
			return flags.Count(e => FastHasFlag(thisValue, e));
		}

		public static T HighestFlag<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			Type type = typeof(T);
			T value = default(T);
			if (!type.HasAttribute<FlagsAttribute>()) return value;

			T[] flags = EnumHelper<T>.GetValues();

			foreach (T flag in flags)
			{
				if (FastHasFlag(thisValue, flag))
					value = flag;
			}

			return value;
		}

		[NotNull]
		public static T[] Flags<T>(this T thisValue, params T[] exclude)
			where T : struct, Enum, IComparable
		{
			Type type = typeof(T);
			if (!type.HasAttribute<FlagsAttribute>()) return Array.Empty<T>();

			T[] values = EnumHelper<T>.GetValues();
			return exclude is { Length: > 0 }
						? values.Where(e => !exclude.Contains(e) && FastHasFlag(thisValue, e)).ToArray()
						: values;
		}

		public static string GetName<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return EnumHelper<T>.GetName(thisValue);
		}

		public static string GetDisplayName<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			string name = GetName(thisValue);
			return thisValue.GetType()
				.GetField(name)
				.GetDisplayName(name);
		}

		public static DisplayAttribute GetDisplay<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return thisValue.GetType()
							.GetField(GetName(thisValue))
							.GetDisplay();
		}

		public static string GetDescription<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return thisValue.GetType()
							.GetField(GetName(thisValue))
							.GetDescription();
		}

		[NotNull]
		public static Type GetUnderlyingType<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return thisValue.AsType(true).GetUnderlyingType();
		}

		public static TypeCode GetUnderlyingTypeCode<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return thisValue.AsType(true).GetUnderlyingTypeCode();
		}

		public static FieldInfo GetField<T>(this T thisValue)
			where T : struct, Enum, IComparable
		{
			return thisValue.GetType()
							.GetField(GetName(thisValue));
		}

		[NotNull]
		public static IEnumerable<Attribute> GetAttributes<T>(this T thisValue, Type type = null)
			where T : struct, Enum, IComparable
		{
			FieldInfo field = GetField(thisValue);
			return field?.GetAttributes(type) ?? Enumerable.Empty<Attribute>();
		}

		public static Attribute GetAttribute<T>(this T thisValue, Type type = null)
			where T : struct, Enum, IComparable
		{
			return GetAttributes(thisValue, type).FirstOrDefault();
		}

		public static bool HasAttribute<T>(this T thisValue, Type type = null)
			where T : struct, Enum, IComparable
		{
			return GetAttribute(thisValue, type) != null;
		}

		public static TEnum ChangeTo<T, TEnum>(this T thisValue)
			where T : struct, Enum, IComparable
			where TEnum : struct, Enum, IComparable
		{
			return (TEnum)ChangeTo(thisValue, typeof(TEnum));
		}

		[NotNull]
		public static object ChangeTo<T>(this T thisValue, [NotNull] Type type)
			where T : struct, Enum, IComparable
		{
			if (!type.IsEnum) throw new InvalidEnumArgumentException();
			return Enum.ToObject(type, thisValue);
		}

		[NotNull]
		public static IEnumerable<TAttribute> GetAttributes<T, TAttribute>(this T thisValue)
			where T : struct, Enum, IComparable
			where TAttribute : Attribute
		{
			return GetAttributes(thisValue, typeof(TAttribute)).CastTo<Attribute, TAttribute>();
		}

		public static TAttribute GetAttribute<T, TAttribute>(this T thisValue)
			where T : struct, Enum, IComparable
			where TAttribute : Attribute
		{
			return GetAttributes<T, TAttribute>(thisValue).FirstOrDefault();
		}

		public static bool HasAttribute<T, TAttribute>(this T thisValue)
			where T : struct, Enum, IComparable
			where TAttribute : Attribute
		{
			return GetAttribute<T, TAttribute>(thisValue) != null;
		}

		public static bool IsMemberOf<T>(this T thisValue, [NotNull] Type type)
			where T : struct, Enum, IComparable
		{
			return type.IsDefined(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool FastHasFlag<T>(this T thisValue, T flag)
			where T : struct, Enum, IComparable
		{
			return EnumHelper<T>.HasAllFlags(thisValue, flag);
		}
	}
}