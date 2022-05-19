using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Settings;

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

	[NotNull]
	public string EncodingName
	{
		get => _encoding.WebName;
		set => _encoding = Encoding.GetEncoding(value);
	}

	public ushort SaltSize { get; set; }

	public ushort RFC2898Iterations { get; set; }
}