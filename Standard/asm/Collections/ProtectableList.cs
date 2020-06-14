using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <inheritdoc cref="IList" />
	[Serializable]
	public class ProtectableList : IList, IProtectable
	{
		public ProtectableList([NotNull] IList source)
		{
			Source = source;
		}

		[NotNull]
		protected IList Source { get; }

		/// <inheritdoc />
		public int Count => Source.Count;

		/// <inheritdoc />
		public object SyncRoot => Source.SyncRoot;

		/// <inheritdoc />
		public bool IsSynchronized => Source.IsSynchronized;

		/// <inheritdoc />
		public bool IsReadOnly => Source.IsReadOnly;

		/// <inheritdoc />
		public bool IsFixedSize => Source.IsFixedSize;

		/// <inheritdoc />
		public bool IsProtected { get; set; }

		/// <inheritdoc />
		public object this[int index]
		{
			get => Source[index];
			set
			{
				if (IsProtected) throw new CollectionLockedException();
				Source[index] = value;
			}
		}

		/// <inheritdoc />
		public IEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }

		/// <inheritdoc />
		public bool Contains(object value) { return Source.Contains(value); }

		/// <inheritdoc />
		public int IndexOf(object value) { return Source.IndexOf(value); }

		/// <inheritdoc />
		public int Add(object value)
		{
			if (IsProtected) throw new CollectionLockedException();
			return Source.Add(value);
		}

		/// <inheritdoc />
		public void Insert(int index, object value)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Insert(index, value);
		}

		/// <inheritdoc />
		public void Remove(object value)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Remove(value);
		}

		/// <inheritdoc />
		public void RemoveAt(int index)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.RemoveAt(index);
		}

		/// <inheritdoc />
		public void Clear()
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Clear();
		}
	}

	/// <inheritdoc cref="IList{T}" />
	/// <inheritdoc cref="ProtectableList" />
	[Serializable]
	public class ProtectableList<T> : ProtectableList, IList<T>, IList
	{
		[NonSerialized]
		private readonly IList<T> _source;

		/// <inheritdoc cref="ProtectableList" />
		/// <inheritdoc cref="IList{T}" />
		public ProtectableList([NotNull] IList<T> source)
			: base(source as IList ?? throw new ArgumentException("Source does not implement IList.", nameof(source)))
		{
			_source = (IList<T>)base.Source;
		}

		[NotNull]
		protected new IList<T> Source => _source;

		/// <inheritdoc />
		public new T this[int index]
		{
			get => Source[index];
			set
			{
				if (IsProtected) throw new CollectionLockedException();
				Source[index] = value;
			}
		}

		object IList.this[int index]
		{
			get => this[index];
			set => this[index] = (T)value;
		}

		/// <inheritdoc />
		public new IEnumerator<T> GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }
		void ICollection.CopyTo(Array array, int index) { base.CopyTo(array, index); }

		/// <inheritdoc />
		public bool Contains(T item) { return Source.Contains(item); }

		bool IList.Contains(object value) { return base.Contains(value); }

		/// <inheritdoc />
		public int IndexOf(T item) { return Source.IndexOf(item); }

		int IList.IndexOf(object value) { return base.IndexOf(value); }

		/// <inheritdoc />
		public void Add(T item)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Add(item);
		}

		int IList.Add(object value) { return base.Add(value); }

		/// <inheritdoc />
		public void Insert(int index, T item)
		{
			if (IsProtected) throw new CollectionLockedException();
			Source.Insert(index, item);
		}
		void IList.Insert(int index, object value) { base.Insert(index, value); }

		/// <inheritdoc />
		public bool Remove(T item)
		{
			if (IsProtected) throw new CollectionLockedException();
			return Source.Remove(item);
		}
		void IList.Remove(object value) { base.Remove(value); }
	}
}