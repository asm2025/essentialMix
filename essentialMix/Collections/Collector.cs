using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("Count = {Count}")]
public class Collector<T> : Enumerator<T>, IReadOnlyCollection<T>
{
	private readonly IReadOnlyCollection<T> _readOnlyCollection;
		
	private ICollection<T> _collection;

	protected Collector()
		: this(Array.Empty<T>())
	{
	}

	public Collector([NotNull] IEnumerable<T> enumerable)
		: base(enumerable)
	{
		IsIterable = true;

		switch (enumerable)
		{
			case List<T> list:
				_readOnlyCollection = list;
				Done = true;
				break;
			case IReadOnlyList<T> readOnlyList:
				_readOnlyCollection = readOnlyList;
				Done = true;
				break;
			case IList<T> list:
				_collection = list;
				_readOnlyCollection = new ReadOnlyCollection<T>(list);
				Done = true;
				break;
			case IReadOnlyCollection<T> readOnlyCollection:
				_readOnlyCollection = readOnlyCollection;
				Done = true;
				break;
			default:
				if (Enumerable is ICollection<T> enumerableCollection)
				{
					_collection = enumerableCollection;
					Done = true;
				}
				else
				{
					_collection = new List<T>();
				}
				_readOnlyCollection = new ReadOnlyCollection<T>(_collection as IList<T> ?? _collection.ToList());
				break;
		}
	}

	public virtual int Count
	{
		get
		{
			if (Done) return _readOnlyCollection.Count;
			int n = Index;
			MoveTo(int.MaxValue);
			Reset();
			Index = n;
			return _readOnlyCollection.Count;
		}
	}

	public bool HasMany => Count > 1;

	public bool HasOne => Count == 1;

	[NotNull]
	protected ICollection<T> Collection => _collection ??= new Collection<T>();

	public override bool MoveNext()
	{
		if (!IsIterable) return false;
		if (!Done) return MoveTo((Index + 1).NotBelow(0)) > 0;

		if (Index < 0)
		{
			if (_readOnlyCollection.Count > 0) Index = 0;
			return Index == 0;
		}

		if (Index >= _readOnlyCollection.Count - 1) return false;
		Index++;
		return true;
	}

	public override void Reset()
	{
		if (!Done)
		{
			Impl = null;
			Collection.Clear();
			base.Reset();
			return;
		}

		base.Reset();
		Done = true;
	}

	protected virtual int MoveTo(int index)
	{
		if (!IsIterable || Done) return 0;
		AssertImpl();

		int n = 0;

		while (!Done && Collection.Count <= index)
		{
			if (!Impl.MoveNext())
			{
				Done = true;
				return n;
			}

			Collection.Add(Impl.Current);

			if (Index < 0) Index = 0;
			else Index++;

			n++;
		}

		return n;
	}
}

public class Collector([NotNull] IEnumerable enumerable) : Collector<object>(enumerable.Cast<object>())
{
	protected Collector()
		: this(Array.Empty<object>())
	{
	}
}