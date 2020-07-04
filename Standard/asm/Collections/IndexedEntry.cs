using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// Represents each entry returned within a collection,
	/// containing the value and whether it is the first and/or
	/// the last entry in the collection's. enumeration
	/// </summary>
	public class IndexedEntryBase
	{
		protected IndexedEntryBase(bool isFirst, bool isLast, int index)
		{
			IsFirst = isFirst;
			IsLast = isLast;
			Index = index;
		}

		/// <summary>
		/// Whether or not this entry is first in the collection's enumeration.
		/// </summary>
		public bool IsFirst { get; }

		/// <summary>
		/// Whether or not this entry is last in the collection's enumeration.
		/// </summary>
		public bool IsLast { get; }

		/// <summary>
		/// The 0-based index of this entry (i.e. how many entries have been returned before this one)
		/// </summary>
		public int Index { get; }
	}

	/// <inheritdoc />
	public class IndexedEntry : IndexedEntryBase
	{
		protected IndexedEntry(bool isFirst, bool isLast, object value, int index)
			: base(isFirst, isLast, index)
		{
			Value = value;
		}

		/// <summary>
		/// The value of the entry.
		/// </summary>
		public object Value { get; }

		[NotNull]
		public static IndexedEntry Create(bool isFirst, bool isLast, object value, int index) { return new IndexedEntry(isFirst, isLast, value, index); }

		public static object ToObject([NotNull] IndexedEntry value) { return value.Value; }
	}

	/// <inheritdoc />
	public class IndexedEntry<T> : IndexedEntryBase
	{
		protected IndexedEntry(bool isFirst, bool isLast, T value, int index)
			: base(isFirst, isLast, index)
		{
			Value = value;
		}

		/// <summary>
		/// The value of the entry.
		/// </summary>
		public T Value { get; }

		[NotNull]
		public static IndexedEntry<T> Create(bool isFirst, bool isLast, T value, int index) { return new IndexedEntry<T>(isFirst, isLast, value, index); }
	}
}