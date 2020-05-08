using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric.Signature
{
	public class DSA : DSAAlgorithmBase<System.Security.Cryptography.DSA>
	{
		public DSA()
			: base(System.Security.Cryptography.DSA.Create())
		{
		}

		public DSA([NotNull] System.Security.Cryptography.DSA algorithm) 
			: base(algorithm)
		{
		}

		public DSA([NotNull] Encoding encoding) 
			: base(System.Security.Cryptography.DSA.Create(), encoding)
		{
		}

		public DSA([NotNull] System.Security.Cryptography.DSA algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new DSA(Algorithm, Encoding); }
	}
}