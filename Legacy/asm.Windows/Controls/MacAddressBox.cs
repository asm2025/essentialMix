using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Windows.Controls
{
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[DefaultEvent("MaskInputRejected")]
	[DefaultBindingProperty("Text")]
	[DefaultProperty("Mask")]
	public class MacAddressBox : MaskedTextBox
	{
		private string _format;

		public MacAddressBox()
		{
			InDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
			IsLoading = true;
			base.AsciiOnly = true;
			Format = DefaultFormat;
		}

		/// <inheritdoc />
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			IsLoading = false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			char c = (char)e.KeyCode;

			switch (e.KeyCode)
			{
				case Keys.NumPad7:
					c = '7';
					break;
				case Keys.NumPad8:
					c = '8';
					break;
				case Keys.NumPad9:
					c = '9';
					break;
			}

			if (c.IsLetterOrDigit() && !MacAddressHelper.IsAllowed(c))
			{
				if (e.Modifiers == Keys.None || (c != 'C' && c != 'X' && c != 'V'))
				{
					e.Handled = true;
					e.SuppressKeyPress = true;
					return;
				}
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (!MacAddressHelper.IsAllowed(e.KeyChar)) return;
			base.OnKeyPress(e);
		}

		public string DefaultFormat => MacAddressHelper.DefaultFormat;

		public IReadOnlyCollection<string> Formats => MacAddressHelper.Formats;

		public string Format
		{
			get => _format;
			set
			{
				if (_format == value) return;
				if (string.IsNullOrEmpty(value)) value = DefaultFormat;
				_format = value;
				BaseMask = MacAddressHelper.GetFormatMask(_format);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string Mask => base.Mask;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue(true)]
		public new bool AsciiOnly => base.AsciiOnly;

		protected string BaseMask
		{
			get => base.Mask;
			set => base.Mask = value;
		}

		protected bool InDesignMode { get; }

		protected bool IsLoading { get; set; }

		public Regex[] GetFormatRegEx([NotNull] string format) { return MacAddressHelper.GetFormatRegEx(format); }

		public bool IsFormatSupported(string format) { return MacAddressHelper.IsFormatSupported(format); }
	}
}