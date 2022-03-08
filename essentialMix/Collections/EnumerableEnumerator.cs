using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Collections;

public static class EnumerableEnumerator
{
	private class Enumerator<T> : IEnumerableEnumerator<T>
	{
		/// <inheritdoc />
		public T Current => default(T);

		/// <inheritdoc />
		object IEnumerator.Current => Current;

		/// <inheritdoc />
		public void Dispose() { }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Enumerable.Empty<T>().GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public bool MoveNext() { return false; }

		/// <inheritdoc />
		void IEnumerator.Reset() { }
	}

	[NotNull]
	public static IEnumerableEnumerator<T> Empty<T>() { return new Enumerator<T>(); }
}