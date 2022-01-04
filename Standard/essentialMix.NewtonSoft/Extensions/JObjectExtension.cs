using System;
using System.Collections.Generic;
using essentialMix.Delegation;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class JObjectExtension
{
	public static T Get<T>([NotNull] this JObject thisValue, [NotNull] string name) { return Get(thisValue, name, default(T)); }

	public static T Get<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue) { return Get(thisValue, name, defaultValue, null); }

	public static T Get<T>([NotNull] this JObject thisValue, [NotNull] string name, Func<string, T, T> whenFailed) { return Get(thisValue, name, default(T), whenFailed); }

	public static T Get<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, Func<string, T, T> whenFailed)
	{
		return Get(thisValue, name, defaultValue, null, whenFailed);
	}

	public static T Get<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		JObject obj = thisValue;
		string nam = name;
		if (!WalkPath(ref obj, ref nam)) return defaultValue;
		return obj.GetValue(nam, StringComparison.OrdinalIgnoreCase) is not JValue value 
					? defaultValue
					: value.Value.To(defaultValue, beforeParse, whenFailed);
	}

	public static T Get<T, TCheck>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
	{
		return Get(thisValue, name, defaultValue, beforeConvert, null, whenFailed);
	}

	public static T Get<T, TCheck>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		JObject obj = thisValue;
		string nam = name;
		if (!WalkPath(ref obj, ref nam)) return defaultValue;
		return obj.GetValue(nam, StringComparison.OrdinalIgnoreCase) is not JValue value 
					? defaultValue
					: value.To(defaultValue, beforeParse, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this JObject thisValue, [NotNull] string name, out T value) { return TryGet(thisValue, name, default(T), out value); }

	public static bool TryGet<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, out T value)
	{
		return TryGet(thisValue, name, defaultValue, out value, null);
	}

	public static bool TryGet<T>([NotNull] this JObject thisValue, [NotNull] string name, out T value, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, name, default(T), out value, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, out T value, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, name, defaultValue, out value, null, whenFailed);
	}

	public static bool TryGet<T>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		if (!TryWalkPath(ref thisValue, ref name))
		{
			value = defaultValue;
			return false;
		}

		if (!thisValue.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out JToken token)
			|| token is not JValue jValue)
		{
			value = defaultValue;
			return false;
		}

		value = jValue.Value.To(defaultValue, beforeParse, whenFailed);
		return true;
	}

	public static bool TryGet<T, TCheck>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, Func<string, T, T> whenFailed)
	{
		return TryGet(thisValue, name, defaultValue, out value, beforeConvert, null, whenFailed);
	}

	public static bool TryGet<T, TCheck>([NotNull] this JObject thisValue, [NotNull] string name, T defaultValue, out T value, OutWithDefaultFunc<TCheck, T, bool> beforeConvert, OutWithDefaultFunc<string, T, bool> beforeParse, Func<string, T, T> whenFailed)
	{
		if (!TryWalkPath(ref thisValue, ref name))
		{
			value = defaultValue;
			return false;
		}

		if (!thisValue.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out JToken token)
			|| token is not JValue jValue)
		{
			value = defaultValue;
			return false;
		}

		value = jValue.Value.To(defaultValue, beforeConvert, beforeParse, whenFailed);
		return true;
	}

	private static bool WalkPath(ref JObject jObject, ref string name)
	{
		if (jObject == null || string.IsNullOrEmpty(name)) return false;
		if (!name.Contains('.')) return true;
			
		Queue<string> queue = new Queue<string>(name.Split(StringSplitOptions.RemoveEmptyEntries, '.'));

		while (jObject != null && queue.Count > 1)
		{
			name = queue.Dequeue();
			if (jObject.GetValue(name, StringComparison.OrdinalIgnoreCase) is not JObject jObj || !jObj.HasValues) return false;
			jObject = jObj;
		}

		if (jObject == null || queue.Count < 1) return false;
		name = queue.Dequeue();
		return true;
	}

	private static bool TryWalkPath(ref JObject jObject, ref string name)
	{
		if (jObject == null || string.IsNullOrEmpty(name)) return false;
		if (!name.Contains('.')) return true;
			
		Queue<string> queue = new Queue<string>(name.Split(StringSplitOptions.RemoveEmptyEntries, '.'));

		while (jObject != null && queue.Count > 1)
		{
			name = queue.Dequeue();

			if (!jObject.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out JToken token) 
				|| token is not JObject jObj
				|| !jObj.HasValues) return false;

			jObject = jObj;
		}

		if (jObject == null || queue.Count < 1) return false;
		name = queue.Dequeue();
		return true;
	}
}