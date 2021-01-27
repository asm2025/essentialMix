using System;
using System.Drawing;
using System.Windows.Forms;
using asm;
using asm.Helpers;
using asm.Patterns.Hardware;

namespace TestApp
{
	public partial class TestUSBForm : Form
	{
		private const string MSG_ATTACH = "Please attach a USB device to see a change.";
		private const string MSG_DETACH = "Please detach a USB device to close the window.";

		private DevicesMonitor _devicesMonitor;

		public TestUSBForm()
		{
			InitializeComponent();
			lblMessage.Text = MSG_ATTACH;
			_devicesMonitor = new DevicesMonitor();
		}

		/// <inheritdoc />
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			_devicesMonitor.Register(Handle, new Guid(DeviceGuids.USB));
		}

		/// <inheritdoc />
		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			_devicesMonitor.Deregister();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObjectHelper.Dispose(ref _devicesMonitor);
				ObjectHelper.Dispose(ref components);
			}
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg != Win32.WM_DEVICECHANGE) return;

			DBT wParam = (DBT)(int)m.WParam;

			switch (wParam)
			{
				case DBT.DBT_DEVICEARRIVAL:
					lblMessage.Text = MSG_DETACH;
					lblMessage.ForeColor = Color.Green;
					break;
				case DBT.DBT_DEVICEREMOVECOMPLETE:
					Close();
					break;
			}
		}
	}
}
