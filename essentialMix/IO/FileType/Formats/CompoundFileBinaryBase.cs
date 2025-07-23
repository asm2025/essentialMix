using System;
using System.IO;
using System.Text;
using essentialMix.Extensions;
using essentialMix.Helpers;
using OpenMcdf;

namespace essentialMix.IO.FileType.Formats
{
	public abstract record CompoundFileBinaryBase : FileFormatBase, ICompoundStream
	{
		private readonly Guid _guid;
		private readonly string _storage;

		/// <inheritdoc />
		protected CompoundFileBinaryBase(string extension, string mimeType, Guid guid)
			: this(extension, mimeType)
		{
			_guid = guid;
		}

		/// <inheritdoc />
		protected CompoundFileBinaryBase(string extension, string mimeType, string storage)
			: this(extension, mimeType)
		{
			_storage = storage.ToNullIfEmpty();
		}

		/// <inheritdoc />
		private CompoundFileBinaryBase(string extension, string mimeType)
			: base(extension, mimeType, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1])
		{
		}

		/// <inheritdoc />
		public bool HasGuid => !_guid.IsEmpty();

		/// <inheritdoc />
		public bool HasStorage => _storage != null;

		/// <inheritdoc />
		public override bool IsMatch(Stream stream)
		{
			if (!stream.CanSeek || Signature == null || Signature.Length == 0) throw new NotSupportedException("Stream type is not supported.");
			if (Offset + Signature.Length > stream.Length) return false;

			BinaryReader reader = null;

			try
			{
				// Values in a CFB files are always little-endian.
				stream.Position = Offset;
				reader = new BinaryReader(stream, Encoding.Unicode, true);
				// Check that data has the CFB file header
				byte[] signature = reader.ReadBytes(8);
				if (!IsMatch(signature)) return false;
				// delegate the match operation to find the storage
				if (HasStorage) return true;

				/*
				 * Get sector size (2 byte uint) at offset 30 in the header
				 * Value at 1C specifies this as the power of two.
				 * The only valid values are 9 or 12, which gives 512 or 4096 byte sector size.
				 */
				stream.Position = 30;
				int sectorSize = 1 << reader.ReadUInt16();
				// Get first directory sector index at offset 48 in the header
				stream.Position = 48;
				uint dirIndex = reader.ReadUInt32();
				// File header is one sector wide. After that we can address the sector directly using the sector index
				long dirAddress = sectorSize + dirIndex * sectorSize;
				// Object type field is offset 80 bytes into the directory sector. It is a 128 bit GUID, encoded as "DWORD, WORD, WORD, BYTE[8]".
				stream.Position = dirAddress + 80;
				Guid guid = new Guid(reader.ReadInt32(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadBytes(8));
				return guid == _guid;
			}
			catch (IOException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}
			finally
			{
				ObjectHelper.Dispose(ref reader);
			}
		}

		/// <inheritdoc />
		public IDisposable GetStream(Stream stream)
		{
			try
			{
				return RootStorage.Open(stream, StorageModeFlags.LeaveOpen);
			}
			catch (EndOfStreamException)
			{
				return null;
			}
		}

		/// <inheritdoc />
		public bool IsMatch(IDisposable stream) { return stream is RootStorage cf && cf.TryOpenStream(_storage, out _); }
	}
}
