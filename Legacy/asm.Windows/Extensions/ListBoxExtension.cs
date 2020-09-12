using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using asm.Patterns.Direction;
using asm.Threading.Helpers;
using JetBrains.Annotations;

namespace asm.Windows.Extensions
{
	public static class ListBoxExtension
	{
		public static void SelectAll([NotNull] this ListBox thisValue)
		{
			if (thisValue.Items.Count == 0) return;

			for (int i = 0; i < thisValue.Items.Count; i++)
			{
				if (thisValue.SelectedIndices.Contains(i)) continue;
				thisValue.SelectedIndices.Add(i);
			}
		}

		public static void SelectNone([NotNull] this ListBox thisValue)
		{
			thisValue.SelectedIndices.Clear();
		}

		public static void SelectInvert([NotNull] this ListBox thisValue)
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

		public static void Copy<T>([NotNull] this ListBox thisValue, Action<StringBuilder, T> onAppend = null)
		{
			if (thisValue.Items.Count == 0) return;

			StringBuilder sb = new StringBuilder();

			onAppend ??= (builder, item) =>
			{
				string t = Convert.ToString(item);
				if (string.IsNullOrEmpty(t)) return;
				builder.AppendLine(t);
			};

			if (thisValue.SelectedIndices.Count == 0)
			{
				foreach (T item in thisValue.Items)
					onAppend(sb, item);
			}
			else
			{
				foreach (T item in thisValue.SelectedItems)
					onAppend(sb, item);
			}

			Clipboard.SetText(sb.ToString());
		}

		public static void OpenOnActivate([NotNull] this ListBox thisValue, Point location)
		{
			int index = thisValue.IndexFromPoint(location);
			if (index == ListBox.NoMatches) return;

			string value = Convert.ToString(thisValue.Items[index]);
			if (string.IsNullOrEmpty(value)) return;
			ProcessHelper.ShellExec(value);
		}

		public static void MoveSelectedItems([NotNull] this ListBox thisValue, VerticalDirection moveDirection)
		{
			ListBox.SelectedIndexCollection selectedIndices = thisValue.SelectedIndices;
			if (selectedIndices.Count == 0) return;

			ListBox.ObjectCollection items = thisValue.Items;

			switch (moveDirection)
			{
				case VerticalDirection.Up when selectedIndices[0] <= 0:
					return;
				case VerticalDirection.Down when selectedIndices[selectedIndices.Count - 1] >= items.Count - 1:
					return;
			}

			IEnumerable<int> itemsToBeMoved = selectedIndices.Cast<int>();
			if (moveDirection == VerticalDirection.Down) itemsToBeMoved = itemsToBeMoved.Reverse();
			thisValue.BeginUpdate();

			int direction = (int)moveDirection;

			foreach (int indices in itemsToBeMoved)
			{
				int index = indices + direction;
				object item = items[index];
				items.RemoveAt(indices);
				items.Insert(index, item);
			}

			thisValue.EndUpdate();
		}
	}
}