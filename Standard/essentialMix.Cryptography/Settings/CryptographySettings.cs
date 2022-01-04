using System;
using System.Text;
using essentialMix.Helpers;

namespace essentialMix.Cryptography.Settings;

[Serializable]
public class CryptographySettings : Settings
{
	private int _keySize;

	/// <inheritdoc />
	public CryptographySettings()
		: this(0, null)
	{
	}

	/// <inheritdoc />
	public CryptographySettings(int keySize)
		: this(keySize, null)
	{
	}

	/// <inheritdoc />
	public CryptographySettings(int keySize, Encoding encoding)
		: base(encoding)
	{
		_keySize = keySize;
	}

	public virtual int KeySize
	{
		get => _keySize; 
		set => _keySize = value;
	}

	public bool UseExpiration { get; set; }
	public TimeSpan? Timeout { get; set; }

	public DateTime GetExpiration()
	{
		TimeSpan timeout = Timeout ?? TimeSpanHelper.Minute;
		if (timeout <= TimeSpan.Zero) timeout = TimeSpanHelper.Minute;
		return DateTime.UtcNow.Add(timeout);
	}
}