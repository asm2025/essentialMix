using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using essentialMix.Patterns.Direction;
using essentialMix.Threading.Helpers;
using essentialMix.Windows.Controls;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ListViewExtension
	{
		public static void FitColumns([NotNull] this ListView thisValue, ListViewFitColumnEnum type)
		{
			if (thisValue.Columns.Count == 0) return;

			int width;

			switch (type)
			{
				case ListViewFitColumnEnum.Header:
					width = -1;
					break;
				case ListViewFitColumnEnum.Content:
					width = -2;
					break;
				default:
					width = (thisValue.ClientRectangle.Width - 20) / thisValue.Columns.Count;
					break;
			}

			foreach (ColumnHeader column in thisValue.Columns)
			{
				column.Width = width;
			}
		}

		public static void SelectAll([NotNull] this ListView thisValue)
		{
			if (thisValue.Items.Count == 0) return;

			for (int i = 0; i < thisValue.Items.Count; i++)
			{
				if (thisValue.SelectedIndices.Contains(i)) continue;
				thisValue.SelectedIndices.Add(i);
			}
		}

		public static void SelectNone([NotNull] this ListView thisValue)
		{
			thisValue.SelectedIndices.Clear();
		}

		public static void SelectInvert([NotNull] this ListView thisValue)
		{
			if (thisValue.Items.Count == 0) return;

			if (thisValue.SelectedIndices.Count == 0)
				SelectAll(thisValue);
			else if (thisValue.SelectedIndices.Count == thisValue.Items.Count)
				SelectNone(thisValue);
			else
			{
				HashSet<int> selected = new HashSet<int>(thisValue.SelectedIndices.Cast<int>());
				thisValue.SelectedIndices.Clear();

				for (int i = 0; i < thisValue.Items.Count; i++)
				{
					if (selected.Contains(i)) continue;
					thisValue.SelectedIndices.Add(i);
				}
			}
		}

		public static void Copy([NotNull] this ListView thisValue, Action<StringBuilder, ListViewItem> onAppend = null)
		{
			if (thisValue.Items.Count == 0) return;

			StringBuilder sb = new StringBuilder();

			if (onAppend == null)
			{
				if (thisValue.Columns.Count > 0)
					onAppend = (builder, item) => builder.AppendLine(string.Join("\t\t", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(e => e.Text)));
				else onAppend = (builder, item) => builder.AppendLine(item.Text);
			}

			if (thisValue.SelectedItems.Count == 0)
			{
				foreach (ListViewItem item in thisValue.Items)
					onAppend(sb, item);
			}
			else
			{
				foreach (ListViewItem item in thisValue.SelectedItems)
					onAppend(sb, item);
			}

			Clipboard.SetText(sb.ToString());
		}

		public static void OpenOnActivate([NotNull] this ListView thisValue, Point location)
		{
			ListViewItem item = thisValue.HitTest(location).Item;
			if (string.IsNullOrEmpty(item?.Text)) return;
			ProcessHelper.ShellExec(item.Text);
		}

		public static void MoveSelectedItems([NotNull] this ListView thisValue, VerticalDirection moveDirection)
		{
			ListView.SelectedListViewItemCollection selectedItems = thisValue.SelectedItems;
			if (selectedItems.Count == 0) return;

			ListView.ListViewItemCollection items = thisValue.Items;

			switch (moveDirection)
			{
				case VerticalDirection.Up when selectedItems[0].Index <= 0:
					return;
				case VerticalDirection.Down when selectedItems[selectedItems.Count - 1].Index >= items.Count - 1:
					return;
			}

			IEnumerable<ListViewItem> itemsToBeMoved = selectedItems.Cast<ListViewItem>();
			if (moveDirection == VerticalDirection.Down) itemsToBeMoved = itemsToBeMoved.Reverse();
			thisValue.BeginUpdate();

			int direction = (int)moveDirection;

			foreach (ListViewItem item in itemsToBeMoved)
			{
				int index = item.Index + direction;
				items.RemoveAt(item.Index);
				items.Insert(index, item);
			}

			thisValue.EndUpdate();
		}
	}
}