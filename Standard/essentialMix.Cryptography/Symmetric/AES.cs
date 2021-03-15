using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Symmetric
{
	public abstract class AES<T> : SymmetricAlgorithmBase<T>
		where T : Aes
	{
		protected AES([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected AES([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class AES : AES<AesManaged>
	{
		public AES()
			: base(new AesManaged())
		{
		}

		public AES([NotNull] AesManaged algorithm) 
			: base(algorithm)
		{
		}

		public AES([NotNull] Encoding encoding) 
			: base(new AesManaged(), encoding)
		{
		}

		public AES([NotNull] AesManaged algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new AES(Algorithm, Encoding); }
	}
}