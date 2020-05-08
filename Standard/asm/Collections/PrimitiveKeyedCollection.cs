using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class PrimitiveKeyedCollection<T> : KeyedCollection<T, T>
	{
		/// <inheritdoc />
		public PrimitiveKeyedCollection()
			: this(null, -1)
		{
		}

		/// <inheritdoc />
		public PrimitiveKeyedCollection(IEqualityComparer<T> comparer)
			: this(comparer, -1)
		{
		}

		/// <inheritdoc />
		public PrimitiveKeyedCollection(IEqualityComparer<T> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
			Type type = typeof(T);
			if (!type.IsPrimitive()) throw new InvalidOperationException($"'{type}' is not a primitive type.");
		}

		/// <inheritdoc />
		public PrimitiveKeyedCollection([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		public PrimitiveKeyedCollection([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: base(comparer)
		{
			Type type = typeof(T);
			if (!type.IsPrimitive()) throw new InvalidOperationException($"'{type}' is not a primitive type.");
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			
			foreach (T value in collection) 
				base.Add(value);
		}

		/// <inheritdoc />
		protected PrimitiveKeyedCollection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc />
		[NotNull]
		protected override T GetKeyForItem(T item) { return item; }
	}
}