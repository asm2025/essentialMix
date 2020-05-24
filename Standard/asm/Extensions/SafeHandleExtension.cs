using System.Runtime.InteropServices;

namespace asm.Extensions
{
	public static class SafeHandleExtension
	{
		public static bool IsAwaitable(this SafeHandle thisValue) { return thisValue != null && !thisValue.IsInvalid && !thisValue.IsClosed; }
	}
}