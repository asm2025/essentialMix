using System;

namespace asm.Compression
{
	public interface ICompressor : ICompressorCore, IDisposable
	{
		bool PreserveDirectoryRoot { get; set; }
	}
}