using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using asm.Data.Extensions;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Data.Xml
{
	public sealed class XNodeEnumerator : Disposable, IEnumerator<XNode>, IEnumerator, IEnumerable<XNode>, IEnumerable
	{
		private readonly XPathNodeIterator _iterator;

		private XPathNodeIterator _impl;
		private bool _done;

		public XNodeEnumerator([NotNull] XPathNodeIterator iterator)
		{
			_iterator = iterator;
			_impl = _iterator.Clone();
			IsIterable = _impl.Count > 0;
		}

		public bool IsIterable { get; }

		[NotNull]
		public XNode Current => _impl.Current?.GetNode().ToXNode() ?? throw new InvalidOperationException();

		[NotNull]
		object IEnumerator.Current => Current;

		/// <inheritdoc />
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

			if (!_impl.MoveNext())
			{
				_done = true;
				return false;
			}

			return true;
		}

		public void Reset()
		{
			_impl = _iterator.Clone();
			_done = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _impl);
			base.Dispose(disposing);
		}

		private void AssertImpl()
		{
			if (_impl != null) return;
			throw new InvalidOperationException("No implementation available for this operation");
		}
	}
}