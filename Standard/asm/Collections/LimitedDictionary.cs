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
	/// <inheritdoc cref="IDictionary" />
	[Serializable]
	public class LimitedDictionary : IDictionary, ILimited
	{
		private int _limit;

		public LimitedDictionary([NotNull] IDictionary source)
		{
			Source = source;
		}

		protected IDictionary Source { get; }

		public int Count => Source.Count;

		/// <inheritdoc />
		public object SyncRoot => Source.SyncRoot;

		/// <inheritdoc />
		public bool IsSynchronized => Source.IsSynchronized;

		/// <inheritdoc />
		public bool IsFixedSize => Source.IsFixedSize;

		public bool IsReadOnly => Source.IsReadOnly;

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
		public virtual bool LimitReached => Limit > 0 && Count >= Limit;

		[Browsable(false)]
		public virtual LimitType LimitReachedBehavior { get; set; }

		/// <inheritdoc />
		public object this[object key]
		{
			get => Source[key];
			set
			{
				if (!Contains(key))
				{
					Add(key, value);
					return;
				}

				Source[key] = value;
			}
		}

		/// <inheritdoc />
		public ICollection Keys => Source.Keys;

		/// <inheritdoc />
		public ICollection Values => Source.Values;

		/// <inheritdoc />
		public IDictionaryEnumerator GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { Source.CopyTo(array, index); }
		
		/// <inheritdoc />
		public bool Contains(object value) { return Source.Contains(value); }

		/// <inheritdoc />
		public void Add(object key, object value)
		{
			if (LimitReached)
			{
				switch (LimitReachedBehavior)
				{
					case LimitType.RemoveFirst:
						if (Count - Limit == 0)
						{
							Source.Remove(Keys.Cast<object>().First());
						}
						else
						{
							object[] keys = Keys.GetRange(0, Count - Limit + 1).Reverse().ToArray();
							
							foreach (object k in keys) 
								Source.Remove(k);
						}

						break;
					case LimitType.SkipAdding:
						return;
					case LimitType.RaiseException:
						throw new LimitReachedException();
				}
			}

			Source.Add(key, value);
		}

		/// <inheritdoc />
		public void Remove(object key) { Source.Remove(key); }

		public void Clear() { Source.Clear(); }
	
		public virtual void Refresh()
		{
			if (!LimitReached || Count == Limit) return;

			switch (LimitReachedBehavior)
			{
				case LimitType.RemoveFirst:
				case LimitType.SkipAdding:
					if (Count - Limit == 1)
					{
						Remove( Keys.Cast<object>().First());
					}
					else
					{
						object[] keys = Keys.GetRange(0, Count - Limit);
						
						foreach (object k in keys) 
							Remove(k);
					}
					break;
				case LimitType.RaiseException:
					throw new LimitReachedException();
			}
		}
	}
	
	/// <inheritdoc cref="IDictionary{TKey,TValue}" />
	/// <inheritdoc cref="LimitedDictionary" />
	[Serializable]
	public class LimitedDictionary<TKey, TValue> : LimitedDictionary, IDictionary<TKey, TValue>, IDictionary, ILimited
	{
		[NonSerialized]
		private IDictionary<TKey, TValue> _source;

		public LimitedDictionary([NotNull] IDictionary<TKey, TValue> source)
			: base(source as IDictionary ?? throw new ArgumentException("Source does not implement IDictionary.", nameof(source)))
		{
		}

		[NotNull]
		protected new IDictionary<TKey, TValue> Source => _source ??= (IDictionary<TKey, TValue>)base.Source;

		/// <inheritdoc />
		public TValue this[TKey key]
		{
			get => Source[key];
			set
			{
				if (!ContainsKey(key))
				{
					Add(key, value);
					return;
				}

				Source[key] = value;
			}
		}

		object IDictionary.this[object key]
		{
			get => this[key];
			set => this[(TKey)key] = (TValue)value;
		}

		/// <inheritdoc />
		public new ICollection<TKey> Keys => Source.Keys;

		/// <inheritdoc />
		public new ICollection<TValue> Values => Source.Values;

		/// <inheritdoc />
		public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return Source.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { Source.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index) { base.CopyTo(array, index); }

		/// <inheritdoc />
		public bool ContainsKey(TKey key) { return Source.ContainsKey(key); }

		/// <inheritdoc />
		public bool Contains(KeyValuePair<TKey, TValue> item) { return Source.Contains(item); }

		/// <inheritdoc />
		bool IDictionary.Contains(object value) { return base.Contains(value); }

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			if (!LimitReached)
			{
				Source.Add(key, value);
				return;
			}

			switch (LimitReachedBehavior)
			{
				case LimitType.RemoveFirst:
					if (Count - Limit == 0) 
						Remove(Keys.First());
					else
					{
						TKey[] keys = Keys.GetRange(0, Count - Limit + 1).Reverse().ToArray();
						
						foreach (TKey k in keys) 
							Remove(k);
					}
					break;
				case LimitType.SkipAdding:
					return;
				case LimitType.RaiseException:
					throw new LimitReachedException();
			}

			Source.Add(key, value);
		}

		/// <inheritdoc />
		public void Add(KeyValuePair<TKey, TValue> item) { Add(item.Key, item.Value); }

		/// <inheritdoc />
		void IDictionary.Add(object key, object value) { Add((TKey)key, (TValue)value); }

		/// <inheritdoc />
		public bool Remove(TKey key) { return Source.Remove(key); }

		/// <inheritdoc />
		public bool Remove(KeyValuePair<TKey, TValue> item) { return Source.Remove(item); }

		/// <inheritdoc />
		void IDictionary.Remove(object key) { base.Remove(key); }

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value) { return Source.TryGetValue(key, out value); }

		public override void Refresh()
		{
			if (!LimitReached || Count == Limit) return;

			switch (LimitReachedBehavior)
			{
				case LimitType.RemoveFirst:
				case LimitType.SkipAdding:
					if (Count - Limit == 1) 
						Remove(Keys.First());
					else
					{
						TKey[] keys = Keys.GetRange(0, Count - Limit).Reverse().ToArray();
						
						foreach (TKey k in keys) 
							Remove(k);
					}
					break;
				case LimitType.RaiseException:
					throw new LimitReachedException();
			}
		}
	}
}