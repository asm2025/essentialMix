using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Symmetric
{
	public abstract class DES<T> : SymmetricAlgorithmBase<T>
		where T : System.Security.Cryptography.DES
	{
		protected DES([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected DES([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class DES : DES<System.Security.Cryptography.DES>
	{
		public DES()
			: base(System.Security.Cryptography.DES.Create())
		{
		}

		public DES([NotNull] System.Security.Cryptography.DES algorithm) 
			: base(algorithm)
		{
		}

		public DES([NotNull] Encoding encoding) 
			: base(System.Security.Cryptography.DES.Create(), encoding)
		{
		}

		public DES([NotNull] System.Security.Cryptography.DES algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new DES(Algorithm, Encoding); }
	}
}