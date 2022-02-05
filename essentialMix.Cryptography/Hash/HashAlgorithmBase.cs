using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

/*
	Usage
		x.ComputeHash(value);
		x.ComputeHash(byte[]);
		x.ComputeHash(byte[], int, int)
		x.ComputeHash(global::System.IO.Stream)
*/
public abstract class HashAlgorithmBase<T> : AlgorithmEncodeBase<T>, IHashAlgorithm
	where T : HashAlgorithm
{
	protected HashAlgorithmBase([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected HashAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	public string ComputeHash(string value)
	{
		return string.IsNullOrEmpty(value) 
					? value 
					: string.Concat(ComputeHash(Encoding.GetBytes(value))
										.Select(b => b.ToString("x2")));
	}

	public int HashSize => Algorithm.HashSize;

	public int InputBlockSize => Algorithm.InputBlockSize;

	public int OutputBlockSize => Algorithm.OutputBlockSize;

	public bool CanReuseTransform => Algorithm.CanReuseTransform;

	public bool CanTransformMultipleBlocks => Algorithm.CanTransformMultipleBlocks;

	[NotNull]
	public byte[] Hash => Algorithm.Hash;

	public void Clear() {  Algorithm.Clear(); }

	[NotNull]
	public byte[] ComputeHash(byte[] buffer) { return Algorithm.ComputeHash(buffer); }

	[NotNull]
	public byte[] ComputeHash(byte[] buffer, int offset, int count) { return Algorithm.ComputeHash(buffer, offset, count); }

	[NotNull]
	public byte[] ComputeHash(Stream inputStream) { return Algorithm.ComputeHash(inputStream); }

	public int TransformBlock(byte[] buffer, int offset, int count, byte[] outputBuffer, int outputOffset)
	{
		return Algorithm.TransformBlock(buffer, offset, count, outputBuffer, outputOffset);
	}

	[NotNull]
	public byte[] TransformFinalBlock(byte[] buffer, int offset, int count) { return Algorithm.TransformFinalBlock(buffer, offset, count); }

	public static byte[] ComputeHash([NotNull] Func<HashAlgorithmBase<T>> creator, string value)
	{
		return string.IsNullOrEmpty(value)
					? Array.Empty<byte>()
					: ComputeHash(creator, new[]
						{
							value
						})
						.FirstOrDefault();
	}

	public static IEnumerable<byte[]> ComputeHash([NotNull] Func<HashAlgorithmBase<T>> creator, [NotNull] params string[] values)
	{
		if (values.Length == 0) yield break;

		using (HashAlgorithmBase<T> encryptor = creator())
		{
			foreach (string value in values)
				yield return encryptor.ComputeHash(encryptor.Encoding.GetBytes(value));
		}
	}
}