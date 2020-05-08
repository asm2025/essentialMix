using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <inheritdoc cref="Dictionary{TKey,TValue}" />
	/// <summary>
	/// Map from types to instances of those types, e.g. int to 10 and
	/// string to "hi" within the same dictionary. This cannot be done
	/// without casting (and boxing for value types) as .NET cannot
	/// represent this relationship with generics in their current form.
	/// This class encapsulates the nastiness in a single place.
	/// </summary>
	public class TypeDictionary : Dictionary<Type, object>, IDictionary<Type, object>, ICollection<KeyValuePair<Type, object>>, IEnumerable<KeyValuePair<Type, object>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<Type, object>, IReadOnlyCollection<KeyValuePair<Type, object>>, ISerializable, IDeserializationCallback
	{
		public TypeDictionary()
		{
		}

		public TypeDictionary(int capacity) : base(capacity)
		{
		}

		public TypeDictionary(IEqualityComparer<Type> comparer) : base(comparer)
		{
		}

		public TypeDictionary(int capacity, IEqualityComparer<Type> comparer) : base(capacity, comparer)
		{
		}

		public TypeDictionary([NotNull] IDictionary<Type, object> dictionary) : base(dictionary)
		{
		}

		public TypeDictionary([NotNull] IDictionary<Type, object> dictionary, IEqualityComparer<Type> comparer) : base(dictionary, comparer)
		{
		}

		protected TypeDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		/// Maps the specified type argument to the given value. If
		/// the type argument already has a value within the dictionary,
		/// ArgumentException is thrown.
		/// </summary>
		public void Add<T>(T value)
		{
			base.Add(typeof(T), value);
		}

		/// <summary>
		/// Maps the specified type argument to the given value. If
		/// the type argument already has a value within the dictionary, it
		/// is overwritten.
		/// </summary>
		public void Put<T>(T value)
		{
			base[typeof(T)] = value;
		}

		/// <summary>
		/// Attempts to fetch a value from the dictionary, throwing a
		/// KeyNotFoundException if the specified type argument has no
		/// entry in the dictionary.
		/// </summary>
		public T Get<T>() { return (T)base[typeof(T)]; }

		/// <summary>
		/// Attempts to fetch a value from the dictionary, returning false and
		/// setting the output parameter to the default value for T if it
		/// fails, or returning true and setting the output parameter to the
		/// fetched value if it succeeds.
		/// </summary>
		public bool TryGet<T>(out T value)
		{
			if (TryGetValue(typeof(T), out object tmp))
			{
				value = (T)tmp;
				return true;
			}

			value = default(T);
			return false;
		}
	}
}