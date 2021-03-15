using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Hash
{
	/// <inheritdoc />
	public abstract class MD5<T> : HashAlgorithmBase<T>
		where T : System.Security.Cryptography.MD5
	{
		protected MD5([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected MD5([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}
	}

	public class MD5 : MD5<System.Security.Cryptography.MD5>
	{
		public MD5()
			: base(System.Security.Cryptography.MD5.Create())
		{
		}

		public MD5([NotNull] System.Security.Cryptography.MD5 algorithm)
			: base(algorithm)
		{
		}

		public MD5([NotNull] Encoding encoding) 
			: base(System.Security.Cryptography.MD5.Create(), encoding)
		{
		}

		public MD5([NotNull] System.Security.Cryptography.MD5 algorithm, [NotNull] Encoding encoding) 
			: base(algorithm, encoding)
		{
		}

		public override object Clone() { return new MD5(Algorithm, Encoding); }
	}
}