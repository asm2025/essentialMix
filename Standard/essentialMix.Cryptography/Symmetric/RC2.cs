using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric
{
	public abstract class RC2<T> : SymmetricAlgorithmBase<T>
		where T : System.Security.Cryptography.RC2
	{
		protected RC2([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected RC2([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class RC2 : RC2<System.Security.Cryptography.RC2>
	{
		public RC2()
			: base(System.Security.Cryptography.RC2.Create())
		{
		}

		public RC2([NotNull] System.Security.Cryptography.RC2 algorithm) 
			: base(algorithm)
		{
		}

		public RC2([NotNull] Encoding encoding) 
			: base(System.Security.Cryptography.RC2.Create(), encoding)
		{
		}

		public RC2([NotNull] System.Security.Cryptography.RC2 algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new RC2(Algorithm, Encoding); }
	}
}