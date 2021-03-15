using System;
using System.Runtime.InteropServices;

namespace essentialMix.Helpers
{
	public static class GuidHelper
	{
		public static Guid NewSequential()
		{
			int result = Win32.UuidCreateSequential(out Guid guid);
			if (result != ResultCom.S_OK && result != ResultWin32.RPC_S_UUID_LOCAL_ONLY) throw new COMException("UuidCreateSequential call failed", result);
			return guid;
		}

		public static Guid New(int seed)
		{
			Random r = new Random(seed);
			byte[] guid = new byte[16];
			r.NextBytes(guid);
			return new Guid(guid);
		}

		public static bool IsGuid(string value) { return !string.IsNullOrEmpty(value) && Guid.TryParse(value, out Guid guid) && guid != Guid.Empty; }

		public static Guid Combine(string a, string b)
		{
			return string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || !Guid.TryParse(a, out Guid ag) || !Guid.TryParse(b, out Guid bg)
						? Guid.Empty
						: Combine(ag, bg);
		}

		public static Guid Combine(Guid a, Guid b)
		{
			const int BYTE_COUNT = 16;

			byte[] buffer = new byte[BYTE_COUNT];
			byte[] ab = a.ToByteArray();
			byte[] bb = b.ToByteArray();

			for (int i = 0; i < BYTE_COUNT; i++)
				buffer[i] = (byte)(ab[i] ^ bb[i]);

			return new Guid(buffer);
		}
	}
}