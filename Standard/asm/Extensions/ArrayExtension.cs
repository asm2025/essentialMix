using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using asm.Collections;
using asm.Comparers;

namespace asm.Extensions
{
	public static class ArrayExtension
	{
		public static bool IsNullOrEmpty(this Array thisValue) { return thisValue == null || thisValue.Length == 0; }

		[NotNull]
		public static Array Prepend([NotNull] this Array thisValue, [NotNull] params object[] items)
		{
			if (thisValue.Length == 0) return items;
			if (thisValue.Rank > 1) throw new RankException("Arrays of rank greater than one are not supported.");
			if (items.Length == 0) return thisValue;
			Array array = new object[thisValue.Length + items.Length];
			Array.Copy(items, 0, array, 0, items.Length);
			Array.Copy(thisValue, 0, array, items.Length, thisValue.Length);
			return array;
		}

		[NotNull]
		public static Array Append([NotNull] this Array thisValue, [NotNull] params object[] items)
		{
			if (thisValue.Length == 0) return items;
			if (thisValue.Rank > 1) throw new RankException("Arrays of rank greater than one are not supported.");
			if (items.Length == 0) return thisValue;
			Array array = new object[thisValue.Length + items.Length];
			Array.Copy(thisValue, 0, array, 0, thisValue.Length);
			Array.Copy(items, 0, array, thisValue.Length, items.Length);
			return array;
		}

		[NotNull]
		public static Array Insert([NotNull] this Array thisValue, int index, [NotNull] params object[] value)
		{
			if (!index.InRange(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(index));
			if (thisValue.Length == 0) return value;
			if (value.Length == 0) return thisValue;

			int n;
			Array array = new object[thisValue.Length + value.Length];

			if (index == thisValue.Length)
			{
				n = thisValue.Length;
				Array.Copy(thisValue, 0, array, 0, thisValue.Length);
			}
			else if (index == 0)
			{
				n = 0;
				Array.Copy(thisValue, 0, array, value.Length, thisValue.Length);
			}
			else
			{
				n = index;
				Array.Copy(thisValue, 0, array, 0, n);
				Array.Copy(thisValue, n, array, n + value.Length, thisValue.Length - n);
			}

			Array.Copy(value, 0, array, n, value.Length);
			return thisValue;
		}

		[NotNull]
		public static Array Remove([NotNull] this Array thisValue, int startIndex, int length)
		{
			if (!startIndex.InRangeRx(0, thisValue.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > thisValue.Length) throw new ArgumentOutOfRangeException(nameof(length));
			if (length == 0) return thisValue;
			if (length == thisValue.Length) return Array.Empty<object>();

			Array newValues = new object[thisValue.Length - length];
			if (startIndex > 0) Array.Copy(thisValue, 0, newValues, 0, startIndex);

			int limit = startIndex + length;
			int remaining = thisValue.Length - limit;
			if (remaining > 0) Array.Copy(thisValue, limit, newValues, startIndex, remaining);
			return newValues;
		}

		public static void Initialize([NotNull] this Array thisValue, object value, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (thisValue.Length == 0) return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue.SetValue(value, i);
		}

		public static void Initialize([NotNull] this Array thisValue, [NotNull] Func<int, object> func, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (thisValue.Length == 0) return;

			int lastPos = startIndex + count;
			
			for (int i = startIndex; i < lastPos; i++)
				thisValue.SetValue(func(i), i);
		}

		[NotNull]
		public static Array GetRange([NotNull] this Array thisValue, int startIndex, int count)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length == 0) return Array.Empty<object>();

			object[] range = new object[count];
			Array.Copy(thisValue, startIndex, range, 0, count);
			return range;
		}

		public static bool Exists([NotNull] this Array thisValue, [NotNull] Predicate<object> predicate) { return Array.Exists((object[])thisValue, predicate); }

		public static bool Contains([NotNull] this Array thisValue, Array values)
		{
			if (thisValue.Rank != 1) throw new RankException();
			if (values == null) return false;
			if (values.Rank != 1) throw new RankException();
			if (thisValue.Length == 0 && values.Length == 0) return true;
			return values.Length <= thisValue.Length && values.Cast<object>().All(v => thisValue.Cast<object>().Contains(v));
		}

		public static bool Contains([NotNull] this Array thisValue, Array values, [NotNull] IEqualityComparer<object> comparer)
		{
			if (thisValue.Rank != 1) throw new RankException();
			if (values == null) return false;
			if (values.Rank != 1) throw new RankException();
			if (thisValue.Length == 0 && values.Length == 0) return true;
			return values.Length <= thisValue.Length && values.Cast<object>().All(v => thisValue.Cast<object>().Contains(v, comparer));
		}

		public static int IndexOf([NotNull] this Array thisValue, object value, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			return Array.IndexOf(thisValue, value, startIndex, count);
		}

		public static int IndexOf([NotNull] this Array thisValue, [NotNull] Predicate<object> comparison, int startIndex = 0, int count = -1)
		{
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length == 0) return -1;

			int ndx = -1;
			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
			{
				if (!comparison(thisValue.GetValue(i))) continue;
				ndx = i;
				break;
			}

			return ndx;
		}

		public static void Sort([NotNull] this Array thisValue, IComparer comparer = null, int startIndex = 0, int count = -1)
		{
			if (thisValue.Rank != 1) throw new RankException();
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length < 2) return;
			Array.Sort(thisValue, startIndex, count, comparer);
		}

		public static void Sort([NotNull] this Array thisValue, [NotNull] Comparison<object> comparison, int startIndex, int count)
		{
			if (thisValue.Rank != 1) throw new RankException();
			thisValue.Length.ValidateRange(startIndex, ref count);
			if (count == 0 || thisValue.Length < 2) return;
			Array.Sort(thisValue, startIndex, count, new FunctorComparer<object>(comparison));
		}

		[NotNull]
		public static Type[] Types([NotNull] this Array thisValue)
		{
			if (thisValue.Length == 0) return Type.EmptyTypes;

			Type[] types = new Type[thisValue.Length];
			thisValue.ForEach((o, i) => types[i] = o?.AsType() ?? typeof(object));
			return types;
		}

		public static void ForEach([NotNull] this Array thisValue, [NotNull] Action<object> action)
		{
			if (thisValue.Length == 0) return;
			
			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static void ForEach([NotNull] this Array thisValue, [NotNull] Action<object, int> action)
		{
			if (thisValue.Length == 0) return;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current, enumerator.Position);
			}
		}

		public static void ForEach([NotNull] this Array thisValue, [NotNull] Func<object, bool> action)
		{
			if (thisValue.Length == 0) return;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current)) break;
				}
			}
		}

		public static void ForEach([NotNull] this Array thisValue, [NotNull] Func<object, int, bool> action)
		{
			if (thisValue.Length == 0) return;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current, enumerator.Position)) break;
				}
			}
		}

		public static bool All([NotNull] this Array thisValue, [NotNull] Func<object, bool> predicate)
		{
			if (thisValue.Length == 0) return false;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!predicate(enumerator.Current)) return false;
				}
			}

			return true;
		}

		public static bool All([NotNull] this Array thisValue, [NotNull] Func<object, int, bool> predicate)
		{
			if (thisValue.Length == 0) return false;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!predicate(enumerator.Current, enumerator.Position)) return false;
				}
			}

			return true;
		}

		public static bool Any([NotNull] this Array thisValue, [NotNull] Func<object, bool> predicate)
		{
			if (thisValue.Length == 0) return false;

			using (Enumerator enumerator = new Enumerator(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!predicate(enumerator.Current)) return true;
				}
			}

			return false;
		}
	}
}