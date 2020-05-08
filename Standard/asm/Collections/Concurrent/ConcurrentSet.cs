using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace asm.Collections.Concurrent
{
	[DebuggerDisplay("Count = {Count}")]
	[Serializable]
	public class ConcurrentSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
	{
		private ConcurrentDictionary<T, byte> _dictionary;

		[NonSerialized] 
		private SerializationInfo _siInfo;

		/// <inheritdoc />
		public ConcurrentSet()
			: this(null, null)
		{
		}

		/// <inheritdoc />
		public ConcurrentSet(IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public ConcurrentSet(IEqualityComparer<T> comparer)
			: this(null, comparer)
		{
		}

		public ConcurrentSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
			_dictionary = collection == null
							? new ConcurrentDictionary<T, byte>(Comparer)
							: new ConcurrentDictionary<T, byte>(collection.Select(e => new KeyValuePair<T, byte>(e, 0)), Comparer);
		}

		protected ConcurrentSet(SerializationInfo info, StreamingContext context)
		{
			_siInfo = info;
		}

		public IEqualityComparer<T> Comparer { get; private set; }

		/// <inheritdoc />
		public bool IsReadOnly { get; } = false;

		public int Count => _dictionary.Count;

		/// <inheritdoc />
		[SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		[SecurityCritical]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof (info));
			info.AddValue("Comparer", Comparer, typeof (IEqualityComparer<T>));
			T[] array = _dictionary?.Keys.ToArray();
			if (array == null || array.Length == 0) return;
			info.AddValue("Elements", array, typeof (T[]));
		}

		/// <inheritdoc />
		public virtual void OnDeserialization(object sender)
		{
			if (_siInfo == null) return;
			Comparer = (IEqualityComparer<T>) _siInfo.GetValue("Comparer", typeof (IEqualityComparer<T>));
			T[] array = (T[])_siInfo.GetValue("Elements", typeof(T[]));
			_dictionary = array != null && array.Length > 0
							? new ConcurrentDictionary<T, byte>(array.Select(e => new KeyValuePair<T, byte>(e, 0)), Comparer)
							: new ConcurrentDictionary<T, byte>(Comparer);
			_siInfo = null;
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return _dictionary.Keys.GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc />
		public bool Add(T item)
		{
			return _dictionary.TryAdd(item, 0);
		}

		/// <inheritdoc />
		void ICollection<T>.Add(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			Add(item);
		}
		
		public bool Remove(T item)
		{
			return item != null && _dictionary.TryRemove(item, out _);
		}

		/// <inheritdoc />
		public void Clear()
		{
			_dictionary.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T item)
		{
			return item != null && _dictionary.ContainsKey(item);
		}

		public void CopyTo([NotNull] T[] array)
		{
			CopyTo(array, 0);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			_dictionary.Keys.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc />
		public void UnionWith(IEnumerable<T> other)
		{
			foreach (T item in other) 
				Add(item);
		}

		/// <inheritdoc />
		public void IntersectWith(IEnumerable<T> other)
		{
			if (Count == 0 || ReferenceEquals(other, this)) return;

			switch (other)
			{
				case ICollection<T> collectionT:
					if (collectionT.Count == 0)
					{
						Clear();
						return;
					}

					foreach (T item in _dictionary.Keys.ToArray())
					{
						if (collectionT.Contains(item)) continue;
						Remove(item);
					}
					break;
				case IReadOnlyCollection<T> collectionTR:
					if (collectionTR.Count == 0)
					{
						Clear();
						return;
					}

					foreach (T item in _dictionary.Keys.ToArray())
					{
						if (collectionTR.Contains(item)) continue;
						Remove(item);
					}
					break;
				default:
					ICollection<T> collection = other.ToArray();
					
					if (collection.Count == 0)
					{
						Clear();
						return;
					}

					foreach (T item in _dictionary.Keys.ToArray())
					{
						if (collection.Contains(item)) continue;
						Remove(item);
					}
					break;
			}
		}

		/// <inheritdoc />
		public void ExceptWith(IEnumerable<T> other)
		{
			foreach (T item in other)
				Remove(item);
		}

		/// <inheritdoc />
		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			if (Count == 0)
			{
				UnionWith(other);
			}
			else if (ReferenceEquals(this, other))
			{
				Clear();
			}
			else
			{
				foreach (T item in other)
				{
					if (Remove(item)) continue;
					Add(item);
				}
			}
		}

		/// <inheritdoc />
		public bool IsSubsetOf(IEnumerable<T> other)
		{
			ICollection<T> collection = other as ICollection<T> ?? other.ToArray();
			return _dictionary.Keys.All(collection.Contains);
		}

		/// <inheritdoc />
		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			ICollection<T> collection = other as ICollection<T> ?? other.ToArray();
			return Count != collection.Count && IsSubsetOf(collection);
		}

		/// <inheritdoc />
		public bool IsSupersetOf(IEnumerable<T> other)
		{
			return other.All(_dictionary.Keys.Contains);
		}

		/// <inheritdoc />
		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			ICollection<T> collection = other as ICollection<T> ?? other.ToArray();
			return Count != collection.Count && IsSupersetOf(collection);
		}

		/// <inheritdoc />
		public bool Overlaps(IEnumerable<T> other)
		{
			return other.Any(_dictionary.Keys.Contains);
		}

		/// <inheritdoc />
		public bool SetEquals(IEnumerable<T> other)
		{
			ICollection<T> collection = other as ICollection<T> ?? other.ToArray();
			return Count == collection.Count && collection.All(_dictionary.Keys.Contains);
		}

		public int RemoveWhere([NotNull] Predicate<T> match)
		{
			return _dictionary.Keys
							.Where(e => match(e))
							.ToArray()
							.Count(Remove);
		}
	}
}
