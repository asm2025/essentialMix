using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Cryptography.Symmetric;

/*
	Usage
		x.KeySize
		x.BlockSize
		x.FeedbackSize
		x.Key
		x.IV
		x.Padding
		x.Mode
		
		x.GenerateKey();
		x.GenerateKey(string, int, int);
		x.GenerateKey(string, byte[], int);

		x.GenerateIV();
		
		x.ValidKeySize(int)

		x.Encrypt(plainText);
		x.Encrypt(byte[]);
		x.Encrypt(byte[], int, int);
		x.Decrypt(encryptedText);
		x.Decrypt(byte[]);
		x.Decrypt(byte[], int, int);
*/
public abstract class SymmetricAlgorithmBase<T> : EncryptBase<T>, ISymmetricAlgorithm
	where T : SymmetricAlgorithm
{
	protected SymmetricAlgorithmBase([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected SymmetricAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	[NotNull]
	public byte[] Key
	{
		get => Algorithm.Key;
		set => Algorithm.Key = value;
	}

	[NotNull]
	public byte[] IV
	{
		get => Algorithm.IV;
		set => Algorithm.IV = value;
	}

	public int KeySize
	{
		get => Algorithm.KeySize;
		set
		{
			if (value == 0) return;
			KeySizes minSize = KeySizes[0];
			if (value < minSize.MinSize) value = minSize.MinSize;
			int m = value % 8;
			if (m > 0) value -= m;
			Algorithm.KeySize = value;
		}
	}

	public int BlockSize
	{
		get => Algorithm.BlockSize;
		set
		{
			if (value == 0) return;
			KeySizes minSize = BlockSizes[0];
			if (value < minSize.MinSize) value = minSize.MinSize;
			int m = value % 8;
			if (m > 0) value -= m;
			Algorithm.BlockSize = value;
		}
	}

	[NotNull]
	public KeySizes[] KeySizes => Algorithm.LegalKeySizes;

	[NotNull]
	public KeySizes[] BlockSizes => Algorithm.LegalBlockSizes;

	public int FeedbackSize
	{
		get => Algorithm.FeedbackSize;
		set => Algorithm.FeedbackSize = value;
	}

	public PaddingMode Padding
	{
		get => Algorithm.Padding;
		set => Algorithm.Padding = value;
	}

	public CipherMode Mode
	{
		get => Algorithm.Mode;
		set => Algorithm.Mode = value;
	}

	public void GenerateKey() { Algorithm.GenerateKey(); }

	public virtual void GenerateKey(SecureString passphrase, ushort saltSize = 0, int iterations = 0)
	{
		if (passphrase.IsNullOrEmpty()) throw new ArgumentNullException(nameof(passphrase));

		byte[] salt = new byte[saltSize.NotBelow((ushort)8)];
		RNGRandomHelper.NextNonZeroBytes(salt);
		GenerateKey(passphrase, salt, iterations);
	}

	public virtual void GenerateKey(SecureString passphrase, byte[] salt, int iterations = 100)
	{
		if (passphrase.IsNullOrEmpty()) throw new ArgumentNullException(nameof(passphrase));
			
		using (Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(passphrase.UnSecure(), salt, iterations.NotBelow(100))) 
			Algorithm.Key = rfc2898.GetBytes(ByteHelper.GetByteSize(KeySize));
	}

	public void GenerateIV() { Algorithm.GenerateIV(); }

	public bool ValidKeySize(int bitLength) { return Algorithm.ValidKeySize(bitLength); }

	public void Clear() { Algorithm.Clear(); }

	[NotNull]
	public override byte[] Encrypt(byte[] buffer, int startIndex, int count)
	{
		buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
		if (buffer.Length == 0) return buffer;
		if (count == 0) return Array.Empty<byte>();

		using (MemoryStream ms = new MemoryStream())
		{
			using (CryptoStream cs = new CryptoStream(ms, Algorithm.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
			{
				// IV
				ms.Write(IV);
				//Data
				cs.Write(buffer, startIndex, count);
				cs.FlushFinalBlock();
				cs.Close();
				ms.Flush();
				return ms.ToArray();
			}
		}
	}

	public override byte[] Decrypt(byte[] buffer, int startIndex, int count)
	{
		buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
		if (buffer.Length == 0) return buffer;
		if (count == 0) return Array.Empty<byte>();

		int ivLen = BlockSize >> 3;
		byte[] iv = ivLen > 0 ? buffer.GetRange(startIndex, ivLen) : Array.Empty<byte>();
		startIndex += ivLen;
		count -= ivLen;

		using (MemoryStream ms = new MemoryStream(buffer, startIndex, count))
		{
			using (CryptoStream cs = new CryptoStream(ms, Algorithm.CreateDecryptor(Key, iv), CryptoStreamMode.Read))
			{
				byte[] bytes = new byte[count];
				int read = cs.Read(bytes);
				return read > 0 ? bytes : null;
			}
		}
	}
}