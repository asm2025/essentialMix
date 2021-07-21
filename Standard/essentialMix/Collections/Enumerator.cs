using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Collections
{
	public class Enumerator : Disposable, IEnumerator, IEnumerable
	{
		private IEnumerator _impl;

		public Enumerator() { IsIterable = false; }

		public Enumerator([NotNull] IEnumerable enumerable)
		{
			Enumerable = enumerable;

			switch (enumerable)
			{
				case IReadOnlyCollection<object> readOnlyCollection:
					IsIterable = readOnlyCollection.Count > 0;
					return;
				case ICollection collection:
					IsIterable = collection.Count > 0;
					return;
			}

			IEnumerator im = Enumerable.GetEnumerator();
			IsIterable = im.MoveNext();
		}

		public virtual object Current => Impl.Current;

		public virtual int Position => Index;

		public bool IsIterable { get; protected set; }

		protected IEnumerable Enumerable { get; }

		protected int Index { get; set; } = -1;

		protected bool Done { get; set; }

		protected virtual IEnumerator Impl => _impl ??= Enumerable?.GetEnumerator();

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			Reset();
			return this;
		}

		public virtual bool MoveNext()
		{
			if (Impl == null || !IsIterable || Done) return false;

			if (!Impl.MoveNext())
			{
				Done = true;
				return false;
			}

			Index++;
			return true;
		}

		public virtual void Reset()
		{
			_impl = null;
			Index = -1;
			Done = false;
		}
	}

	public class Enumerator<T> : Disposable, IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable
	{
		private IEnumerator<T> _impl;

		public Enumerator() { IsIterable = false; }

		public Enumerator([NotNull] IEnumerable<T> enumerable)
		{
			switch (enumerable)
			{
				case IReadOnlyCollection<T> readOnlyCollection:
					Enumerable = readOnlyCollection;
					IsIterable = readOnlyCollection.Count > 0;
					break;
				case ICollection<T> collection:
					Enumerable = collection;
					IsIterable = collection.Count > 0;
					break;
				default:
					IList<T> list = enumerable.ToList();
					Enumerable = list;
					IsIterable = list.Count > 0;
					break;
			}
		}

		public virtual T Current => Impl.Current;

		object IEnumerator.Current => Current;

		public virtual int Position { get => Index; set => Index = value; }

		public bool IsIterable { get; protected set; }

		public IEnumerable<T> Enumerable { get; }

		protected int Index { get; set; } = -1;

		protected bool Done { get; set; }

		protected virtual IEnumerator<T> Impl { get { return _impl ??= Enumerable?.GetEnumerator(); } set => _impl = value; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			Reset();
			return this;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public virtual bool MoveNext()
		{
			if (!IsIterable || Done) return false;

			if (!Impl.MoveNext())
			{
				Done = true;
				return false;
			}

			Index++;
			return true;
		}

		public virtual void Reset()
		{
			_impl = null;
			Index = -1;
			Done = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _impl);
			base.Dispose(disposing);
		}

		protected void AssertImpl()
		{
			if (Impl != null) return;
			throw new InvalidOperationException("No implementation available for this operation");
		}
	}
}