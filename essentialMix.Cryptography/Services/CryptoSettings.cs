using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Services;

[Serializable]
public class CryptoSettings
{
	[NotNull]
	private Encoding _encoding;

	/// <inheritdoc />
	public CryptoSettings()
		: this(Encoding.Unicode)
	{
	}

	public CryptoSettings(Encoding encoding)
	{
		_encoding = encoding ?? Encoding.Unicode;
	}

	[NotNull]
	public virtual Encoding Encoding
	{
		get => _encoding;
		set => _encoding = value;
	}

	[NotNull]
	public string EncodingName
	{
		get => _encoding.WebName;
		set => _encoding = Encoding.GetEncoding(value);
	}

	public string Key { get; set; }
	public ushort KeySize { get; set; } = 256;
	public ushort IVSize { get; set; } = 128;
	public ushort SaltSize { get; set; } = 256;

	public ushort Iterations { get; set; } = 1000;
}