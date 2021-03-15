using System;
using System.Text;
using System.Windows.Forms;
using essentialMix.Threading.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class DataGridViewExtension
	{
		public static void SelectNone([NotNull] this DataGridView thisValue)
		{
			thisValue.ClearSelection();
		}

		public static void SelectInvert([NotNull] this DataGridView thisValue)
		{
			if (thisValue.RowCount == 0) return;

			if (thisValue.SelectedRows.Count == 0)
				thisValue.SelectAll();
			else if (thisValue.SelectedRows.Count == thisValue.RowCount)
				SelectNone(thisValue);
			else
			{
				foreach (DataGridViewRow row in thisValue.Rows)
				{
					row.Selected = !row.Selected;
				}
			}
		}

		public static void Copy([NotNull] this DataGridView thisValue)
		{
			if (thisValue.RowCount == 0) return;

			StringBuilder sb = new StringBuilder();

			if (thisValue.SelectedRows.Count == 0)
			{
				foreach (DataGridViewRow row in thisValue.SelectedRows)
				{
					StringBuilder sbItem = new StringBuilder();

					foreach (DataGridViewCell cell in row.Cells)
					{
						if (sbItem.Length > 0) sbItem.Append("\t\t");
						sbItem.Append(cell.Value ?? string.Empty);
					}

					sb.AppendLine(sbItem.ToString());
				}
			}
			else
			{
				foreach (DataGridViewRow row in thisValue.SelectedRows)
				{
					StringBuilder sbItem = new StringBuilder();

					foreach (DataGridViewCell cell in row.Cells)
					{
						if (sbItem.Length > 0) sbItem.Append("\t\t");
						sbItem.Append(cell.Value ?? string.Empty);
					}

					sb.AppendLine(sbItem.ToString());
				}
			}

			Clipboard.SetText(sb.ToString());
		}

		public static void OpenOnActivate([NotNull] this DataGridView thisValue, [NotNull] MouseEventArgs args)
		{
			int index = thisValue.HitTest(args.X, args.Y).RowIndex;
			if (index < 0) return;
			DataGridViewRow row = thisValue.Rows[index];

			try
			{
				string value = Convert.ToString(row.Cells[0].Value);
				ProcessHelper.ShellExec(value);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.CollectMessages(), "Error...", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}