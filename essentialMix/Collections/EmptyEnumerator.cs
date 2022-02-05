using System;
using System.Collections;
using System.Collections.Generic;

namespace essentialMix.Collections;

/// <summary>
/// This is a dummy empty IEnumerator{T} when an empty one is needed.
/// yield break can be used to force the compiler to treat it as an iterator block.
/// But it's less efficient than a custom empty iterator.
/// See <see href="https://stackoverflow.com/questions/1714351/return-an-empty-ienumerator#1714395">here</see>
/// and <see href="https://stackoverflow.com/questions/1714351/return-an-empty-ienumerator#1714391">here</see>
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct EmptyEnumerator<T> : IEnumerator<T>, IEnumerator
{
	/// <inheritdoc />
	public T Current => throw new InvalidOperationException();

	/// <inheritdoc />
	object IEnumerator.Current => Current;

	/// <inheritdoc />
	public bool MoveNext() { return false; }

	/// <inheritdoc />
	void IEnumerator.Reset() { }

	/// <inheritdoc />
	public void Dispose() { }
}

/// <summary>
/// This is a dummy empty IEnumerator when an empty one is needed.
/// yield break can be used to force the compiler to treat it as an iterator block.
/// But it's less efficient than a custom empty iterator.
/// See <see href="https://stackoverflow.com/questions/1714351/return-an-empty-ienumerator#1714395">here</see>
/// and <see href="https://stackoverflow.com/questions/1714351/return-an-empty-ienumerator#1714391">here</see>
/// </summary>
public readonly struct EmptyEnumerator : IEnumerator, IDisposable
{
	/// <inheritdoc />
	object IEnumerator.Current => throw new InvalidOperationException();

	/// <inheritdoc />
	public bool MoveNext() { return false; }

	/// <inheritdoc />
	void IEnumerator.Reset() { }

	/// <inheritdoc />
	public void Dispose() { }
}