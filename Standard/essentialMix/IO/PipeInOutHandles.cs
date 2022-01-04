using System;

namespace essentialMix.IO;

public struct PipeInOutHandles
{
	public struct StdHandle
	{
		public IntPtr HRead;
		public IntPtr HWrite;
	}

	public StdHandle In;
	public StdHandle Out;

	public void Init()
	{
		In.HRead = Win32.INVALID_HANDLE_VALUE;
		In.HWrite = Win32.INVALID_HANDLE_VALUE;
		Out.HRead = Win32.INVALID_HANDLE_VALUE;
		Out.HWrite = Win32.INVALID_HANDLE_VALUE;
	}
}