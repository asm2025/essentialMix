using System;
using System.Linq;
using System.Web;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class HttpApplicationStateExtension
{
	public static T GetValueOf<T>([NotNull] this HttpApplicationState thisValue, [NotNull] string name, T defaultValue = default(T))
	{
		T result = defaultValue;
		if (name.Length == 0) return result;
		thisValue.Lock();

		string key = thisValue.AllKeys.FirstOrDefault(k => k.IsSame(name));
		if (key != null) result = thisValue[key].To(result);

		thisValue.UnLock();
		return result;
	}

	public static T GetValueOf<T>([NotNull] this HttpApplicationState thisValue, int index, T defaultValue = default(T))
	{
		T result = defaultValue;
		thisValue.Lock();
		if (thisValue.Keys.Count > 0 && index.InRangeRx(0, thisValue.Keys.Count)) result = thisValue.Get(index).To(result);
		thisValue.UnLock();
		return result;
	}

	public static void SetValueOf<T>([NotNull] this HttpApplicationState thisValue, [NotNull] string name, T value)
	{
		if (name.Length == 0) throw new ArgumentNullException(nameof(name));
		thisValue.Lock();

		string key = thisValue.AllKeys.FirstOrDefault(k => k.IsSame(name));

		if (value == null || value.Equals(default(T)))
		{
			if (key != null) thisValue.Remove(key);
		}
		else
		{
			key ??= name;
			thisValue[key] = value;
		}

		thisValue.UnLock();
	}
}