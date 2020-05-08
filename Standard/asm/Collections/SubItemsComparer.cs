using System.Collections.Generic;
using asm.Comparers;

namespace asm.Collections
{
	public class SubItemsComparer : GenericComparer<Item.SubItemsCollection>
	{
		public new static SubItemsComparer Default { get; } = new SubItemsComparer();

		/// <inheritdoc />
		public SubItemsComparer()
		{
		}

		/// <inheritdoc />
		public SubItemsComparer(IComparer<Item.SubItemsCollection> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public SubItemsComparer(IComparer<Item.SubItemsCollection> comparer, IEqualityComparer<Item.SubItemsCollection> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(Item.SubItemsCollection x, Item.SubItemsCollection y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return DictionaryComparer<string, SubItem>.Default.Compare(x, y);
		}

		public override bool Equals(Item.SubItemsCollection x, Item.SubItemsCollection y)
		{
			if (ReferenceEquals(x, y)) return true;
			return x != null && y != null && x.GetType() == y.GetType() && ReferenceEquals(x.Owner, y.Owner) && DictionaryComparer<string, SubItem>.Default.Equals(x, y);
		}
	}
}