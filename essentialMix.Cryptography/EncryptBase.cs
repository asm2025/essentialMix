using System;
using System.Text;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Cryptography;

public abstract class EncryptBase<T> : AlgorithmEncodeBase<T>, IEncrypt
{
	protected EncryptBase([NotNull] T algorithm)
		: this(algorithm, EncodingHelper.Default)
	{
	}

	protected EncryptBase([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm)
	{
		Encoding = encoding;
	}

	/// <inheritdoc />
	public virtual string Encrypt(string value)
	{
		return string.IsNullOrEmpty(value)
					? value
					: Convert.ToBase64String(Encrypt(Encoding.GetBytes(value)));
	}

	/// <inheritdoc />
	public byte[] Encrypt(byte[] buffer) { return Encrypt(buffer, 0, buffer.Length); }

	/// <inheritdoc />
	public abstract byte[] Encrypt(byte[] buffer, int startIndex, int count);

	/// <inheritdoc />
	public virtual string Decrypt(string value)
	{
		if (string.IsNullOrEmpty(value)) return value;
		byte[] encryptedBytes = Convert.FromBase64String(value);
		return Encoding.GetString(Decrypt(encryptedBytes)).TrimEnd('\0');
	}

	/// <inheritdoc />
	public byte[] Decrypt(byte[] buffer) { return Decrypt(buffer, 0, buffer.Length); }

	/// <inheritdoc />
	public abstract byte[] Decrypt(byte[] buffer, int startIndex, int count);

	[NotNull]
	public string RandomString(int length)
	{
		if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
		if (length == 0) return string.Empty;

		int byteLength = Encoding.GetMaxByteCount(length);
		byte[] bytes = new byte[byteLength];
		RNGRandomHelper.NextNonZeroBytes(bytes);
		return Encoding.GetString(bytes).Left(length);
	}
}