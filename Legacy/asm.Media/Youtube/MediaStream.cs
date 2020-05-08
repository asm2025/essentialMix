using System;
using System.IO;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
	/// <summary>
	///     Media stream
	/// </summary>
	public class MediaStream : Stream
	{
		private Stream _innerStream;

		public MediaStream([NotNull] MediaStreamInfo mediaStreamInfo, [NotNull] Stream innerStream)
		{
			Info = mediaStreamInfo ?? throw new ArgumentNullException(nameof(mediaStreamInfo));
			_innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
		}

		public override bool CanRead => _innerStream.CanRead;

		public override bool CanSeek => _innerStream.CanSeek;

		public override bool CanWrite => false;

		public override long Length => Info.ContentLength;

		public override long Position
		{
			get => _innerStream.Position;
			set => _innerStream.Position = value;
		}

		public override void Flush() { _innerStream.Flush(); }

		public override int Read(byte[] buffer, int offset, int count) { return _innerStream.Read(buffer, offset, count); }

		public override long Seek(long offset, SeekOrigin origin) { return _innerStream.Seek(offset, origin); }

		public override void SetLength(long value) { _innerStream.SetLength(value); }

		public override void Write(byte[] buffer, int offset, int count) { _innerStream.Write(buffer, offset, count); }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing) ObjectHelper.Dispose(ref _innerStream);
		}

		public MediaStreamInfo Info { get; }
	}
}