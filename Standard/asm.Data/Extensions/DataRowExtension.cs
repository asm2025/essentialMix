using System;
using System.Data;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Delegation;

namespace asm.Data.Extensions
{
	public static class DataRowExtension
	{
		public static bool IsValid(this DataRow thisValue, bool bCount = false, bool bNoErrors = false)
		{
			return thisValue != null && (bCount || thisValue.Table.Columns.Count > 0) && (bNoErrors || !thisValue.HasErrors);
		}

		public static T Get<T>([NotNull] this DataRow thisValue, [NotNull] string name) { return Get(thisValue, name, default(T)); }

		public static T Get<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue) { return Get(thisValue, name, defaultValue, null); }

		public static T Get<T>([NotNull] this DataRow thisValue, [NotNull] string name, Func<string, T, T> whenFailed) { return Get(thisValue, name, default(T), whenFailed); }

		public static T Get<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, Func<string, T, T> whenFailed)
		{
			return Get(thisValue, name, defaultValue, null, whenFailed);
		}

		public static T Get<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			object value = thisValue[name];
			return value.IsNull() ? defaultValue : value.To(defaultValue, beforeParse, whenFailed);
		}

		public static T Get<T, TCheck>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
		{
			return Get(thisValue, name, defaultValue, beforeConvert, null, whenFailed);
		}

		public static T Get<T, TCheck>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			object value = thisValue[name];
			return value.IsNull() ? defaultValue : value.To(defaultValue, beforeParse, whenFailed);
		}

		public static bool TryGet<T>([NotNull] this DataRow thisValue, [NotNull] string name, out T value) { return TryGet(thisValue, name, default(T), out value); }

		public static bool TryGet<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, out T value)
		{
			return TryGet(thisValue, name, defaultValue, out value, null);
		}

		public static bool TryGet<T>([NotNull] this DataRow thisValue, [NotNull] string name, out T value, Func<string, T, T> whenFailed)
		{
			return TryGet(thisValue, name, default(T), out value, whenFailed);
		}

		public static bool TryGet<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, out T value, Func<string, T, T> whenFailed)
		{
			return TryGet(thisValue, name, defaultValue, out value, null, whenFailed);
		}

		public static bool TryGet<T>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			if (!thisValue.Table.Columns.Contains(name))
			{
				value = defaultValue;
				return false;
			}

			object v = thisValue[name];

			if (v.IsNull())
			{
				value = defaultValue;
				return false;
			}

			value = v.To(defaultValue, beforeParse, whenFailed);
			return true;
		}

		public static bool TryGet<T, TCheck>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
		{
			return TryGet(thisValue, name, defaultValue, out value, beforeConvert, null, whenFailed);
		}

		public static bool TryGet<T, TCheck>([NotNull] this DataRow thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
		{
			if (!thisValue.Table.Columns.Contains(name))
			{
				value = defaultValue;
				return false;
			}

			object v = thisValue[name];

			if (v.IsNull())
			{
				value = defaultValue;
				return false;
			}

			value = v.To(defaultValue, beforeConvert, beforeParse, whenFailed);
			return true;
		}
	}
}