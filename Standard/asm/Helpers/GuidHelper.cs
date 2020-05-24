using System;
using System.Runtime.InteropServices;

namespace asm.Helpers
{
	public static class GuidHelper
	{
		public static Guid NewSequential()
		{
			int result = Win32.UuidCreateSequential(out Guid guid);
			if (result != Win32.ResultCom.S_OK && result != Win32.ResultWin32.RPC_S_UUID_LOCAL_ONLY) throw new COMException("UuidCreateSequential call failed", result);
			return guid;
		}

		public static Guid New(int seed)
		{
			Random r = new Random(seed);
			byte[] guid = new byte[16];
			r.NextBytes(guid);
			return new Guid(guid);
		}

		public static bool IsGuid(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;

			return Guid.TryParse(value, out Guid guid) && guid != Guid.Empty;
		}
	}
}