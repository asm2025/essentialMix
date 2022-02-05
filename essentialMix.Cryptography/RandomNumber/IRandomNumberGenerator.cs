using JetBrains.Annotations;

namespace essentialMix.Cryptography.RandomNumber;

public interface IRandomNumberGenerator : IAlgorithmBase
{
	void GetBytes([NotNull] byte[] buffer);
	void GetBytes([NotNull] byte[] buffer, int offset, int count);
	void GetNonZeroBytes([NotNull] byte[] buffer);
	double NextDouble();
	int Next();
	int Next(int maxValue);
	int Next(int minValue, int maxValue);
	byte[] GetUniqueValues(int length);
}