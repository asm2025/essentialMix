using System;
using System.IO;
using System.Security.Cryptography;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace essentialMix.Cryptography.Services;

public class CryptoService([NotNull] IOptions<CryptoSettings> settings) : ICryptoService
{
	[NotNull]
	public CryptoSettings Settings { get; set; } = settings.Value;

	/// <inheritdoc />
	public string Encrypt(string value)
	{
		if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

		SymmetricAlgorithm algo = null;
		ICryptoTransform encryptor = null;
		MemoryStream stream = null;
		Stream cryptoStream = null;
		StreamWriter writer = null;

		try
		{
			byte[] salt = GetSalt(Settings.SaltSize);
			algo = CreateAlgo(Settings, salt);
			algo.GenerateIV();

			byte[] iv = algo.IV;
			encryptor = algo.CreateEncryptor();
			stream = new MemoryStream();
			cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
			writer = new StreamWriter(cryptoStream) { AutoFlush = true };
			writer.Write(value);
			cryptoStream.Close();

			byte[] encrypted = stream.ToArray();
			int encryptedSize = encrypted.Length;
			int combinedSize = iv.Length + salt.Length + encrypted.Length;
			Array.Resize(ref encrypted, combinedSize);
			Buffer.BlockCopy(encrypted, 0, encrypted, iv.Length + salt.Length, encryptedSize);
			Buffer.BlockCopy(salt, 0, encrypted, 0, salt.Length);
			Buffer.BlockCopy(iv, 0, encrypted, salt.Length, iv.Length);
			return Convert.ToBase64String(encrypted);
		}
		finally
		{
			ObjectHelper.Dispose(ref encryptor);
			ObjectHelper.Dispose(ref algo);
			ObjectHelper.Dispose(ref writer);
			ObjectHelper.Dispose(ref cryptoStream);
			ObjectHelper.Dispose(ref stream);
		}
	}

	/// <inheritdoc />
	public string Decrypt(string value)
	{
		if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

		byte[] data = Convert.FromBase64String(value);
		byte[] salt = data.GetRange(0, ByteHelper.GetByteSize(Settings.SaltSize));
		byte[] iv = data.GetRange(salt.Length, ByteHelper.GetByteSize(Settings.IVSize));
		int size = data.Length - (salt.Length + iv.Length);
		Buffer.BlockCopy(data, salt.Length + iv.Length, data, 0, size);
		Array.Resize(ref data, size);

		SymmetricAlgorithm algo = null;
		ICryptoTransform decryptor = null;
		MemoryStream stream = null;
		Stream cryptoStream = null;
		StreamReader reader = null;

		try
		{
			algo = CreateAlgo(Settings, salt);
			algo.IV = iv;
			decryptor = algo.CreateDecryptor(algo.Key, algo.IV);
			stream = new MemoryStream(data);
			cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
			reader = new StreamReader(cryptoStream);
			return reader.ReadToEnd();
		}
		finally
		{
			ObjectHelper.Dispose(ref decryptor);
			ObjectHelper.Dispose(ref algo);
			ObjectHelper.Dispose(ref reader);
			ObjectHelper.Dispose(ref cryptoStream);
			ObjectHelper.Dispose(ref stream);
		}
	}

	[NotNull]
	protected virtual SymmetricAlgorithm CreateAlgo([NotNull] CryptoSettings settings, [NotNull] byte[] salt)
	{
		Aes algo = Aes.Create();
		algo.Key = CreateKey(settings.Key, settings.KeySize, salt, settings.Iterations);
		algo.Padding = PaddingMode.PKCS7;
		algo.Mode = CipherMode.CBC;
		return algo;
	}

	[NotNull]
	protected virtual byte[] CreateKey([NotNull] string base64Key, ushort size, [NotNull] byte[] salt, ushort iterations)
	{
		using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(base64Key, salt, iterations))
			return rfc2898DeriveBytes.GetBytes(ByteHelper.GetByteSize(size));
	}

	[NotNull]
	protected virtual byte[] GetSalt(ushort size)
	{
		byte[] buffer = new byte[ByteHelper.GetByteSize(size)];
		RNGRandomHelper.NextNonZeroBytes(buffer);
		return buffer;
	}
}