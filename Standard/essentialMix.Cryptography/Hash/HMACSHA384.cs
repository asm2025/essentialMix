using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash
{
	public class HMACSHA384 : HMAC<System.Security.Cryptography.HMACSHA384>
	{
		public HMACSHA384()
			: base(new System.Security.Cryptography.HMACSHA384())
		{
		}

		public HMACSHA384([NotNull] System.Security.Cryptography.HMACSHA384 algorithm)
			: base(algorithm)
		{
		}

		public HMACSHA384([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.HMACSHA384(), encoding)
		{
		}

		public HMACSHA384([NotNull] System.Security.Cryptography.HMACSHA384 algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new HMACSHA384(Algorithm, Encoding); }
	}
}