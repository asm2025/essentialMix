using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ProtectableDictionaryWrapper<TSource> : ProtectableDictionary
		where TSource : IDictionary
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public ProtectableDictionaryWrapper([NotNull] TSource source)
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] ProtectableDictionaryWrapper<TSource> wrapper) { return wrapper.Source; }
	}

	[Serializable]
	public class ProtectableDictionaryWrapper<TSource, TKey, TValue> : ProtectableDictionary<TKey, TValue>
		where TSource : IDictionary<TKey, TValue>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public ProtectableDictionaryWrapper([NotNull] TSource source)
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] ProtectableDictionaryWrapper<TSource, TKey, TValue> wrapper) { return wrapper.Source; }
	}
}