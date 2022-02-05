// Copyright (c) 2016 Tyler Brinkley
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Other.TylerBrinkley.Collections;

/// <summary>
/// An <see cref="T:global::System.Attribute" /> collection.
/// </summary>
/// <seealso cref="T:global::System.Attribute" />
/// <seealso cref="T:global::System.Attribute" />
public sealed class AttributeCollection : IList<Attribute>, IReadOnlyList<Attribute>
{
	private readonly Attribute[] _attributes;

	/// <summary>
	/// The <see cref="AttributeCollection"/> indexer. 
	/// </summary>
	/// <param name="index">The index of the <see cref="Attribute"/> to retrieve.</param>
	/// <returns>The <see cref="Attribute"/> at the specified <paramref name="index"/></returns>
	public Attribute this[int index] => _attributes[index];

	/// <summary>
	/// The number of <see cref="Attribute"/>s.
	/// </summary>
	public int Count => _attributes.Length;

	internal AttributeCollection(Attribute[] attributes) { _attributes = attributes; }

	/// <summary>
	/// Indicates if the collection contains a <typeparamref name="TAttribute"/>.
	/// </summary>
	/// <typeparam name="TAttribute">The attribute type.</typeparam>
	/// <returns>Indication if the collection contains a <typeparamref name="TAttribute"/>.</returns>
	public bool Has<TAttribute>()
		where TAttribute : Attribute
	{
		return Get<TAttribute>() != null;
	}

	/// <summary>
	/// Indicates if the collection contains an <see cref="Attribute"/> that is an instance of <paramref name="attributeType"/>.
	/// </summary>
	/// <param name="attributeType">The attribute type.</param>
	/// <returns>Indication if the collection contains an <see cref="Attribute"/> that is an instance of <paramref name="attributeType"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <c>null</c>.</exception>
	public bool Has([NotNull] Type attributeType) { return Get(attributeType) != null; }

	/// <summary>
	/// Retrieves the first <typeparamref name="TAttribute"/> in the collection if defined otherwise <c>null</c>.
	/// </summary>
	/// <typeparam name="TAttribute">The attribute type.</typeparam>
	/// <returns>The first <typeparamref name="TAttribute"/> in the collection if defined otherwise <c>null</c>.</returns>
	public TAttribute Get<TAttribute>()
		where TAttribute : Attribute
	{
		foreach (Attribute attribute in _attributes)
		{
			if (attribute is TAttribute castedAttr)
			{
				return castedAttr;
			}
		}
		return null;
	}

	/// <summary>
	/// Retrieves the first <see cref="Attribute"/> that is an instance of <paramref name="attributeType"/> in the collection if defined otherwise <c>null</c>.
	/// </summary>
	/// <param name="attributeType">The attribute type.</param>
	/// <returns>The first <see cref="Attribute"/> that is an instance of <paramref name="attributeType"/> in the collection if defined otherwise <c>null</c>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <c>null</c>.</exception>
	public Attribute Get([NotNull] Type attributeType) { return _attributes.FirstOrDefault(attributeType.IsInstanceOfType); }

	/// <summary>
	/// Retrieves all <typeparamref name="TAttribute"/>'s in the collection.
	/// </summary>
	/// <typeparam name="TAttribute">The attribute type.</typeparam>
	/// <returns>All <typeparamref name="TAttribute"/>'s in the collection.</returns>
	[ItemNotNull]
	public IEnumerable<TAttribute> GetAll<TAttribute>()
		where TAttribute : Attribute
	{
		foreach (Attribute attribute in _attributes)
		{
			if (attribute is TAttribute castedAttr) yield return castedAttr;
		}
	}

	/// <summary>
	/// Retrieves all <see cref="Attribute"/>s that are an instance of <paramref name="attributeType"/> in the collection.
	/// </summary>
	/// <param name="attributeType">The attribute type.</param>
	/// <returns>All <see cref="Attribute"/>s that are an instance of <paramref name="attributeType"/> in the collection.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <c>null</c>.</exception>
	public IEnumerable<Attribute> GetAll([NotNull] Type attributeType)
	{
		foreach (Attribute attribute in _attributes)
		{
			if (attributeType.IsInstanceOfType(attribute)) yield return attribute;
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator that iterates through the collection.</returns>
	public IEnumerator<Attribute> GetEnumerator() { return ((IEnumerable<Attribute>)_attributes).GetEnumerator(); }

	bool ICollection<Attribute>.IsReadOnly => true;

	bool ICollection<Attribute>.Contains(Attribute item) { return ((ICollection<Attribute>)_attributes).Contains(item); }

	void ICollection<Attribute>.CopyTo(Attribute[] array, int arrayIndex) { _attributes.CopyTo(array, arrayIndex); }

	int IList<Attribute>.IndexOf(Attribute item) { return ((IList<Attribute>)_attributes).IndexOf(item); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	Attribute IList<Attribute>.this[int index]
	{
		get => _attributes[index];
		set => throw new NotSupportedException();
	}

	void ICollection<Attribute>.Add(Attribute item) { throw new NotSupportedException(); }

	void ICollection<Attribute>.Clear() { throw new NotSupportedException(); }

	bool ICollection<Attribute>.Remove(Attribute item) { throw new NotSupportedException(); }

	void IList<Attribute>.Insert(int index, Attribute item) { throw new NotSupportedException(); }

	void IList<Attribute>.RemoveAt(int index) { throw new NotSupportedException(); }
}