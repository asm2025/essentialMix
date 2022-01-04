using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public abstract class HMAC<T> : KeyedHashAlgorithmBase<T>
	where T : System.Security.Cryptography.HMAC
{
	protected HMAC([NotNull] T algorithm)
		: base(algorithm)
	{
	}

	protected HMAC([NotNull] T algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}
}

public class HMAC : HMAC<System.Security.Cryptography.HMAC>
{
	public HMAC()
		: base(System.Security.Cryptography.HMAC.Create())
	{
	}

	public HMAC([NotNull] System.Security.Cryptography.HMAC algorithm)
		: base(algorithm)
	{
	}

	public HMAC([NotNull] Encoding encoding)
		: base(System.Security.Cryptography.HMAC.Create(), encoding)
	{
	}

	public HMAC([NotNull] System.Security.Cryptography.HMAC algorithm, [NotNull] Encoding encoding)
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new KeyedHashAlgorithmBase(Algorithm, Encoding); }
}