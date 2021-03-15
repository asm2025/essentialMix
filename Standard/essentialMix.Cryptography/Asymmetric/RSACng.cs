using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric
{
	public class RSACng : RSAAlgorithmBase<System.Security.Cryptography.RSACng>
	{
		public RSACng()
			: base(new System.Security.Cryptography.RSACng())
		{
		}

		public RSACng([NotNull] System.Security.Cryptography.RSACng algorithm) 
			: base(algorithm)
		{
		}

		public RSACng([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.RSACng(), encoding)
		{
		}

		public RSACng([NotNull] System.Security.Cryptography.RSACng algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public CngKey Key => Algorithm.Key;

		public override object Clone() { return new RSACng(Algorithm, Encoding); }
	}
}