using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash;

public class HMACSHA1 : HMAC<System.Security.Cryptography.HMACSHA1>
{
	public HMACSHA1()
		: base(new System.Security.Cryptography.HMACSHA1())
	{
	}

	public HMACSHA1([NotNull] System.Security.Cryptography.HMACSHA1 algorithm)
		: base(algorithm)
	{
	}

	public HMACSHA1([NotNull] Encoding encoding) 
		: base(new System.Security.Cryptography.HMACSHA1(), encoding)
	{
	}

	public HMACSHA1([NotNull] System.Security.Cryptography.HMACSHA1 algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new HMACSHA1(Algorithm, Encoding); }
}