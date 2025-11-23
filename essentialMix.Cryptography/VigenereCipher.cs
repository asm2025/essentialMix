using System;
using System.Text;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Cryptography;

/// <summary>
/// Implementation of Vigenère cipher using byte-wise operations (mod 256).
/// Encryption: C_i = (P_i + K_i) mod 256
/// Decryption: P_i = (C_i - K_i) mod 256
/// </summary>
public class VigenereCipher : EncryptBase<byte[]>, IVigenereCipher
{
	private byte[] _key;

	/// <summary>
	/// Initializes a new instance of the <see cref="VigenereCipher"/> class with the specified key.
	/// </summary>
	/// <param name="key">The encryption key. Must not be null or empty.</param>
	public VigenereCipher([NotNull] byte[] key)
		: this(key, EncodingHelper.Default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VigenereCipher"/> class with the specified key and encoding.
	/// </summary>
	/// <param name="key">The encryption key. Must not be null or empty.</param>
	/// <param name="encoding">The encoding to use for string operations.</param>
	public VigenereCipher([NotNull] byte[] key, [NotNull] Encoding encoding)
		: base(key, encoding)
	{
		_key = (byte[])key.Clone();
	}

	/// <inheritdoc />
	public byte[] Key
	{
		get => _key;
		set => _key = (byte[])(value ?? throw new ArgumentNullException(nameof(value))).Clone();
	}

	/// <inheritdoc />
	public override byte[] Encrypt(byte[] buffer, int startIndex, int count)
	{
		buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
		if (buffer == null || buffer.Length == 0 || count == 0)
			return buffer ?? [];

		byte[] result = new byte[count];
		int keyLength = _key.Length;

		for (int i = 0; i < count; i++)
		{
			byte plaintextByte = buffer[startIndex + i];
			byte keyByte = _key[i % keyLength];
			// Encryption: C_i = (P_i + K_i) mod 256
			result[i] = (byte)((plaintextByte + keyByte) % 256);
		}

		return result;
	}

	/// <inheritdoc />
	public override byte[] Decrypt(byte[] buffer, int startIndex, int count)
	{
		buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
		if (buffer == null || buffer.Length == 0 || count == 0)
			return buffer ?? [];

		byte[] result = new byte[count];
		int keyLength = _key.Length;

		for (int i = 0; i < count; i++)
		{
			byte cipherByte = buffer[startIndex + i];
			byte keyByte = _key[i % keyLength];
			// Decryption: P_i = (C_i - K_i) mod 256
			// Handle negative modulo by adding 256 before mod
			result[i] = (byte)((cipherByte - keyByte + 256) % 256);
		}

		return result;
	}

	/// <inheritdoc />
	public override object Clone()
	{
		return new VigenereCipher(_key, Encoding);
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_key != null)
			{
				Array.Clear(_key, 0, _key.Length);
				_key = null;
			}
		}
		base.Dispose(disposing);
	}
}

