using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class NameObjectCollectionBaseExtension
{
	private enum BaseMethodType
	{
		Add,
		Remove,
		Clear,
		InvalidateCache
	}

	public static void UnlockAndAdd([NotNull] this NameObjectCollectionBase thisValue, [NotNull] string name, object value)
	{
		UnlockAndAdd(thisValue, (name, value));
	}

	public static void UnlockAndAdd([NotNull] this NameObjectCollectionBase thisValue, [NotNull] params (string, object)[] values)
	{
		UnlockAndAdd(thisValue, (IReadOnlyCollection<(string Name, object Value)>)values);
	}

	public static void UnlockAndAdd([NotNull] this NameObjectCollectionBase thisValue, [NotNull] IReadOnlyCollection<(string Name, object Value)> values)
	{
		if (values.Count == 0) return;
		UnlockAndAdd(thisValue, values.Select(tuple => new KeyValuePair<string, object>(tuple.Name, tuple.Value)).ToArray());
	}

	public static void UnlockAndAdd([NotNull] this NameObjectCollectionBase thisValue, [NotNull] IReadOnlyCollection<KeyValuePair<string, object>> values)
	{
		if (values.Count == 0) return;

		MethodInfo baseAddMethod = GetBaseMethod(thisValue, BaseMethodType.Add);
		MethodInfo invalidateCachedArraysMethod = GetBaseMethod(thisValue, BaseMethodType.InvalidateCache);
		bool isReadOnly = thisValue.Unlock();

		try
		{
			invalidateCachedArraysMethod.Invoke(thisValue, null);

			foreach (KeyValuePair<string, object> pair in values.Where(p => !string.IsNullOrEmpty(p.Key)))
			{
				baseAddMethod.Invoke(thisValue, new[]
				{
					pair.Key,
					pair.Value
				});
			}
		}
		finally
		{
			if (isReadOnly) thisValue.Lock();
		}
	}

	public static void UnlockAndRemove([NotNull] this NameObjectCollectionBase thisValue, [NotNull] params string[] names)
	{
		if (thisValue.Count == 0 || names.Length == 0) return;

		MethodInfo baseRemoveMethod = GetBaseMethod(thisValue, BaseMethodType.Remove);
		MethodInfo invalidateCachedArraysMethod = GetBaseMethod(thisValue, BaseMethodType.InvalidateCache);
		bool isReadOnly = thisValue.Unlock();

		try
		{
			invalidateCachedArraysMethod.Invoke(thisValue, null);

			foreach (string key in names.SkipNullOrEmpty())
			{
				baseRemoveMethod.Invoke(thisValue, new object[]
				{
					key
				});
			}
		}
		finally
		{
			if (isReadOnly) thisValue.Lock();
		}
	}

	public static void UnlockAndClear([NotNull] this NameObjectCollectionBase thisValue)
	{
		if (thisValue.Count == 0) return;

		MethodInfo baseClearMethod = GetBaseMethod(thisValue, BaseMethodType.Clear);
		MethodInfo invalidateCachedArraysMethod = GetBaseMethod(thisValue, BaseMethodType.InvalidateCache);
		bool isReadOnly = thisValue.Unlock();

		try
		{
			invalidateCachedArraysMethod.Invoke(thisValue, null);
			baseClearMethod.Invoke(thisValue, null);
		}
		finally
		{
			if (isReadOnly) thisValue.Lock();
		}
	}

	[NotNull]
	private static MethodInfo GetBaseMethod([NotNull] NameObjectCollectionBase collection, BaseMethodType type)
	{
		const BindingFlags BF_FLAGS = Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.InvokeMethod;

		string name;

		switch (type)
		{
			case BaseMethodType.Add:
				name = "BaseAdd";
				break;
			case BaseMethodType.Remove:
				name = "BaseRemove";
				break;
			case BaseMethodType.Clear:
				name = "BaseClear";
				break;
			case BaseMethodType.InvalidateCache:
				name = "InvalidateCachedArrays";
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}

		return collection.GetType().GetMethod(name, BF_FLAGS) ?? throw new InvalidOperationException($"Could not acquire {name} method.");
	}
}