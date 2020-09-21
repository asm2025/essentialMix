using System;
using System.Collections;
using System.Windows.Forms;
using asm.Windows.Controls;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class GridItemExtension
	{
		private const GridItemFlags GRID_ITEM_TYPE_DEFAULT = GridItemFlags.Root
																| GridItemFlags.Category
																| GridItemFlags.Property
																| GridItemFlags.Array;

		[NotNull]
		public static GridItem GetRoot([NotNull] this GridItem thisValue)
		{
			GridItem root = thisValue;

			while (root?.Parent != null)
				root = root.Parent;

			return root;
		}

		public static void Expand([NotNull] this GridItem thisValue, Predicate<GridItem> predicate, bool expanded = true, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			ExpandInternal(thisValue, predicate, expanded, spanNested, flags);
		}

		public static void Expand([NotNull] this GridItem thisValue)
		{
			ExpandLocal(thisValue);

			static void ExpandLocal(GridItem item)
			{
				GridItemCollection items = item.GridItems;
				if (items == null || items.Count == 0) return;

				foreach (GridItem gridItem in items)
				{
					if (gridItem.Expandable) gridItem.Expanded = true;
					ExpandLocal(gridItem);
				}
			}
		}

		public static void ExpandSiblings([NotNull] this GridItem thisValue, bool expanded = true)
		{
			if (thisValue.Parent == null) return;
			Expand(thisValue.Parent, expanded);
		}

		public static void ExpandParents([NotNull] this GridItem thisValue, bool expanded = true)
		{
			if (thisValue.Parent == null) return;
			Expand(thisValue.Parent, expanded);
			ExpandSiblings(thisValue.Parent, expanded);
		}

		public static void ExpandChildren([NotNull] this GridItem thisValue, bool expanded = true)
		{
			GridItemCollection items = thisValue.GridItems;
			if (items == null || items.Count == 0) return;

			foreach (GridItem item in items)
				Expand(item);
		}

		public static void Expand([NotNull] this GridItem thisValue, bool expanded, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			ExpandInternal(thisValue, null, expanded, spanNested, flags);
		}

		public static void Expand([NotNull] this GridItem thisValue, [NotNull] string name, bool expanded = true, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			if (string.IsNullOrEmpty(name)) return;
			ExpandInternal(thisValue, item => item.Label.IsSame(name), expanded, spanNested, flags);
		}

		private static void ExpandInternal([NotNull] this GridItem thisValue, Predicate<GridItem> predicate, bool expanded, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			ICollection items = thisValue.GridItems;
			if (items == null || items.Count == 0) return;
			if (flags == GridItemFlags.Default) flags = GRID_ITEM_TYPE_DEFAULT;

			Predicate<GridItem> filter;

			if (predicate == null)
			{
				filter = item =>
						{
							switch (item.GridItemType)
							{
								case GridItemType.Root:
									return flags.HasFlag(GridItemFlags.Root);
								case GridItemType.Category:
									return flags.HasFlag(GridItemFlags.Category);
								case GridItemType.ArrayValue:
									return flags.HasFlag(GridItemFlags.Array);
								case GridItemType.Property:
									return flags.HasFlag(GridItemFlags.Property) && item.Expandable;
								default:
									return false;
							}
						};
			}
			else
			{
				filter = item =>
						{
							switch (item.GridItemType)
							{
								case GridItemType.Root:
									if (!flags.HasFlag(GridItemFlags.Root)) return false;
									break;
								case GridItemType.Category:
									if (!flags.HasFlag(GridItemFlags.Category)) return false;
									break;
								case GridItemType.ArrayValue:
									if (!flags.HasFlag(GridItemFlags.Array)) return false;
									break;
								case GridItemType.Property:
									if (!flags.HasFlag(GridItemFlags.Property) || !item.Expandable) return false;
									break;
							}

							return predicate(item);
						};
			}

			foreach (GridItem item in items)
			{
				if (filter(item)) item.Expanded = expanded;
				if (spanNested) ExpandInternal(item, predicate, expanded, true, flags);
			}
		}
	}
}