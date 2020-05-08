using System;
using System.IO;
using asm.Extensions;
using asm.Helpers;
using asm.Media.Youtube.Exceptions;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal
{
	internal class AacAudioExtractor : Disposable, IAudioExtractor, IDisposable
	{
		private FileStream _fileStream;
		private int _aacProfile;
		private int _channelConfig;
		private int _sampleRateIndex;
		private readonly string _videoPath;

		public AacAudioExtractor([NotNull] string path)
		{
			_videoPath = path;
			_fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _fileStream);
			base.Dispose(disposing);
		}

		public string VideoPath
		{
			get
			{
				ThrowIfDisposed();
				return _videoPath;
			}
		}

		public void WriteChunk([NotNull] byte[] chunk, uint timeStamp)
		{
			ThrowIfDisposed();
			if (chunk.Length < 1) return;

			if (chunk[0] == 0)
			{
				// Header
				if (chunk.Length < 3) return;

				ulong bits = (ulong)chunk.ToUInt16(1) << 48;

				_aacProfile = BitHelper.Read(ref bits, 5) - 1;
				_sampleRateIndex = BitHelper.Read(ref bits, 4);
				_channelConfig = BitHelper.Read(ref bits, 4);

				if (_aacProfile < 0 || _aacProfile > 3) throw new AudioExtractionException("Unsupported AAC profile.");
				if (_sampleRateIndex > 12) throw new AudioExtractionException("Invalid AAC sample rate index.");
				if (_channelConfig > 6) throw new AudioExtractionException("Invalid AAC channel configuration.");
			}
			else
			{
				// Audio data
				int dataSize = chunk.Length - 1;
				ulong bits = 0;

				// Reference: WriteADTSHeader from FAAC's bitstream.c

				BitHelper.Write(ref bits, 12, 0xFFF);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 2, 0);
				BitHelper.Write(ref bits, 1, 1);
				BitHelper.Write(ref bits, 2, _aacProfile);
				BitHelper.Write(ref bits, 4, _sampleRateIndex);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 3, _channelConfig);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 1, 0);
				BitHelper.Write(ref bits, 13, 7 + dataSize);
				BitHelper.Write(ref bits, 11, 0x7FF);
				BitHelper.Write(ref bits, 2, 0);

				_fileStream.Write(bits.ToBytes(), 1, 7);
				_fileStream.Write(chunk, 1, dataSize);
			}
		}
	}
}