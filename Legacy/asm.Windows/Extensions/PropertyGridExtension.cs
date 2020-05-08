using System;
using System.Windows.Forms;
using asm.Windows.Controls;
using JetBrains.Annotations;

namespace asm.Windows.Extensions
{
	public static class PropertyGridExtension
	{
		public static GridItem GetRoot([NotNull] this PropertyGrid thisValue) { return thisValue.SelectedGridItem?.GetRoot(); }

		public static void Expand([NotNull] this PropertyGrid thisValue, Predicate<GridItem> predicate, bool expanded = true, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			GetRoot(thisValue)?.Expand(predicate, expanded, spanNested, flags);
		}

		public static void Expand([NotNull] this PropertyGrid thisValue)
		{
			GetRoot(thisValue)?.Expand();
		}

		public static void Expand([NotNull] this PropertyGrid thisValue, bool expanded, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			GetRoot(thisValue)?.Expand(expanded, spanNested, flags);
		}

		public static void Expand([NotNull] this PropertyGrid thisValue, [NotNull] string name, bool expanded = true, bool spanNested = false, GridItemFlags flags = GridItemFlags.Default)
		{
			if (string.IsNullOrEmpty(name)) return;
			GetRoot(thisValue)?.Expand(name, expanded, spanNested, flags);
		}

		public static void ExpandSiblings([NotNull] this PropertyGrid thisValue, bool expanded = true)
		{
			thisValue.SelectedGridItem?.ExpandSiblings(expanded);
		}

		public static void ExpandParents([NotNull] this PropertyGrid thisValue, bool expanded = true)
		{
			thisValue.SelectedGridItem?.ExpandParents(expanded);
		}

		public static void ExpandChildren([NotNull] this PropertyGrid thisValue, bool expanded = true)
		{
			thisValue.SelectedGridItem?.ExpandChildren(expanded);
		}
	}
}