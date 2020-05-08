using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Hash
{
	public abstract class SHA256<T> : HashAlgorithmBase<T>
		where T : System.Security.Cryptography.SHA256
	{
		protected SHA256([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected SHA256([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class SHA256 : SHA256<SHA256Managed>
	{
		public SHA256()
			: base(new SHA256Managed())
		{
		}

		public SHA256([NotNull] SHA256Managed algorithm)
			: base(algorithm)
		{
		}

		public SHA256([NotNull] Encoding encoding) 
			: base(new SHA256Managed(), encoding)
		{
		}

		public SHA256([NotNull] SHA256Managed algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new SHA256(Algorithm, Encoding); }
	}
}