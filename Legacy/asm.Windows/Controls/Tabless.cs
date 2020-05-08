using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace asm.Windows.Controls
{
	public class Tabless : TabControl
	{
		private const int TCM_ADJUSTRECT = 0x1328;

		private static readonly IntPtr INT_PTR_ONE = new IntPtr(1);

		public Tabless()
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
		}

		/// <inheritdoc />
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			IsLoading = false;
		}

		/// <inheritdoc />
		/// <summary>
		/// This member overrides <see cref="M:global::System.Windows.Forms.Control.WndProc(global::System.Windows.Forms.Message@)" />.
		/// </summary>
		/// <param name="m">A Windows Message Object. </param>
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == TCM_ADJUSTRECT)
			{
				m.Result = INT_PTR_ONE;
				return;
			}
			base.WndProc(ref m);
		}
	
		protected bool InDesignMode { get; }

		protected bool IsLoading { get; set; }
	}
}
