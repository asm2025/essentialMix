using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[DebuggerDisplay("{Count}")]
	public class Lister<T> : Enumerator<T>, IReadOnlyList<T>
	{
		private readonly IReadOnlyList<T> _readOnlyList;
		
		private IList<T> _list;

		protected Lister()
			: this(Array.Empty<T>())
		{
		}

		public Lister([NotNull] IEnumerable<T> enumerable)
			: base(enumerable)
		{
			IsIterable = true;

			switch (enumerable)
			{
				case List<T> list:
					_readOnlyList = list;
					Done = true;
					break;
				case IReadOnlyList<T> readOnlyList:
					_readOnlyList = readOnlyList;
					Done = true;
					break;
				case IList<T> list:
					_list = list;
					_readOnlyList = new ReadOnlyList<T>(_list);
					Done = true;
					break;
				default:
					if (Enumerable is IList<T> enumerableList)
					{
						_list = enumerableList;
						Done = true;
					}
					else
					{
						_list = new List<T>();
					}
					_readOnlyList = new ReadOnlyList<T>(_list);
					break;
			}
		}

		public override T Current => Item(Index);

		[IndexerName("ItemOf")]
		public T this[int i] => Item(i);

		public virtual int Count
		{
			get
			{
				if (Done) return _readOnlyList.Count;
				int n = Index;
				MoveTo(int.MaxValue);
				Reset();
				Index = n;
				return _readOnlyList.Count;
			}
		}

		public bool HasMany => Count > 1;

		public bool HasOne => Count == 1;

		[NotNull]
		protected IList<T> List => _list ??= new List<T>();

		public virtual T Item(int index)
		{
			if (_readOnlyList.Count <= index) MoveTo(index);
			return _readOnlyList[index];
		}

		public override bool MoveNext()
		{
			if (!IsIterable) return false;
			if (!Done) return MoveTo((Index + 1).NotBelow(0)) > 0;

			if (Index < 0)
			{
				if (_readOnlyList.Count > 0) Index = 0;
				return Index == 0;
			}

			if (Index >= _readOnlyList.Count - 1) return false;
			Index++;
			return true;
		}

		public override void Reset()
		{
			if (!Done)
			{
				Impl = null;
				List.Clear();
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

			while (!Done && List.Count <= index)
			{
				if (!Impl.MoveNext())
				{
					Done = true;
					return n;
				}

				List.Add(Impl.Current);

				if (Index < 0) Index = 0;
				else Index++;

				n++;
			}

			return n;
		}
	}

	public class Lister : Lister<object>
	{
		protected Lister()
			: this(Array.Empty<object>())
		{
		}

		public Lister([NotNull] IEnumerable enumerable)
			: base(enumerable.Cast<object>())
		{
		}
	}
}