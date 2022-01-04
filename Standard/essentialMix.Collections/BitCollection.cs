using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <summary>
/// A collection of bit flags from 0 to <see cref="Maximum"/> inclusive.
/// </summary>
[ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class BitCollection : ICollection<uint>, ICollection, IReadOnlyCollection<uint>, IEnumerable<uint>, IEnumerable
{
	private struct Enumerator : IEnumerator<uint>, IEnumerator, IDisposable
	{
		private readonly BitCollection _bitCollection;
		private readonly int _version;
		private int _index;
		private uint _current;

		internal Enumerator([NotNull] BitCollection bitCollection)
		{
			_bitCollection = bitCollection;
			_version = bitCollection._version;
			_index = 0;
			_current = 0u;
		}

		/// <inheritdoc />
		public uint Current
		{
			get
			{
				if (!_index.InRange(0, _bitCollection.Count)) throw new InvalidOperationException();
				return _current;
			}
		}

		/// <inheritdoc />
		object IEnumerator.Current => Current;

		/// <inheritdoc />
		public void Dispose() { }

		/// <inheritdoc />
		public bool MoveNext()
		{
			if (_version == _bitCollection._version && _index < _bitCollection.Count)
			{
				while (_index < _bitCollection.Count && _current < _bitCollection.Maximum)
				{
					_current++;
					if (!_bitCollection.Contains(_current)) continue;
					_index++;
					return true;
				}
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _bitCollection._version) throw new VersionChangedException();
			_index = _bitCollection.Count + 1;
			_current = 0u;
			return false;
		}

		/// <inheritdoc />
		void IEnumerator.Reset()
		{
			if (_version != _bitCollection._version) throw new InvalidOperationException();
			_index = 0;
			_current = 0u;
		}
	}

	//32 bits for each int. bytes: 4, binary: 11111 (+1), mask: 0x1f, power: 2^5
	private int[] _flags;
	private uint _maximum;
	private int _version;

	[NonSerialized]
	private object _syncRoot;

	public BitCollection(uint maximum)
	{
		_maximum = maximum;
		_flags = new int[GetCapacity(maximum)];
	}

	/// <summary>
	/// The maximum value allowed in the collection
	/// </summary>
	public uint Maximum
	{
		get => _maximum;
		set
		{
			if (value < Count || value < 1) throw new ArgumentOutOfRangeException(nameof(value));
			if (value == _maximum) return;
			_maximum = value;
			int[] newItems = new int[GetCapacity(_maximum)];
			Array.Copy(_flags, newItems, _flags.Length);
			_flags = newItems;
			_version++;
		}
	}

	/// <inheritdoc cref="ICollection" />
	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	/// <inheritdoc />
	public bool IsReadOnly => false;

	/// <inheritdoc />
	bool ICollection.IsSynchronized => false;

	/// <inheritdoc />
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	/// <inheritdoc />
	public IEnumerator<uint> GetEnumerator() { return new Enumerator(this); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	/// <inheritdoc />
	public void Add(uint item)
	{
		if (!item.InRange(0u, Maximum)) throw new ArgumentOutOfRangeException(nameof(item));
		(uint index, int offset) = GetIndexOffset(item);
		if (Contains(index, offset)) return;
		_flags[index] |= 1 << offset;
		Count++;
		_version++;
	}

	/// <inheritdoc />
	public bool Remove(uint item)
	{
		if (!item.InRange(0u, Maximum)) throw new ArgumentOutOfRangeException(nameof(item));
		(uint index, int offset) = GetIndexOffset(item);
		if (!Contains(index, offset)) return false;
		_flags[index] &= ~(1 << offset);
		Count--;
		_version++;
		return true;
	}

	public void Toggle(uint item)
	{
		if (!item.InRange(0u, Maximum)) throw new ArgumentOutOfRangeException(nameof(item));
		(uint index, int offset) = GetIndexOffset(item);
		_flags[index] ^= 1 << offset;
		Count += Contains(index, offset)
					? 1
					: -1;
		_version++;
	}

	/// <inheritdoc />
	public void Clear()
	{
		Array.Clear(_flags, 0, _flags.Length);
		Count = 0;
		_version++;
	}

	/// <inheritdoc />
	public bool Contains(uint item)
	{
		if (!item.InRange(0u, Maximum)) throw new ArgumentOutOfRangeException(nameof(item));
		(uint index, int offset) = GetIndexOffset(item);
		return Contains(index, offset);
	}

	/// <inheritdoc />
	public void CopyTo(uint[] array, int arrayIndex)
	{
		if (Count == 0) return;
		array.Length.ValidateRange(arrayIndex, Count);

		foreach (uint value in this)
		{
			array[arrayIndex++] = value;
		}
	}

	/// <inheritdoc />
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array.Rank != 1) throw new RankException();
		if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
		if (Count == 0) return;

		if (array is uint[] tArray)
		{
			CopyTo(tArray, arrayIndex);
			return;
		}

		//
		// Catch the obvious case assignment will fail.
		// We can found all possible problems by doing the check though.
		// For example, if the element type of the Array is derived from T,
		// we can't figure out if we can successfully copy the element beforehand.
		//
		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(uint);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));
	
		try
		{
			foreach (uint value in this)
			{
				objects[arrayIndex++] = value;
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Invalid array type", nameof(array));
		}
	}

	/// <summary>
	/// Creates and returns a new array containing the elements in this deque.
	/// </summary>
	[NotNull]
	public uint[] ToArray()
	{
		uint[] result = new uint[Count];
		CopyTo(result, 0);
		return result;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private bool Contains(uint index, int offset)
	{
		return ((_flags[index] >> offset) & 1) == 1;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static (uint Index, int Offset) GetIndexOffset(uint n)
	{
		return (n >> 5, (int)(n & 0b11111));
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private static int GetCapacity(uint n)
	{
		return (int)((n >> 5) + 1);
	}
}