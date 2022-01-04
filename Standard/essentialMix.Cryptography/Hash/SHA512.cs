using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public abstract class SHA512<T> : HashAlgorithmBase<T>
	where T : System.Security.Cryptography.SHA512
{
	protected SHA512([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected SHA512([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}
}

public class SHA512 : SHA512<SHA512Managed>
{
	public SHA512()
		: base(new SHA512Managed())
	{
	}

	public SHA512([NotNull] SHA512Managed algorithm)
		: base(algorithm)
	{
	}

	public SHA512([NotNull] Encoding encoding) 
		: base(new SHA512Managed(), encoding)
	{
	}

	public SHA512([NotNull] SHA512Managed algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new SHA512(Algorithm, Encoding); }
}