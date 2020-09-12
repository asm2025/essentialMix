using System.Collections.Generic;
using asm.Comparers;

namespace asm.Collections
{
	public class SubItemComparer : GenericComparer<SubItem>
	{
		public new static SubItemComparer Default { get; } = new SubItemComparer();

		/// <inheritdoc />
		public SubItemComparer() 
		{
		}

		/// <inheritdoc />
		public SubItemComparer(IComparer<SubItem> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public SubItemComparer(IComparer<SubItem> comparer, IEqualityComparer<SubItem> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(SubItem x, SubItem y) { return PropertyComparer.Default.Compare(x, y); }

		public override bool Equals(SubItem x, SubItem y) { return PropertyComparer.Default.Equals(x, y); }
	}
}