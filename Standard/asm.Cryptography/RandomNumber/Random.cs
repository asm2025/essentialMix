using JetBrains.Annotations;

namespace asm.Cryptography.RandomNumber
{
	public class Random : RandomNumberGeneratorBase<RandomAlgorithm>
	{
		public Random()
			: base(new RandomAlgorithm())
		{
		}

		public Random(int seed)
			: base(new RandomAlgorithm(seed))
		{
		}

		public Random([NotNull] RandomAlgorithm algorithm) 
			: base(algorithm)
		{
		}

		public override object Clone() { return new Random(Algorithm); }
	}
}