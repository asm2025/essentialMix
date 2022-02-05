using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class HMACSHA256 : HMAC<System.Security.Cryptography.HMACSHA256>
{
	public HMACSHA256()
		: base(new System.Security.Cryptography.HMACSHA256())
	{
	}

	public HMACSHA256([NotNull] System.Security.Cryptography.HMACSHA256 algorithm)
		: base(algorithm)
	{
	}

	public HMACSHA256([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.HMACSHA256(), encoding)
	{
	}

	public HMACSHA256([NotNull] System.Security.Cryptography.HMACSHA256 algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new HMACSHA256(Algorithm, Encoding); }
}