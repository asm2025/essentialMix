using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using essentialMix.Exceptions.Collections;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class ISetExtension
{
	public static T GetByIndex<T>([NotNull] this ISet<T> thisValue, int index) { return GetByIndex(thisValue, index, default(T)); }

	public static T GetByIndex<T>([NotNull] this ISet<T> thisValue, int index, T defaultKey)
	{
		if (!index.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index));
		return index == 0 ? thisValue.First() : thisValue.Skip(index + 1).First();
	}

	public static IEqualityComparer<T> GetComparer<T>([NotNull] this ISet<T> thisValue, string fieldName = null)
	{
		Type type = thisValue.GetType();
		PropertyInfo comparerProperty = type.GetProperty("Comparer", Constants.BF_PUBLIC_NON_PUBLIC_INSTANCE, typeof(IEqualityComparer<T>));
		if (comparerProperty != null) return (IEqualityComparer<T>)comparerProperty.GetValue(thisValue);

		fieldName = fieldName.ToNullIfEmpty();
		bool useDefaultName = fieldName == null;
		if (useDefaultName) fieldName = "_comparer";
		FieldInfo comparerField = type.GetField(fieldName, Constants.BF_NON_PUBLIC_INSTANCE);

		if (comparerField == null)
		{
			fieldName = "comparer";
			comparerField = type.GetField(fieldName, Constants.BF_NON_PUBLIC_INSTANCE);
		}

		return comparerField == null || !typeof(IEqualityComparer<T>).IsAssignableFrom(comparerField.FieldType)
					? null
					: (IEqualityComparer<T>)comparerField.GetValue(thisValue);
	}

	public static T PickRandom<T>([NotNull] this ISet<T> thisValue)
	{
		if (thisValue.Count == 0) throw new CollectionIsEmptyException();

		int max;
		int n;

		if (thisValue is ICollection collection)
		{
			lock (collection.SyncRoot)
			{
				max = thisValue.Count - 1;
				n = RNGRandomHelper.Next(0, max);
				return thisValue.ElementAt(n);
			}
		}

		lock (thisValue)
		{
			max = thisValue.Count - 1;
			n = RNGRandomHelper.Next(0, max);
			return thisValue.ElementAt(n);
		}
	}

	public static T PopRandom<T>([NotNull] this ISet<T> thisValue)
	{
		if (thisValue.Count == 0) throw new CollectionIsEmptyException();

		int max;
		int n;
		T result;

		if (thisValue is ICollection collection)
		{
			lock (collection.SyncRoot)
			{
				max = thisValue.Count - 1;
				n = RNGRandomHelper.Next(0, max);
				result = thisValue.ElementAt(n);
				thisValue.Remove(result);
				return result;
			}
		}

		lock (thisValue)
		{
			max = thisValue.Count - 1;
			n = RNGRandomHelper.Next(0, max);
			result = thisValue.ElementAt(n);
			thisValue.Remove(result);
			return result;
		}
	}

	public static T PopFirst<T>([NotNull] this ISet<T> thisValue)
	{
		if (thisValue.Count == 0) throw new CollectionIsEmptyException();

		T result;

		if (thisValue is ICollection { IsSynchronized: true } collection)
		{
			lock (collection.SyncRoot)
			{
				result = thisValue.First();
				thisValue.Remove(result);
				return result;
			}
		}

		lock (thisValue)
		{
			result = thisValue.First();
			thisValue.Remove(result);
			return result;
		}
	}

	public static T PopLast<T>([NotNull] this ISet<T> thisValue)
	{
		if (thisValue.Count == 0) throw new CollectionIsEmptyException();

		T result;

		if (thisValue is ICollection { IsSynchronized: true } collection)
		{
			lock (collection.SyncRoot)
			{
				result = thisValue.Last();
				thisValue.Remove(result);
				return result;
			}
		}

		lock (thisValue)
		{
			result = thisValue.Last();
			thisValue.Remove(result);
			return result;
		}
	}
}