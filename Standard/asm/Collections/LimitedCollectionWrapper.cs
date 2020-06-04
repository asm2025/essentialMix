using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <inheritdoc />
	[Serializable]
	public class LimitedCollectionWrapper<TSource, T> : LimitedCollection<T>
		where TSource : ICollection<T>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public LimitedCollectionWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = (TSource)base.Source;
		}

		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] LimitedCollectionWrapper<TSource, T> wrapper) { return wrapper.Source; }
	}
}