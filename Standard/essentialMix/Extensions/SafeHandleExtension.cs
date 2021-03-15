using System.Runtime.InteropServices;

namespace essentialMix.Extensions
{
	public static class SafeHandleExtension
	{
		public static bool IsAwaitable(this SafeHandle thisValue) { return thisValue != null && !thisValue.IsInvalid && !thisValue.IsClosed; }
	}
}