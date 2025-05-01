using System.Security.Cryptography;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Cryptography.RandomNumber;

public class RandomAlgorithm<T>([NotNull] T random) : RandomNumberGenerator
	where T : System.Random, new()
{
	private readonly object _lock = new object();
	private readonly T _random = random;

	public RandomAlgorithm()
		: this(new T())
	{
	}

	public override void GetBytes(byte[] data)
	{
		if (data.Length == 0) return;

		lock(_lock)
		{
			for (int i = 0; i < data.Length; i++)
				data[i] = (byte)_random.Next(byte.MinValue, byte.MaxValue);
		}
	}

	public override void GetNonZeroBytes(byte[] data)
	{
		if (data.Length == 0) return;

		lock(_lock)
		{
			for (int i = 0; i < data.Length; i++)
				data[i] = (byte)_random.Next(1, byte.MaxValue);
		}
	}
}

public class RandomAlgorithm : RandomAlgorithm<System.Random>
{
	public RandomAlgorithm()
		: this(RNGRandomHelper.Next())
	{
	}

	public RandomAlgorithm(int seed)
		: base(new System.Random(seed))
	{
	}

	public RandomAlgorithm([NotNull] System.Random random) 
		: base(random)
	{
	}
}