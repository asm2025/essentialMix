using System;

namespace essentialMix.Compression
{
	public interface ICompressor : ICompressorCore, IDisposable
	{
		bool PreserveDirectoryRoot { get; set; }
	}
}