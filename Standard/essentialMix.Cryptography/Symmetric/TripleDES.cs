using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric
{
	public abstract class TripleDES<T> : SymmetricAlgorithmBase<T>
		where T : System.Security.Cryptography.TripleDES
	{
		protected TripleDES([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected TripleDES([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class TripleDES : TripleDES<System.Security.Cryptography.TripleDES>
	{
		public TripleDES()
			: base(System.Security.Cryptography.TripleDES.Create())
		{
		}

		public TripleDES([NotNull] System.Security.Cryptography.TripleDES algorithm) 
			: base(algorithm)
		{
		}

		public TripleDES([NotNull] Encoding encoding) 
			: base(System.Security.Cryptography.TripleDES.Create(), encoding)
		{
		}

		public TripleDES([NotNull] System.Security.Cryptography.TripleDES algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new TripleDES(Algorithm, Encoding); }
	}
}