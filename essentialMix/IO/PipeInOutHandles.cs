using System;
using essentialMix.Helpers;

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
		In.HRead = IntPtrHelper.INVALID_HANDLE_VALUE;
		In.HWrite = IntPtrHelper.INVALID_HANDLE_VALUE;
		Out.HRead = IntPtrHelper.INVALID_HANDLE_VALUE;
		Out.HWrite = IntPtrHelper.INVALID_HANDLE_VALUE;
	}
}