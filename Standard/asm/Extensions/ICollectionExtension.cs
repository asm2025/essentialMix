using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class ICollectionExtension
	{
		public static void Add([NotNull] this ICollection thisValue, [NotNull] params object[] values) { AddRange(thisValue, values); }
		public static void Add<T>([NotNull] this ICollection<T> thisValue, [NotNull] params T[] values) { AddRange(thisValue, values); }

		public static void AddRange([NotNull] this ICollection thisValue, [NotNull] IEnumerable values)
		{
			foreach (object item in values)
				thisValue.Add(item);
		}

		public static void AddRange<T>([NotNull] this ICollection<T> thisValue, [NotNull] IEnumerable<T> values)
		{
			foreach (T item in values)
				thisValue.Add(item);
		}

		public static void CopyFrom<T>([NotNull] this ICollection<T> thisValue, [NotNull] IEnumerable<T> values)
		{
			thisValue.Clear();
			AddRange(thisValue, values);
		}

		public static void CloneFrom<T>([NotNull] this ICollection<T> thisValue, [NotNull] IEnumerable<T> values)
		{
			thisValue.Clear();

			if (typeof(T) is ICloneable)
			{
				foreach (T value in values)
					thisValue.Add((T)((ICloneable)value).Clone());
			}
			else
			{
				foreach (T value in values)
					thisValue.Add(value);
			}
		}

		public static void RemoveRange<T>([NotNull] this ICollection<T> thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Count == 0) return;

			if (count == 0) return;

			T[] items = thisValue.GetRange(startIndex, count);

			foreach (T item in items)
				thisValue.Remove(item);
		}

		public static int RemoveAll<T>([NotNull] this ICollection<T> thisValue, [NotNull] Predicate<T> match)
		{
			ICollection<T> items = new Collection<T>();

			foreach (T item in thisValue)
			{
				if (!match(item)) continue;
				items.Add(item);
			}

			if (items.Count == 0) return 0;

			foreach (T item in items)
				thisValue.Remove(item);

			return items.Count;
		}

		public static void MoveTo<T>([NotNull] this ICollection<T> thisValue, [NotNull] ICollection<T> destination)
		{
			destination.AddRange(thisValue);
			thisValue.Clear();
		}

		[NotNull]
		public static object[] GetRange([NotNull] this ICollection thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Count == 0) return Array.Empty<object>();

			int n = 0;
			object[] range = new object[count];

			foreach (object item in thisValue.Cast<object>().Skip(startIndex).Take(count)) 
				range[n++] = item;

			return range;
		}

		[NotNull]
		public static T[] GetRange<T>([NotNull] this ICollection<T> thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Count == 0) return Array.Empty<T>();

			int n = 0;
			T[] range = new T[count];

			foreach (T item in thisValue.Skip(startIndex).Take(count)) 
				range[n++] = item;

			return range;
		}

		public static IEnumerable<IReadOnlyCollection<T>> Partition<T>([NotNull] this ICollection<T> thisValue, int size, PartitionSize type)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0 || thisValue.Count == 0) return Enumerable.Empty<IReadOnlyCollection<T>>();
			if (type == PartitionSize.TotalCount) size = (int)Math.Ceiling(thisValue.Count / (double)size);
			return thisValue.Partition(size);
		}

		public static IEnumerable<IReadOnlyCollection<T>> PartitionUnique<T>([NotNull] this ICollection<T> thisValue, int size, PartitionSize type, IEqualityComparer<T> comparer = null)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0 || thisValue.Count == 0) return Enumerable.Empty<IReadOnlyCollection<T>>();
			if (type == PartitionSize.TotalCount) size = (int)Math.Ceiling(thisValue.Count / (double)size);
			return thisValue.PartitionUnique(size, comparer);
		}

		public static bool SequenceEqual<T>([NotNull] this ICollection<T> thisValue, ICollection<T> other, IEqualityComparer<T> comparer = null)
		{
			if (thisValue.Count != other?.Count) return false;
			if (thisValue.Count == 0) return true;
			if (comparer == null) comparer = EqualityComparer<T>.Default;
			return Enumerable.SequenceEqual(thisValue, other, comparer);
		}

		public static bool Lock([NotNull] this ICollection thisValue)
		{
			PropertyInfo isReadOnlyProperty = GetReadOnlyProperty(thisValue);
			bool isReadOnly = (bool)isReadOnlyProperty.GetValue(thisValue);
			if (!isReadOnly) isReadOnlyProperty.SetValue(thisValue, true);
			return !isReadOnly;
		}

		public static bool Lock<T>([NotNull] this ICollection<T> thisValue)
		{
			PropertyInfo isReadOnlyProperty = GetReadOnlyProperty(thisValue);
			bool isReadOnly = (bool)isReadOnlyProperty.GetValue(thisValue);
			if (!isReadOnly) isReadOnlyProperty.SetValue(thisValue, true);
			return !isReadOnly;
		}

		public static bool Unlock([NotNull] this ICollection thisValue)
		{
			PropertyInfo isReadOnlyProperty = GetReadOnlyProperty(thisValue);
			bool isReadOnly = (bool)isReadOnlyProperty.GetValue(thisValue);
			if (isReadOnly) isReadOnlyProperty.SetValue(thisValue, false);
			return isReadOnly;
		}

		public static bool Unlock<T>([NotNull] this ICollection<T> thisValue)
		{
			PropertyInfo isReadOnlyProperty = GetReadOnlyProperty(thisValue);
			bool isReadOnly = (bool)isReadOnlyProperty.GetValue(thisValue);
			if (isReadOnly) isReadOnlyProperty.SetValue(thisValue, false);
			return isReadOnly;
		}

		[NotNull]
		private static PropertyInfo GetReadOnlyProperty([NotNull] ICollection collection)
		{
			return collection.GetType().GetProperty("IsReadOnly", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.FlattenHierarchy) ?? throw new InvalidOperationException("Could not get IsReadOnly property.");
		}

		[NotNull]
		private static PropertyInfo GetReadOnlyProperty<T>([NotNull] ICollection<T> collection)
		{
			return collection.GetType().GetProperty("IsReadOnly", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.FlattenHierarchy) ?? throw new InvalidOperationException("Could not get IsReadOnly property.");
		}
	}
}