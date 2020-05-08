using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <inheritdoc cref="ICollection" />
	/// <inheritdoc cref="ICollection{T}" />
	[Serializable]
	public class ProtectableCollection<T> : ICollection<T>, ICollection, IProtectable
	{
		[NonSerialized] private ICollection _collection;

		public ProtectableCollection([NotNull] ICollection<T> source)
		{
			Source = source;
		}

		[NotNull]
		protected ICollection<T> Source { get; }

		[NotNull]
		protected ICollection SourceCollection => _collection ??= (ICollection)Source;

		public int Count => Source.Count;

		/// <inheritdoc />
		public bool IsReadOnly => Source.IsReadOnly;

		/// <inheritdoc />
		public object SyncRoot => SourceCollection.SyncRoot;

		/// <inheritdoc />
		public bool IsSynchronized => SourceCollection.IsSynchronized;

		/// <inheritdoc />
		public bool IsProtected { get; set; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }
		void ICollection.CopyTo(Array array, int index) { SourceCollection.CopyTo(array, index); }

		/// <inheritdoc />
		public bool Contains(T item) { return Source.Contains(item); }

		/// <inheritdoc />
		public void Add(T item)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Add(item);
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (IsProtected) throw new CollectionLockedException();
			return Source.Remove(item);
		}

		/// <inheritdoc />
		public void Clear()
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Clear();
		}
	}
}