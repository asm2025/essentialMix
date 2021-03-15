using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using essentialMix.Patterns.Direction;
using essentialMix.Web.Routing;
using essentialMix.Extensions;

namespace SortMVCRoutes
{
	public partial class MainForm : Form
	{
		private readonly string _sortStartText;
		private readonly string _sortEndText = "&Stop";
		
		public MainForm()
		{
			InitializeComponent();
			_sortStartText = btnSort.Text;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EnableControls();
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			lvwOutput.InvokeIf(() => lvwOutput.Items.Clear());

			string text = null;
			txtInput.InvokeIf(() => text = txtInput.Text);
			if (string.IsNullOrEmpty(text)) return;
			EnableControls();

			List<RouteData> routes = new List<RouteData>();

			foreach (RouteData data in RouteData.ParseRoutes(text))
			{
				if (worker.CancellationPending) return;
				routes.Add(data);
			}

			if (worker.CancellationPending) return;
			routes.Sort(RouteDataComparer.Default);

			lvwOutput.InvokeIf(() =>
			{
				lvwOutput.BeginUpdate();

				foreach (RouteData data in routes)
				{
					if (worker.CancellationPending) return;
					lvwOutput.Items.Add(new ListViewItem
					{
						Text = data.Route.Url,
						ToolTipText = data.Data,
						Tag = data
					});
				}

				lvwOutput.EndUpdate();
			});
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			EnableControls();
		}

		private void txtInput_TextChanged(object sender, EventArgs e)
		{
			EnableControls();
		}

		private void lvwOutput_ClientSizeChanged(object sender, EventArgs e)
		{
			lvwOutput.Columns[0].Width = lvwOutput.ClientRectangle.Width;
		}

		private void lvwOutput_KeyUp(object sender, KeyEventArgs e)
		{
			if (lvwOutput.SelectedIndices.Count < 1 || !e.Control) return;

			switch (e.KeyCode)
			{
				case Keys.Down:
				case Keys.NumPad2:
					lvwOutput.MoveSelectedItems(VerticalDirection.Down);
					break;
				case Keys.Up:
				case Keys.NumPad8:
					lvwOutput.MoveSelectedItems(VerticalDirection.Up);
					break;
			}
		}

		private void btnSort_Click(object sender, EventArgs e)
		{
			if (worker.IsBusy)
			{
				worker.CancelAsync();
				return;
			}

			worker.RunWorkerAsync();
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.Clear();
			if (!HasItems()) return;

			StringBuilder sb = new StringBuilder(lvwOutput.Items.Count * 50);

			foreach (ListViewItem item in lvwOutput.Items)
			{
				sb.AppendLine(((RouteData)item.Tag).Data);
			}

			Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			worker.CancelAsync();
			Application.Exit();
		}

		private bool HasText()
		{
			bool hasText = false;
			txtInput.InvokeIf(() => hasText = txtInput.Text.Length > 0);
			return hasText;
		}

		private bool HasItems()
		{
			bool hasItems = false;
			lvwOutput.InvokeIf(() => hasItems = lvwOutput.Items.Count > 0);
			return hasItems;
		}

		private void EnableControls(bool enable = true)
		{
			if (worker.IsBusy) enable = false;

			if (!enable)
			{
				this.EnableControls(false, Array.Empty<Control>());
				return;
			}

			this.EnableControls(true, btnSort, btnCopy, btnExit);
			btnSort.InvokeIf(() =>
			{
				if (worker.IsBusy)
				{
					btnSort.Enabled = true;
					if (btnSort.Text != _sortEndText) btnSort.Text = _sortEndText;
				}
				else
				{
					btnSort.Enabled = HasText();
					if (btnSort.Text != _sortStartText) btnSort.Text = _sortStartText;
				}
			});
			btnCopy.InvokeIf(() => btnCopy.Enabled = HasItems());
			btnExit.InvokeIf(() => btnExit.Enabled = !worker.IsBusy);
		}
	}
}
