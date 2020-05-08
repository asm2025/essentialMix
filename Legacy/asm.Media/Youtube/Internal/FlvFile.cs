using System;
using System.IO;
using asm.Events;
using asm.Extensions;
using asm.Helpers;
using asm.Media.Youtube.Exceptions;
using asm.Patterns.Object;
using JetBrains.Annotations;

namespace asm.Media.Youtube.Internal
{
	internal class FlvFile : Disposable, IDisposable
	{
		private readonly long _fileLength;
		private readonly string _outputPath;
		private IAudioExtractor _audioExtractor;
		private long _fileOffset;
		private FileStream _fileStream;
		private bool _extractedAudio;

		/// <summary>
		///     Initializes a new instance of the <see cref="FlvFile" /> class.
		/// </summary>
		/// <param name="inputPath">The path of the input.</param>
		/// <param name="outputPath">The path of the output without extension.</param>
		public FlvFile([NotNull] string inputPath, string outputPath)
		{
			_outputPath = outputPath;
			_fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024);
			_fileOffset = 0;
			_fileLength = _fileStream.Length;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_fileStream != null)
				{
					_fileStream.Close();
					_fileStream = null;
				}

				CloseOutput(true);
			}

			base.Dispose(disposing);
		}

		public event EventHandler<ProgressEventArgs> ConversionProgressChanged;

		public bool ExtractedAudio
		{
			get
			{
				ThrowIfDisposed();
				return _extractedAudio;
			}
			private set
			{
				ThrowIfDisposed();
				_extractedAudio = value;
			}
		}

		/// <exception cref="AudioExtractionException">The input file is not an FLV file.</exception>
		public void ExtractStreams()
		{
			ThrowIfDisposed();
			Seek(0);

			if (ReadUInt32() != 0x464C5601)
			{
				// not a FLV file
				throw new AudioExtractionException("Invalid input file. Impossible to extract audio track.");
			}

			ReadUInt8();
			uint dataOffset = ReadUInt32();

			Seek(dataOffset);

			ReadUInt32();

			while (_fileOffset < _fileLength)
			{
				if (!ReadTag())
				{
					break;
				}

				if (_fileLength - _fileOffset < 4)
				{
					break;
				}

				ReadUInt32();
				if (ConversionProgressChanged == null) continue;

				int progress = (int)((double)_fileOffset / _fileLength * 100);
				ConversionProgressChanged.Invoke(this, new ProgressEventArgs(progress));
			}

			CloseOutput(false);
		}

		private void CloseOutput(bool disposing)
		{
			if (_audioExtractor != null)
			{
				if (disposing && _audioExtractor.VideoPath != null)
				{
					try
					{
						File.Delete(_audioExtractor.VideoPath);
					}
					catch { }
				}

				ObjectHelper.Dispose(ref _audioExtractor);
			}
		}

		[NotNull]
		private IAudioExtractor GetAudioWriter(uint mediaInfo)
		{
			uint format = mediaInfo >> 4;

			switch (format)
			{
				case 14:
				case 2:
					return new Mp3AudioExtractor(_outputPath);
				case 10:
					return new AacAudioExtractor(_outputPath);
			}

			string typeStr;

			switch (format)
			{
				case 1:
					typeStr = "ADPCM";
					break;

				case 6:
				case 5:
				case 4:
					typeStr = "Nellymoser";
					break;

				default:
					typeStr = "format=" + format;
					break;
			}

			throw new AudioExtractionException($"Unable to extract audio ({typeStr} is unsupported).");
		}

		[NotNull]
		private byte[] ReadBytes(int length)
		{
			byte[] buff = new byte[length];
			_fileStream.Read(buff);
			_fileOffset += length;
			return buff;
		}

		private bool ReadTag()
		{
			if (_fileLength - _fileOffset < 11) return false;

			// Read tag header
			uint tagType = ReadUInt8();
			uint dataSize = ReadUInt24();
			uint timeStamp = ReadUInt24();
			timeStamp |= ReadUInt8() << 24;
			ReadUInt24();

			// Read tag data
			if (dataSize == 0) return true;

			if (_fileLength - _fileOffset < dataSize) return false;

			uint mediaInfo = ReadUInt8();
			dataSize -= 1;
			byte[] data = ReadBytes((int)dataSize);

			if (tagType == 0x8)
			{
				// If we have no audio writer, create one
				if (_audioExtractor == null)
				{
					_audioExtractor = GetAudioWriter(mediaInfo);
					ExtractedAudio = _audioExtractor != null;
				}

				if (_audioExtractor == null)
				{
					throw new InvalidOperationException("No supported audio writer found.");
				}

				_audioExtractor.WriteChunk(data, timeStamp);
			}

			return true;
		}

		private uint ReadUInt24()
		{
			byte[] x = new byte[Constants.INT_SIZE];
			_fileStream.Read(x, 1, Constants.INT_24_SIZE);
			_fileOffset += Constants.INT_24_SIZE;
			return x.ToUInt32();
		}

		private uint ReadUInt32()
		{
			byte[] x = new byte[Constants.INT_SIZE];
			_fileStream.Read(x);
			_fileOffset += Constants.INT_SIZE;
			return x.ToUInt32();
		}

		private uint ReadUInt8()
		{
			_fileOffset += 1;
			return (uint)_fileStream.ReadByte();
		}

		private void Seek(long offset)
		{
			_fileStream.Seek(offset, SeekOrigin.Begin);
			_fileOffset = offset;
		}
	}
}