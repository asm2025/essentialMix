using System;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Settings
{
	[Serializable]
	public class Settings
	{
		[NotNull]
		private Encoding _encoding;

		/// <inheritdoc />
		public Settings()
			: this(Encoding.Unicode)
		{
		}

		public Settings(Encoding encoding)
		{
			_encoding = encoding ?? Encoding.Unicode;
		}

		[NotNull]
		public virtual Encoding Encoding
		{
			get => _encoding; 
			set => _encoding = value;
		}

		public ushort SaltSize { get; set; }

		public ushort RFC2898Iterations { get; set; }
	}
}