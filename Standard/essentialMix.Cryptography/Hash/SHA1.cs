using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash
{
	public abstract class SHA1<T> : HashAlgorithmBase<T>
		where T : System.Security.Cryptography.SHA1
	{
		protected SHA1([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected SHA1([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class SHA1 : SHA1<SHA1Managed>
	{
		public SHA1()
			: base(new SHA1Managed())
		{
		}

		public SHA1([NotNull] SHA1Managed algorithm)
			: base(algorithm)
		{
		}

		public SHA1([NotNull] Encoding encoding)
			: base(new SHA1Managed(), encoding)
		{
		}

		public SHA1([NotNull] SHA1Managed algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new SHA1(Algorithm, Encoding); }
	}
}