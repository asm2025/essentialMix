using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using asm.Extensions;
using asm.Network;

namespace asm.Helpers
{
	public static class NetworkHelper
	{
		private static NetworkJoinInformation __local;

		public static NetworkJoinInformation Local
		{
			get
			{
				GetJoinStatus(null, ref __local);
				return __local;
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
			return flags.HasFlag(Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_MODEM) || flags.HasFlag(Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_LAN);
		}

		public static Win32.InternetGetConnectedStateFlags GetInternetConnectionState()
		{
			return Win32.InternetGetConnectedState(out int flags, 0)
						? (Win32.InternetGetConnectedStateFlags)flags
						: Win32.InternetGetConnectedStateFlags.INTERNET_CONNECTION_OFFLINE;
		}
	}
}