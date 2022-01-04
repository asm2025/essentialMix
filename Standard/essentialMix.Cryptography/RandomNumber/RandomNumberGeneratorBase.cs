using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.RandomNumber;

/*
	Usage
		x.GetBytes(byte[]);
		x.GetBytes(byte[], int, int);
		x.GetNonZeroBytes(byte[]);
*/
public abstract class RandomNumberGeneratorBase<T> : AlgorithmBase<T>, IRandomNumberGenerator
	where T : RandomNumberGenerator
{
	protected RandomNumberGeneratorBase([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	public void GetBytes(byte[] buffer) { Algorithm.GetBytes(buffer); }
	public void GetBytes(byte[] buffer, int offset, int count) { Algorithm.GetBytes(buffer, offset, count); }
	public void GetNonZeroBytes(byte[] buffer) { Algorithm.GetNonZeroBytes(buffer); }

	public double NextDouble()
	{
		byte[] bytes = new byte[Constants.INT_SIZE];
		Algorithm.GetBytes(bytes);
		return (double)BitConverter.ToUInt32(bytes, 0) / uint.MaxValue;
	}

	public int Next() { return Next(0, int.MaxValue); }

	public int Next(int maxValue) { return Next(0, maxValue); }

	public int Next(int minValue, int maxValue)
	{
		long range = (long)maxValue - minValue;
		return (int)((long)Math.Floor(NextDouble() * range) + minValue);
	}

	[NotNull]
	public byte[] GetUniqueValues(int length)
	{
		if (length is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(length), $"Length {length} cannot yield unique values.");
			
		switch (length)
		{
			case 0:
				return Array.Empty<byte>();
			case 1:
				return new[] { (byte)Next(length) };
			default:
				ISet<byte> bytes = new HashSet<byte>();

				do
				{
					bytes.Add((byte)Next(length));
				}
				while (bytes.Count < length);

				return bytes.ToArray();
		}
	}
}