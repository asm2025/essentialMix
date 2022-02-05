using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class HMACSHA512 : HMAC<System.Security.Cryptography.HMACSHA512>
{
	public HMACSHA512()
		: base(new System.Security.Cryptography.HMACSHA512())
	{
	}

	public HMACSHA512([NotNull] System.Security.Cryptography.HMACSHA512 algorithm)
		: base(algorithm)
	{
	}

	public HMACSHA512([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.HMACSHA512(), encoding)
	{
	}

	public HMACSHA512([NotNull] System.Security.Cryptography.HMACSHA512 algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new HMACSHA512(Algorithm, Encoding); }
}