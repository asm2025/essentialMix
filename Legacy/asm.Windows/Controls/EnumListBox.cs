using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using asm.Collections;
using asm.Extensions;
using asm.Helpers;

namespace asm.Windows.Controls
{
	public class EnumListBox : ListBox
	{
		public const int ITEM_MARGIN = 4;
		public const int ICON_HEIGHT = 16;

		public EnumListBox()
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;

			// This call is required by the designer.
			InitializeComponent();

			Sorted = false;
			IntegralHeight = false;
			base.DrawMode = DrawMode.OwnerDrawFixed;
			base.SelectionMode = SelectionMode.One;
			base.ItemHeight = ICON_HEIGHT + 2 * ITEM_MARGIN;
		}

		/// <inheritdoc />
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			IsLoading = false;
		}

		public override DrawMode DrawMode
		{
			get => base.DrawMode;
			set { }
		}

		public override int ItemHeight
		{
			get => base.ItemHeight;
			set { }
		}

		public override SelectionMode SelectionMode
		{
			get => base.SelectionMode;
			set { }
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			Item item = (Item)Items[e.Index];
			SizeF txtSize = e.Graphics.MeasureString(item.Text, Font);
			e.ItemWidth = ICON_HEIGHT + 3 * ITEM_MARGIN + (int)txtSize.Width;
			e.ItemHeight = ICON_HEIGHT.Maximum(ICON_HEIGHT, (int)txtSize.Height) + ITEM_MARGIN;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.DrawBackground();

			Item item = (Item)Items[e.Index];

			Brush b;
			bool dispose = false;

			if (item.Enabled)
			{
				if (e.State.HasFlag(DrawItemState.Selected))
					b = SystemBrushes.HighlightText;
				else
				{
					b = new SolidBrush(e.ForeColor);
					dispose = true;
				}
			}
			else
			{
				b = SystemBrushes.ControlDark;
			}

			float x = e.Bounds.Left;
			float y = e.Bounds.Top + ITEM_MARGIN;
			float width = e.Bounds.Right - ITEM_MARGIN - x;
			float height = e.Bounds.Bottom - ITEM_MARGIN - y;
			RectangleF rc = new RectangleF(x, y, width, height);
			e.Graphics.DrawString(item.Text, e.Font, b, rc);
			if (dispose) ObjectHelper.Dispose(ref b);
			e.DrawFocusRectangle();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			//Height = ItemHeight * Items.Count + SystemInformation.HorizontalScrollBarHeight;
			MessageBox.Show("Test");
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			if (IsLoading) return;
			EditorService?.CloseDropDown();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (IsLoading || !e.Button.HasFlag(MouseButtons.Left)) return;
			SelectedIndex = IndexFromPoint(e.Location);
		}

		public IWindowsFormsEditorService EditorService { get; set; }
	
		protected bool InDesignMode { get; }

		protected bool IsLoading { get; set; }

		/// <summary>
		///     Required method for Designer support - do not modify
		///     the contents of this method with the code editor.
		/// </summary>
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void InitializeComponent()
		{
		}
	}
}