using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash
{
	public abstract class SHA384<T> : HashAlgorithmBase<T>
		where T : System.Security.Cryptography.SHA384
	{
		protected SHA384([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected SHA384([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class SHA384 : SHA384<SHA384Managed>
	{
		public SHA384()
			: base(new SHA384Managed())
		{
		}

		public SHA384([NotNull] SHA384Managed algorithm)
			: base(algorithm)
		{
		}

		public SHA384([NotNull] Encoding encoding) 
			: base(new SHA384Managed(), encoding)
		{
		}

		public SHA384([NotNull] SHA384Managed algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new SHA384(Algorithm, Encoding); }
	}
}