using System;

namespace essentialMix.Helpers
{
	public static class MemoryHelper
	{
		public static unsafe IntPtr CopyMemory(IntPtr source, IntPtr destination, int length)
		{
			CopyMemory((byte*) source.ToPointer(), (byte*) destination.ToPointer(), length);
			return destination;
		}

		public static unsafe byte* CopyMemory(byte* source, byte* destination, int length)
		{
			return Win32.CopyMemory(destination, source, length);
		}

		public static unsafe IntPtr SetMemory(IntPtr destination, byte filler, int length)
		{
			SetMemory((byte*) destination.ToPointer(), filler, length);
			return destination;
		}

		public static unsafe byte* SetMemory(byte* destination, byte filler, int length)
		{
			return Win32.memset(destination, filler, length);
		}
	}
}