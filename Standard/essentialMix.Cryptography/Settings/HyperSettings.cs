using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Settings
{
	[Serializable]
	public class HyperSettings : SymmetricSettings
	{
		/// <inheritdoc />
		public HyperSettings()
			: this(DEFAULT_KEY_SIZE, null)
		{
		}

		/// <inheritdoc />
		public HyperSettings(int keySize)
			: this(keySize, null)
		{
		}

		/// <inheritdoc />
		public HyperSettings(int keySize, Encoding encoding)
			: base(keySize, encoding)
		{
		}

		[NotNull]
		public RSASettings RSASettings { get; } = new RSASettings();
	}
}