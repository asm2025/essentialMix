using System;
using System.Text;
using asm.Extensions;
using asm.Helpers;

namespace asm.IO
{
	public class IOSettings
	{
		public const int BUFFER_MINIMUM = Constants.BUFFER_KB;
		public const int BUFFER_DEFAULT = Constants.BUFFER_KB * 20;

		private int _bufferSize = BUFFER_DEFAULT;
		private Encoding _encoding = EncodingHelper.Default;

		/// <inheritdoc />
		public IOSettings()
		{
		}

		/// <inheritdoc />
		public IOSettings(IOSettings settings)
		{
			if (settings == null) return;
			BufferSize = settings.BufferSize;
			Encoding = settings.Encoding;
		}

		public int BufferSize
		{
			get => _bufferSize;
			set => _bufferSize = value.NotBelow(BUFFER_MINIMUM);
		}

		public Encoding Encoding
		{
			get => _encoding;
			set => _encoding = value ?? EncodingHelper.Default;
		}

		public Action<Exception> OnError { get; set; }
	}
}