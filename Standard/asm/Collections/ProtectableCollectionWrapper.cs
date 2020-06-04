using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Collections
{
	[Serializable]
	public class ProtectableCollectionWrapper<TSource, T> : ProtectableCollection<T>
		where TSource : ICollection<T>
	{
		[NonSerialized] private readonly TSource _source;

		/// <inheritdoc />
		public ProtectableCollectionWrapper([NotNull] TSource source) 
			: base(source)
		{
			_source = (TSource)base.Source;
		}

		[NotNull]
		public new TSource Source => _source;

		[NotNull]
		public static implicit operator TSource([NotNull] ProtectableCollectionWrapper<TSource, T> wrapper) { return wrapper.Source; }
	}
}