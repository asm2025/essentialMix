using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using asm.Extensions;
using asm.Network;

namespace asm.Helpers
{
	public static class NetworkHelper
	{
		private static readonly HashSet<Win32.InternetGetConnectedStateFlags> CONNECTED_FLAGS = new HashSet<Win32.InternetGetConnectedStateFlags>
		{
			Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_MODEM,
			Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_LAN
		};

		private static NetworkJoinInformation _local;

		public static NetworkJoinInformation Local
		{
			get
			{
				GetJoinStatus(null, ref _local);
				return _local;
			}
		}

		public static bool GetJoinStatus(string computerName, ref NetworkJoinInformation info)
		{
			NetworkJoinInformation.Zero(ref info);
			computerName = computerName?.Trim();
			if (string.IsNullOrEmpty(computerName)) computerName = null;

			IntPtr buffer = IntPtr.Zero;
			bool result;

			try
			{
				if (Win32.NetGetJoinInformation(computerName, ref buffer, ref info.Status) != 0) return false;
				info.Domain = Marshal.PtrToStringUni(buffer);
				result = true;
			}
			catch
			{
				result = false;
			}
			finally
			{
				if (!buffer.IsZero()) Win32.NetApiBufferFree(buffer);
			}

			return result;
		}

		public static bool IsConnectedToNetwork() { return NetworkInterface.GetIsNetworkAvailable(); }

		public static bool IsConnectedToInternet()
		{
			Win32.InternetGetConnectedStateFlags flags = GetInternetConnectionState();
			return CONNECTED_FLAGS.Any(cf => flags.HasFlag(cf));
		}

		public static Win32.InternetGetConnectedStateFlags GetInternetConnectionState()
		{
			return Win32.InternetGetConnectedState(out int flags, 0)
						? (Win32.InternetGetConnectedStateFlags)flags
						: Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_OFFLINE;
		}
	}
}