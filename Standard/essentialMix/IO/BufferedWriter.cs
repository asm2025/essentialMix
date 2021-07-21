using System;
using System.Text;
using System.Timers;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.IO
{
	public class BufferedWriter : TextWriter
	{
		protected const int BUFFER_DEFAULT = StreamHelper.BUFFER_DEFAULT;  // Byte buffer size
		protected const int BUFFER_MIN = StreamHelper.BUFFER_MIN;
		protected const int BUFFER_MAX = StreamHelper.BUFFER_MAX;

		private readonly StringBuilder _buffer;

		protected bool Receiving;

		private Timer _timer;
		private int _bufferSize;

		/// <inheritdoc />
		public BufferedWriter([NotNull] Action<string> writing)
			: this(writing, null, EncodingHelper.Default)
		{
		}

		/// <inheritdoc />
		public BufferedWriter([NotNull] Action<string> writing, IFormatProvider formatProvider)
			: this(writing, formatProvider, EncodingHelper.Default)
		{
		}

		/// <inheritdoc />
		public BufferedWriter([NotNull] Action<string> writing, [NotNull] Encoding encoding)
			: this(writing, null, encoding)
		{
		}

		/// <inheritdoc />
		public BufferedWriter([NotNull] Action<string> writing, IFormatProvider formatProvider, [NotNull] Encoding encoding)
			: base(formatProvider, encoding)
		{
			_bufferSize = BUFFER_DEFAULT;
			_buffer = new StringBuilder(_bufferSize);
			Writing = writing;
			_timer = new Timer(TimeSpanHelper.QUARTER);
			_timer.Elapsed += (_, _) =>
			{
				lock(_buffer)
				{
					if (!Receiving) return;
					if (_buffer.Length == 0) return;
				}

				FlushBuffer();
			};
			_timer.Enabled = true;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _timer);
			base.Dispose(disposing);
		}

		public virtual int BufferSize
		{
			get => _bufferSize;
			set
			{
				_bufferSize = value.Within(BUFFER_MIN, BUFFER_MAX);
				FlushBuffer();

				lock(_buffer)
					_buffer.Capacity = _bufferSize;
			}
		}

		[NotNull]
		public Action<string> Writing { get; }

		/// <inheritdoc />
		public override void Write(char value)
		{
			lock(_buffer)
			{
				_buffer.Append(value);
				Receiving = true;
				if (_buffer.Length < _bufferSize) return;
			}
			FlushBuffer();
		}

		/// <inheritdoc />
		public override void Write(string value)
		{
			lock(_buffer)
			{
				_buffer.Append(value);
				Receiving = true;
				if (_buffer.Length < _bufferSize) return;
			}
			FlushBuffer();
		}
	
		public virtual void FlushBuffer()
		{
			string value;

			lock(_buffer)
			{
				if (_buffer.Length == 0) return;
				value = _buffer.ToString();
				_buffer.Length = 0;
				Receiving = false;
			}

			Writing(value);
		}

		public virtual void Clear()
		{
			lock(_buffer)
			{
				_buffer.Length = 0;
				Receiving = false;
			}
		}
	}
}