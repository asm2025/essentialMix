using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Hash
{
	public class HMACMD5 : HMAC<System.Security.Cryptography.HMACMD5>
	{
		public HMACMD5()
			: base(new System.Security.Cryptography.HMACMD5())
		{
		}

		public HMACMD5([NotNull] System.Security.Cryptography.HMACMD5 algorithm)
			: base(algorithm)
		{
		}

		public HMACMD5([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.HMACMD5(), encoding)
		{
		}

		public HMACMD5([NotNull] System.Security.Cryptography.HMACMD5 algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new HMACMD5(Algorithm, Encoding); }
	}
}