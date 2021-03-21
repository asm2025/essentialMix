using System;
using System.ComponentModel;

namespace essentialMix.Windows.Controls
{
	public class Control : System.Windows.Forms.Control
	{
		/// <inheritdoc />
		public Control() 
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
		}

		/// <inheritdoc />
		public Control(string text) 
			: base(text)
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
		}

		/// <inheritdoc />
		public Control(string text, int left, int top, int width, int height) 
			: base(text, left, top, width, height)
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
		}

		/// <inheritdoc />
		public Control(System.Windows.Forms.Control parent, string text) 
			: base(parent, text)
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
		}

		/// <inheritdoc />
		public Control(System.Windows.Forms.Control parent, string text, int left, int top, int width, int height) 
			: base(parent, text, left, top, width, height)
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
	
		protected bool InDesignMode { get; }

		protected bool IsLoading { get; set; }
	}
}