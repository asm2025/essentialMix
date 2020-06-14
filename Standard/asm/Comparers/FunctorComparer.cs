using System;
using System.Collections.Generic;
using System.Runtime;
using asm.Other.JonSkeet.MiscUtil.Collections;
using JetBrains.Annotations;

namespace asm.Comparers
{
	// https://github.com/StevenThuriot/FunctorComparer
	public sealed class FunctorComparer<T> : GenericComparer<T>
	{
		public new static FunctorComparer<T> Default { get; } = new FunctorComparer<T>((x, y) => Comparer<T>.Default.Compare(x, y));

		[TargetedPatchingOptOut("Performance critical to in-line this type of method across NGen image boundaries")]
		public FunctorComparer([NotNull] Comparison<T> comparison)
			: this(ComparisonComparer.FromComparison(comparison))
		{
		}

		/// <inheritdoc />
		public FunctorComparer(IComparer<T> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public FunctorComparer(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}
	}
}