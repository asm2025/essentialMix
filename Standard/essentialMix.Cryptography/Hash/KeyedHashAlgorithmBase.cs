using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public abstract class KeyedHashAlgorithmBase<T> : HashAlgorithmBase<T>
	where T : KeyedHashAlgorithm
{
	protected KeyedHashAlgorithmBase([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected KeyedHashAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}
}

public class KeyedHashAlgorithmBase : KeyedHashAlgorithmBase<KeyedHashAlgorithm>
{
	public KeyedHashAlgorithmBase()
		: base(KeyedHashAlgorithm.Create())
	{
	}

	public KeyedHashAlgorithmBase([NotNull] KeyedHashAlgorithm algorithm)
		: base(algorithm)
	{
	}

	public KeyedHashAlgorithmBase([NotNull] Encoding encoding)
		: base(KeyedHashAlgorithm.Create(), encoding)
	{
	}

	public KeyedHashAlgorithmBase([NotNull] KeyedHashAlgorithm algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new KeyedHashAlgorithmBase(Algorithm, Encoding); }
}