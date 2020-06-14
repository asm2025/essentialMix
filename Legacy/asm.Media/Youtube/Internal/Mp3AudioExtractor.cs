using System;
using System.Collections.Generic;
using System.IO;
using asm.Extensions;
using asm.Helpers;
using asm.Other.Moitah;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal
{
	internal class Mp3AudioExtractor : Disposable, IAudioExtractor, IDisposable
	{
		private readonly List<byte[]> _chunkBuffer;
		private readonly List<uint> _frameOffsets;
		private readonly List<string> _warnings;
		private FileStream _fileStream;
		private int _channelMode;
		private bool _delayWrite;
		private int _firstBitRate;
		private uint _firstFrameHeader;
		private bool _hasVbrHeader;
		private bool _isVbr;
		private int _mpegVersion;
		private int _sampleRate;
		private uint _totalFrameLength;
		private bool _writeVbrHeader;
		private readonly string _videoPath;

		public Mp3AudioExtractor([NotNull] string path)
		{
			_videoPath = path;
			_fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024);
			_warnings = new List<string>();
			_chunkBuffer = new List<byte[]>();
			_frameOffsets = new List<uint>();
			_delayWrite = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();

				if (_writeVbrHeader)
				{
					_fileStream.Seek(0, SeekOrigin.Begin);
					WriteVbrHeader(false);
				}

				ObjectHelper.Dispose(ref _fileStream);
			}

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

		public IEnumerable<string> Warnings
		{
			get
			{
				ThrowIfDisposed();
				return _warnings;
			}
		}

		public void WriteChunk([NotNull] byte[] chunk, uint timeStamp)
		{
			ThrowIfDisposed();
			_chunkBuffer.Add(chunk);
			ParseMp3Frames(chunk);

			if (_delayWrite && _totalFrameLength >= 65536)
			{
				_delayWrite = false;
			}

			if (!_delayWrite)
			{
				Flush();
			}
		}

		private void Flush()
		{
			foreach (byte[] chunk in _chunkBuffer)
			{
				_fileStream.Write(chunk);
			}

			_chunkBuffer.Clear();
		}

		private void ParseMp3Frames([NotNull] byte[] buffer)
		{
			int[] mpeg1BitRate = {0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0};
			int[] mpeg2XBitRate = {0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0};
			int[] mpeg1SampleRate = {44100, 48000, 32000, 0};
			int[] mpeg20SampleRate = {22050, 24000, 16000, 0};
			int[] mpeg25SampleRate = {11025, 12000, 8000, 0};

			int offset = 0;
			int length = buffer.Length;

			while (length >= 4)
			{
				ulong header = (ulong)buffer.ToUInt32(offset) << 32;

				if (BitHelper.Read(ref header, 11) != 0x7FF)
				{
					break;
				}

				int mpegVersion = BitHelper.Read(ref header, 2);
				int layer = BitHelper.Read(ref header, 2);
				BitHelper.Read(ref header, 1);
				int bitRate = BitHelper.Read(ref header, 4);
				int sampleRate = BitHelper.Read(ref header, 2);
				int padding = BitHelper.Read(ref header, 1);
				BitHelper.Read(ref header, 1);
				int channelMode = BitHelper.Read(ref header, 2);

				if (mpegVersion == 1 || layer != 1 || bitRate == 0 || bitRate == 15 || sampleRate == 3)
				{
					break;
				}

				bitRate = (mpegVersion == 3 ? mpeg1BitRate[bitRate] : mpeg2XBitRate[bitRate]) * 1000;

				switch (mpegVersion)
				{
					case 2:
						sampleRate = mpeg20SampleRate[sampleRate];
						break;

					case 3:
						sampleRate = mpeg1SampleRate[sampleRate];
						break;

					default:
						sampleRate = mpeg25SampleRate[sampleRate];
						break;
				}

				int frameLength = GetFrameLength(mpegVersion, bitRate, sampleRate, padding);

				if (frameLength > length)
				{
					break;
				}

				bool isVbrHeaderFrame = false;

				if (_frameOffsets.Count == 0)
				{
					// Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
					int o = offset + GetFrameDataOffset(mpegVersion, channelMode);

					if (buffer.ToUInt32(o) == 0x58696E67)
					{
						// "Xing"
						isVbrHeaderFrame = true;
						_delayWrite = false;
						_hasVbrHeader = true;
					}
				}

				if (!isVbrHeaderFrame)
				{
					if (_firstBitRate == 0)
					{
						_firstBitRate = bitRate;
						_mpegVersion = mpegVersion;
						_sampleRate = sampleRate;
						_channelMode = channelMode;
						_firstFrameHeader = buffer.ToUInt32(offset);
					}

					else if (!_isVbr && bitRate != _firstBitRate)
					{
						_isVbr = true;

						if (!_hasVbrHeader)
						{
							if (_delayWrite)
							{
								WriteVbrHeader(true);
								_writeVbrHeader = true;
								_delayWrite = false;
							}

							else
							{
								_warnings.Add("Detected VBR too late, cannot add VBR header.");
							}
						}
					}
				}

				_frameOffsets.Add(_totalFrameLength + (uint)offset);

				offset += frameLength;
				length -= frameLength;
			}

			_totalFrameLength += (uint)buffer.Length;
		}

		private void WriteVbrHeader(bool isPlaceholder)
		{
			byte[] buffer = new byte[GetFrameLength(_mpegVersion, 64000, _sampleRate, 0)];

			if (!isPlaceholder)
			{
				uint header = _firstFrameHeader;
				int dataOffset = GetFrameDataOffset(_mpegVersion, _channelMode);
				header &= 0xFFFE0DFF; // Clear CRC, bitrate, and padding fields
				header |= (uint)(_mpegVersion == 3 ? 5 : 8) << 12; // 64 kbit/sec
				BitHelper.CopyBytes(buffer, 0, header.ToBytes());
				BitHelper.CopyBytes(buffer, dataOffset, 0x58696E67.ToBytes()); // "Xing"
				BitHelper.CopyBytes(buffer, dataOffset + 4, ((uint)0x7).ToBytes()); // Flags
				BitHelper.CopyBytes(buffer, dataOffset + 8, ((uint)_frameOffsets.Count).ToBytes()); // Frame count
				BitHelper.CopyBytes(buffer, dataOffset + 12, _totalFrameLength.ToBytes()); // File length

				for (int i = 0; i < 100; i++)
				{
					int frameIndex = (int)(i / 100.0 * _frameOffsets.Count);

					buffer[dataOffset + 16 + i] = (byte)(_frameOffsets[frameIndex] / (double)_totalFrameLength * 256.0);
				}
			}

			_fileStream.Write(buffer);
		}

		private static int GetFrameDataOffset(int mpegVersion, int channelMode)
		{
			return 4 +
					(mpegVersion == 3
						? channelMode == 3
							? 17
							: 32
						: channelMode == 3
							? 9
							: 17);
		}

		private static int GetFrameLength(int mpegVersion, int bitRate, int sampleRate, int padding)
		{
			return (mpegVersion == 3
						? 144
						: 72) *
					bitRate /
					sampleRate +
					padding;
		}
	}
}