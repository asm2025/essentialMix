using System;
using System.Collections;
using JetBrains.Annotations;
using essentialMix.Delegation;

namespace essentialMix.Extensions;

public static class HashtableExtension
{
	public static T Get<T>([NotNull] this Hashtable thisValue, [NotNull] object key) { return Get(thisValue, key, default(T)); }

	public static T Get<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue) { return Get(thisValue, key, defaultValue, null); }

	public static T Get<T>([NotNull] this Hashtable thisValue, [NotNull] object key, Func<string, T, T> whenFailed) { return Get(thisValue, key, default(T), whenFailed); }

	public static T Get<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, Func<string, T, T> whenFailed)
	{
		return Get(thisValue, key, defaultValue, null, whenFailed);
	}

	public static T Get<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		object value = thisValue[key];
		return value.IsNull() ? defaultValue : value.To(defaultValue, beforeParse, whenFailed);
	}

	public static T Get<T, TCheck>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
	{
		return Get(thisValue, key, defaultValue, beforeConvert, null, whenFailed);
	}

	public static T Get<T, TCheck>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		object value = thisValue[key];
		return value.IsNull() ? defaultValue : value.To(defaultValue, beforeParse, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this Hashtable thisValue, [NotNull] object key, out T value) { return TryGet(thisValue, key, default(T), out value); }

	public static bool TryGet<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, out T value)
	{
		return TryGet(thisValue, key, defaultValue, out value, null);
	}

	public static bool TryGet<T>([NotNull] this Hashtable thisValue, [NotNull] object key, out T value, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, key, default(T), out value, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, out T value, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, key, defaultValue, out value, null, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, out T value, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		if (!thisValue.ContainsKey(key))
		{
			value = defaultValue;
			return false;
		}

		object v = thisValue[key];

		if (v.IsNull())
		{
			value = defaultValue;
			return false;
		}
			
		value = v.To(defaultValue, beforeParse, whenFailed);
		return true;
	}

	public static bool TryGet<T, TCheck>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, key, defaultValue, out value, beforeConvert, null, whenFailed);
	}

	public static bool TryGet<T, TCheck>([NotNull] this Hashtable thisValue, [NotNull] object key, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		if (!thisValue.ContainsKey(key))
		{
			value = defaultValue;
			return false;
		}

		object v = thisValue[key];

		if (v.IsNull())
		{
			value = defaultValue;
			return false;
		}

		value = v.To(defaultValue, beforeConvert, beforeParse, whenFailed);
		return true;
	}
}