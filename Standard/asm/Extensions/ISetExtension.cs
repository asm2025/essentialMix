using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using asm.Collections;
using asm.Collections.Concurrent;
using JetBrains.Annotations;

namespace asm.Extensions
{
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
			switch (thisValue)
			{
				case HashSet<T> hashSet:
					return hashSet.Comparer;
				case ConcurrentSet<T> concurrentSet:
					return concurrentSet.Comparer;
				case Microsoft.Collections.HashSet<T> hashSetBase:
					return hashSetBase.Comparer;
			}

			Type type = thisValue.GetType();
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

		[NotNull]
		public static IReadOnlySet<T> AsReadOnly<T>([NotNull] this ISet<T> thisValue) { return new ReadOnlySet<T>(thisValue); }
	}
}