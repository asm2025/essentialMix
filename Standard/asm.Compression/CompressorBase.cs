namespace asm.Compression
{
	public abstract class CompressorBase : CompressorCore, ICompressor
	{
		/// <inheritdoc />
		protected CompressorBase() 
		{
		}

		/// <inheritdoc />
		public bool PreserveDirectoryRoot { get; set; } = true;
	}
}