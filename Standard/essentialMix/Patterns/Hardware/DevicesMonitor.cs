using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using essentialMix.Extensions;
using essentialMix.Patterns.Object;

namespace essentialMix.Patterns.Hardware
{
	[DebuggerDisplay("{Name}")]
	public class DevicesMonitor : Disposable
	{
		private IntPtr _notificationData;
		private IntPtr _notificationHandle;

		/// <inheritdoc />
		public DevicesMonitor()
			: this(null)
		{
		}

		/// <inheritdoc />
		public DevicesMonitor(string name)
		{
			Name = name;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) Deregister();
			base.Dispose(disposing);
		}

		public string Name { get; set; }

		public void Register(IntPtr hWnd) { Register(hWnd, false, Guid.Empty); }
		public void Register(IntPtr hWnd, Guid deviceId) { Register(hWnd, false, deviceId); }
		/// <summary>
		/// Hook the device class to a <see cref="DevicesMonitor" /> instance.
		/// </summary>
		/// <param name="hWnd">A handle to the window or service that will receive device events</param>
		/// <param name="isServiceHandle">Indicate whether <see cref="hWnd"/> parameter is a window handle or service status handle</param>
		/// <param name="deviceId">Device GUID. See <see cref="DeviceGuids"/> for a list of predefined GUID values</param>
		public void Register(IntPtr hWnd, bool isServiceHandle, Guid deviceId)
		{
			if (hWnd.IsInvalidHandle()) throw new InvalidOperationException("Invalid window handle.");
			if (!_notificationHandle.IsInvalidHandle()) throw new InvalidOperationException($"{GetType().Name} is already attached.");

			DEV_BROADCAST_DEVICEINTERFACE di = new DEV_BROADCAST_DEVICEINTERFACE
			{
				DeviceType = DBT_DeviceType.DeviceInterface,
				ClassGuid = deviceId
			};

			di.Size = Marshal.SizeOf(di);
			_notificationData = Marshal.AllocHGlobal(di.Size);
			Marshal.StructureToPtr(di, _notificationData, true);
			int flags = isServiceHandle
							? Win32.DEVICE_NOTIFY_SERVICE_HANDLE
							: Win32.DEVICE_NOTIFY_WINDOW_HANDLE;
			if (deviceId.IsEmpty()) flags |= Win32.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES;
			_notificationHandle = Win32.RegisterDeviceNotification(hWnd, _notificationData, flags);
		}

		public void Deregister()
		{
			if (_notificationHandle.IsInvalidHandle()) return;
			Win32.UnregisterDeviceNotification(_notificationHandle);
			Marshal.FreeHGlobal(_notificationData);
			_notificationData = IntPtr.Zero;
			_notificationHandle = IntPtr.Zero;
		}
	}
}
