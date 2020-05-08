using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Symmetric
{
	public class TripleDESCng : TripleDES<System.Security.Cryptography.TripleDESCng>
	{
		public TripleDESCng()
			: base(new System.Security.Cryptography.TripleDESCng())
		{
		}

		public TripleDESCng([NotNull] System.Security.Cryptography.TripleDESCng algorithm) 
			: base(algorithm)
		{
		}

		public TripleDESCng([NotNull] Encoding encoding) 
			: base(new System.Security.Cryptography.TripleDESCng(), encoding)
		{
		}

		public TripleDESCng([NotNull] System.Security.Cryptography.TripleDESCng algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new TripleDESCng(Algorithm, Encoding); }
	}
}
