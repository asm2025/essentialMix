using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class LimitedDictionaryWrapper<TSource> : LimitedDictionary
		where TSource : IDictionary
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public LimitedDictionaryWrapper([NotNull] TSource source)
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] LimitedDictionaryWrapper<TSource> wrapper) { return wrapper.Source; }
	}

	[Serializable]
	public class LimitedDictionaryWrapper<TSource, TKey, TValue> : LimitedDictionary<TKey, TValue>
		where TSource : IDictionary<TKey, TValue>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public LimitedDictionaryWrapper([NotNull] TSource source)
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] LimitedDictionaryWrapper<TSource, TKey, TValue> wrapper) { return wrapper.Source; }
	}
}