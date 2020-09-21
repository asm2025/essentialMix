using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;
using asm.Patterns.Object;

namespace asm.Data.Xml
{
	public class XNodeLister : Disposable, IReadOnlyList<XNode>, IEnumerator<XNode>, IEnumerator
	{
		private readonly XPathNodeIterator _iterator;
		private readonly IList<XNode> _list;

		private XPathNodeIterator _impl;
		private bool _done;

		public XNodeLister([NotNull] XPathNodeIterator iterator)
		{
			_iterator = iterator;
			_impl = _iterator.Clone();
			_list = new List<XNode>(_impl.Count);
			IsIterable = _impl.Count > 0;
		}

		public int Index { get; set; } = -1;

		public bool IsIterable { get; }

		public int Count =>
			IsIterable
				? _iterator.Count
				: 0;

		public bool HasMany => Count > 1;

		public bool HasOne => Count == 1;

		[IndexerName("ItemOf")]
		public XNode this[int index] => Item(index);

		/// <inheritdoc />
		public XNode Current => Item(Index);

		/// <inheritdoc />
		object IEnumerator.Current => Current;

		public XNode Item(int index)
		{
			if (_list.Count <= index) MoveTo(index);
			if (!Index.InRangeRx(0, _list.Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return _list[index];
		}

		public IEnumerator<XNode> GetEnumerator()
		{
			Reset();
			return this;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public bool MoveNext()
		{
			AssertImpl();
			if (!IsIterable || _done) return false;
			if (!_done) return MoveTo((Index + 1).NotBelow(0)) > 0;

			if (Index < 0)
			{
				if (_list.Count > 0) Index = 0;
				return Index == 0;
			}

			if (Index >= _list.Count - 1) return false;
			Index++;
			return true;
		}

		public void Reset()
		{
			_impl = _iterator.Clone();
			Index = -1;
			_done = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _impl);
			base.Dispose(disposing);
		}

		private int MoveTo(int index)
		{
			if (!IsIterable || _done) return 0;
			AssertImpl();

			int n = 0;

			while (!_done && _list.Count <= index)
			{
				if (!_impl.MoveNext() || _impl.Current == null)
				{
					_done = true;
					return n;
				}

				_list.Add(_impl.Current.GetNode().ToXNode());

				if (Index < 0) Index = 0;
				else Index++;

				n++;
			}

			return n;
		}

		private void AssertImpl()
		{
			if (_impl != null) return;
			throw new InvalidOperationException("No implementation available for this operation");
		}
	}
}