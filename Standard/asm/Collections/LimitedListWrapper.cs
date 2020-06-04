using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class LimitedListWrapper<TSource> : LimitedList
		where TSource : IList
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public LimitedListWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] LimitedListWrapper<TSource> wrapper) { return wrapper.Source; }
	}

	[Serializable]
	public class LimitedListWrapper<TSource, T> : LimitedList<T>
		where TSource : IList<T>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public LimitedListWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] LimitedListWrapper<TSource, T> wrapper) { return wrapper.Source; }
	}
}