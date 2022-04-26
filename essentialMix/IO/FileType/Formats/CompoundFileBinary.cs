using System;
using System.IO;
using JetBrains.Annotations;
using OpenMcdf;

namespace essentialMix.IO.FileType.Formats
{
	public abstract record CompoundFileBinary : FileFormat, ICompoundStream
	{
		/// <inheritdoc />
		protected CompoundFileBinary(string extension, string mimeType, [NotNull] string storage)
			: base(extension, mimeType, new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 })
		{
			Storage = storage;
		}

		[NotNull]
		public string Storage { get; }

		/// <inheritdoc />
		public IDisposable GetStream(Stream stream)
		{
			try
			{
				return new CompoundFile(stream, CFSUpdateMode.ReadOnly, CFSConfiguration.LeaveOpen);
			}
			catch (EndOfStreamException)
			{
				return null;
			}
		}

		/// <inheritdoc />
		public bool IsMatch(IDisposable stream)
		{
			return stream is CompoundFile cf && cf.RootStorage.TryGetStorage(Storage, out _);
		}
	}
}
