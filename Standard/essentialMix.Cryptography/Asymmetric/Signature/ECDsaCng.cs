using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric.Signature
{
	public class ECDsaCng : ECDsaAlgorithmBase<System.Security.Cryptography.ECDsaCng>
	{
		public ECDsaCng()
			: base(new System.Security.Cryptography.ECDsaCng())
		{
		}

		public ECDsaCng([NotNull] System.Security.Cryptography.ECDsaCng algorithm) 
			: base(algorithm)
		{
		}

		public ECDsaCng([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.ECDsaCng(), encoding)
		{
		}

		public ECDsaCng([NotNull] System.Security.Cryptography.ECDsaCng algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new ECDsaCng(Algorithm, Encoding); }
	}
}