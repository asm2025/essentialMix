using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ProtectableListWrapper<TSource> : ProtectableList
		where TSource : IList
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public ProtectableListWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = (TSource)base.Source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] ProtectableListWrapper<TSource> wrapper) { return wrapper.Source; }
	}

	[Serializable]
	public class ProtectableListWrapper<TSource, T> : ProtectableList<T>
		where TSource : IList<T>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public ProtectableListWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = source;
		}
	
		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] ProtectableListWrapper<TSource, T> wrapper) { return wrapper.Source; }
	}
}