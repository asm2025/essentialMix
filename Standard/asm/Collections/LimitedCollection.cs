using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Exceptions.Collections;

namespace asm.Collections
{
	/// <inheritdoc cref="ICollection" />
	/// <inheritdoc cref="ICollection{T}" />
	[Serializable]
	public class LimitedCollection<T> : ICollection<T>, ICollection, ILimited
	{
		private int _limit;

		[NonSerialized] private ICollection _collection;

		public LimitedCollection([NotNull] ICollection<T> source)
		{
			Source = source;
		}

		[NotNull]
		protected ICollection<T> Source { get; }

		[NotNull]
		protected ICollection SourceCollection => _collection ??= Source as ICollection ?? throw new InvalidCastException($"{nameof(Source)} does not implement ICollection.");

		public int Count => Source.Count;

		/// <inheritdoc />
		public bool IsReadOnly => Source.IsReadOnly;

		/// <inheritdoc />
		public object SyncRoot => SourceCollection.SyncRoot;

		/// <inheritdoc />
		public bool IsSynchronized => SourceCollection.IsSynchronized;

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
			if (LimitReached)
			{
				switch (LimitReachedBehavior)
				{
					case LimitType.RemoveFirst:
						if (Count - Limit == 0)
							Source.Remove(Source.First());
						else
							Source.RemoveRange(0, Count - Limit);
						break;
					case LimitType.SkipAdding:
						return;
					case LimitType.RaiseException:
						throw new LimitReachedException();
				}
			}

			Source.Add(item);
		}

		/// <inheritdoc />
		public bool Remove(T item) { return Source.Remove(item); }

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
					Source.RemoveRange(0, Count - Limit);
					break;
				case LimitType.RaiseException:
					if (Count > Limit) throw new LimitReachedException();
					break;
			}
		}
	}
}