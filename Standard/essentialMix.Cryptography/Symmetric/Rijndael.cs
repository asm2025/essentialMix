using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric;

public abstract class Rijndael<T> : SymmetricAlgorithmBase<T>
	where T : System.Security.Cryptography.Rijndael
{
	protected Rijndael([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected Rijndael([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}
}

public class Rijndael : SymmetricAlgorithmBase<RijndaelManaged>
{
	public Rijndael()
		: base(new RijndaelManaged())
	{
	}

	public Rijndael([NotNull] RijndaelManaged algorithm) 
		: base(algorithm)
	{
	}

	public Rijndael([NotNull] Encoding encoding) 
		: base(new RijndaelManaged(), encoding)
	{
	}

	public Rijndael([NotNull] RijndaelManaged algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new Rijndael(Algorithm, Encoding); }
}