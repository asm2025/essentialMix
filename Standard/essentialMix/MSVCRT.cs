using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix;

public static class MSVCRT
{
	[SecurityCritical]
	[DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
	private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, int sizeOfBuffer, int count, string format, __arglist);

	public static int snwprintf_s([NotNull] StringBuilder str, [NotNull] string format, [NotNull] params object[] parameters)
	{
		if (string.IsNullOrEmpty(format)) return 0;
		if (parameters.Length != 0) return _snwprintf_s(str, str.Capacity, parameters.Length, format, __arglist(parameters));
		str.Append(format);
		return format.Length;
	}
}