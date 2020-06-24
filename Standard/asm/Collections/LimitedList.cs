using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <inheritdoc cref="IList" />
	[Serializable]
	public class LimitedList : IList, ILimited
	{
		private int _limit;

		public LimitedList([NotNull] IList source)
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
		public object this[int index]
		{
			get => Source[index];
			set => Source[index] = value;
		}

		[Browsable(false)]
		public virtual int Limit
		{
			get => _limit;
			set
			{
				_limit = value;
				Refresh();
			}
		}

		[Browsable(false)]
		public bool LimitReached => Limit > 0 && Count >= Limit;

		[Browsable(false)]
		public virtual LimitType LimitReachedBehavior { get; set; }

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
			if (LimitReached)
			{
				switch (LimitReachedBehavior)
				{
					case LimitType.RemoveFirst:
						if (Count - Limit == 0)
						{
							Source.Remove(Source.Cast<object>().First());
						}
						else
						{
							int max = Count - Limit;

							for (int i = max; i >= 0; i--) 
								Source.RemoveAt(i);
						}

						break;
					case LimitType.SkipAdding:
						return -1;
					case LimitType.RaiseException:
						throw new LimitReachedException();
				}
			}

			return Source.Add(value);
		}

		/// <inheritdoc />
		public void Insert(int index, object value)
		{
			Source.Insert(index, value);
			Refresh();
		}

		/// <inheritdoc />
		public void Remove(object value)
		{
			Source.Remove(value);
		}

		/// <inheritdoc />
		public void RemoveAt(int index)
		{
			Source.RemoveAt(index);
		}

		/// <inheritdoc />
		public void Clear()
		{
			Source.Clear();
		}

		public virtual void Refresh()
		{
			if (!LimitReached) return;
			
			switch (LimitReachedBehavior)
			{
				case LimitType.RemoveFirst:
				case LimitType.SkipAdding:
					while (Count > Limit) RemoveAt(0);
					break;
				case LimitType.RaiseException:
					if (Count > Limit) throw new LimitReachedException();
					break;
			}
		}
	}

	/// <inheritdoc cref="IList{T}" />
	[Serializable]
	public class LimitedList<T> : LimitedList, IList<T>, IList
	{
		[NonSerialized]
		private IList<T> _source;

		/// <inheritdoc cref="IList{T}" />
		/// <inheritdoc cref="LimitedList" />
		public LimitedList([NotNull] IList<T> source)
			: base(source as IList ?? throw new ArgumentException("Source does not implement IList.", nameof(source)))
		{
		}

		[NotNull]
		protected new IList<T> Source => _source ??= (IList<T>)base.Source;

		/// <inheritdoc />
		public new T this[int index]
		{
			get => Source[index];
			set => Source[index] = value;
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
			if (!LimitReached)
			{
				Source.Add(item);
				return;
			}

			switch (LimitReachedBehavior)
			{
				case LimitType.RemoveFirst:
					while (Count >= Limit) 
						Source.RemoveAt(0);
					break;
				case LimitType.SkipAdding:
					return;
				case LimitType.RaiseException:
					throw new LimitReachedException();
			}

			Source.Add(item);
		}

		int IList.Add(object value) { return base.Add(value); }

		/// <inheritdoc />
		public void Insert(int index, T item)
		{
			Source.Insert(index, item);
			Refresh();
		}
		void IList.Insert(int index, object value) { base.Insert(index, value); }

		/// <inheritdoc />
		public bool Remove(T item) { return Source.Remove(item); }

		void IList.Remove(object value) { base.Remove(value); }
	}
}