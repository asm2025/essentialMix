using System;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class SecureStringExtension
	{
		public static bool IsNullOrEmpty(this SecureString thisValue) { return thisValue == null || thisValue.Length == 0; }

		[NotNull]
		public static SecureString ToReadOnly([NotNull] this SecureString thisValue)
		{
			if (thisValue.IsReadOnly()) return thisValue;
			thisValue.MakeReadOnly();
			return thisValue;
		}

		public static string UnSecure(this SecureString thisValue)
		{
			if (thisValue == null) return null;
			if (thisValue.Length == 0) return string.Empty;

			IntPtr unmanagedString = IntPtr.Zero;

			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(thisValue);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
			}
		}
	}
}