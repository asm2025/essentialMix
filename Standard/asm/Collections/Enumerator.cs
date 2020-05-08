using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Collections
{
	public class Enumerator : Disposable, IEnumerator, IEnumerable
	{
		private readonly IEnumerable _enumerable;
		private IEnumerator _impl;

		public Enumerator() { IsIterable = false; }

		public Enumerator([NotNull] IEnumerable enumerable)
		{
			_enumerable = enumerable;

			switch (enumerable)
			{
				case ICollection collection:
					IsIterable = collection.Count > 0;
					return;
				case IReadOnlyCollection<object> readOnlyCollection:
					IsIterable = readOnlyCollection.Count > 0;
					return;
			}

			IEnumerator im = _enumerable.GetEnumerator();
			IsIterable = im.MoveNext();
			ObjectHelper.Dispose(ref im);
		}

		public virtual object Current => Impl.Current;

		public virtual int Position => Index;

		public bool IsIterable { get; protected set; }
		protected int Index { get; set; } = -1;
		protected bool Done { get; set; }

		protected virtual IEnumerator Impl => _impl ??= _enumerable?.GetEnumerator();

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			Reset();
			return this;
		}

		public virtual bool MoveNext()
		{
			AssertImpl();
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
			if (_impl != null) return;
			throw new InvalidOperationException("No implementation available for this operation");
		}
	}

	public class Enumerator<T> : Disposable, IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable
	{
		private readonly IEnumerable<T> _enumerable;
		private IEnumerator<T> _impl;

		public Enumerator() { IsIterable = false; }

		public Enumerator([NotNull] IEnumerable<T> enumerable)
		{
			_enumerable = enumerable;

			switch (enumerable)
			{
				case ICollection<T> collection:
					IsIterable = collection.Count > 0;
					return;
				case IReadOnlyCollection<T> readOnlyCollection:
					IsIterable = readOnlyCollection.Count > 0;
					return;
			}

			using (IEnumerator<T> im = _enumerable.GetEnumerator())
				IsIterable = im.MoveNext();
		}

		public virtual T Current => Impl.Current;

		object IEnumerator.Current => Current;

		public virtual int Position { get => Index; set => Index = value; }

		public bool IsIterable { get; protected set; }
		protected int Index { get; set; } = -1;
		protected bool Done { get; set; }

		protected virtual IEnumerator<T> Impl
		{
			get { return _impl ??= _enumerable?.GetEnumerator(); }
			set => _impl = value;
		}

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