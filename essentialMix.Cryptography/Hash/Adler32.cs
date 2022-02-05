using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public abstract class Adler32<T> : HashAlgorithmBase<T>
	where T : Adler32Algorithm
{
	protected Adler32([NotNull] T algorithm) 
		: base(algorithm)
	{
	}

	protected Adler32([NotNull] T algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}
}

/// <inheritdoc />
public class Adler32 : HashAlgorithmBase<Adler32Algorithm>
{
	public Adler32() 
		: base(new Adler32Algorithm())
	{
	}

	public Adler32([NotNull] Adler32Algorithm algorithm) 
		: base(algorithm)
	{
	}

	public Adler32([NotNull] Encoding encoding) 
		: base(new Adler32Algorithm(), encoding)
	{
	}

	public Adler32([NotNull] Adler32Algorithm algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new Adler32(Algorithm, Encoding); }
}