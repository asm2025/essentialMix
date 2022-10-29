using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("{Value}, Count = {Count}")]
[Serializable]
public class Tree<T> : KeyedDictionaryBase<T, Tree<T>>
{
	/// <inheritdoc />
	public Tree(T value)
		: this(value, (IEqualityComparer<T>)null)
	{
	}

	/// <inheritdoc />
	public Tree(T value, IEqualityComparer<T> comparer)
		: base(comparer)
	{
		Value = value;
	}

	/// <inheritdoc />
	public Tree(T value, [NotNull] IEnumerable<Tree<T>> collection)
		: this(value, collection, null)
	{
	}

	/// <inheritdoc />
	public Tree(T value, [NotNull] IEnumerable<Tree<T>> collection, IEqualityComparer<T> comparer)
		: base(collection, comparer)
	{
		Value = value;
	}

	public T Value { get; set; }

	public bool IsLeaf => Count == 0;

	/// <inheritdoc />
	protected override T GetKeyForItem(Tree<T> item) { return Value; }
}

[DebuggerDisplay("{Key} [{Value}], Count = {Count}")]
[Serializable]
public class Tree<TKey, TValue> : KeyedDictionaryBase<TKey, Tree<TKey, TValue>>
{
	/// <inheritdoc />
	public Tree([NotNull] TKey key)
		: this(key, (IEqualityComparer<TKey>)null)
	{
	}

	/// <inheritdoc />
	public Tree([NotNull] TKey key, IEqualityComparer<TKey> comparer)
		: base(comparer)
	{
		Key = key;
	}

	/// <inheritdoc />
	public Tree([NotNull] TKey key, [NotNull] IEnumerable<Tree<TKey, TValue>> collection)
		: this(key, collection, null)
	{
	}

	/// <inheritdoc />
	public Tree([NotNull] TKey key, [NotNull] IEnumerable<Tree<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
		: base(collection, comparer)
	{
		Key = key;
	}

	public TKey Key { get; set; }

	public TValue Value { get; set; }

	public bool IsLeaf => Count == 0;

	/// <inheritdoc />
	protected override TKey GetKeyForItem(Tree<TKey, TValue> item) { return item.Key; }
}