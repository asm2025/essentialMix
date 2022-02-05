using System;
using System.Security.Cryptography;
using System.Text;
using essentialMix.Extensions;

namespace essentialMix.Cryptography.Settings;

[Serializable]
public class SymmetricSettings : CryptographySettings
{
	public const int DEFAULT_KEY_SIZE = 256;
	public const CipherMode DEFAULT_MODE = CipherMode.CBC;
	public const PaddingMode DEFAULT_PADDING_MODE = PaddingMode.PKCS7;

	/// <inheritdoc />
	public SymmetricSettings()
		: this(DEFAULT_KEY_SIZE, null)
	{
	}

	/// <inheritdoc />
	public SymmetricSettings(int keySize)
		: this(keySize, null)
	{
	}

	/// <inheritdoc />
	public SymmetricSettings(int keySize, Encoding encoding)
		: base(keySize, encoding)
	{
	}

	/// <inheritdoc />
	public override int KeySize
	{
		get => base.KeySize;
		set
		{
			base.KeySize = value;
			BlockSize = (int)Math.Floor(base.KeySize.NotBelow(0) / 2.0d);
		}
	}

	public virtual int BlockSize { get; set; }
	public virtual CipherMode Mode { get; set; } = DEFAULT_MODE;
	public virtual PaddingMode PaddingMode { get; set; } = DEFAULT_PADDING_MODE;
}