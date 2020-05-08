using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	public class DSACng : DSAAlgorithmBase<System.Security.Cryptography.DSACng>
	{
		public DSACng()
			: base(new System.Security.Cryptography.DSACng())
		{
		}

		public DSACng([NotNull] System.Security.Cryptography.DSACng algorithm) 
			: base(algorithm)
		{
		}

		public DSACng([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.DSACng(), encoding)
		{
		}

		public DSACng([NotNull] System.Security.Cryptography.DSACng algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new DSACng(Algorithm, Encoding); }
	}
}
