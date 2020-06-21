using System.Text;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Text;

namespace asm.Cryptography
{
	public abstract class AlgorithmEncodeBase<T> : AlgorithmBase<T>, IEncoding
	{
		protected AlgorithmEncodeBase([NotNull] T algorithm)
			: this(algorithm, EncodingHelper.Default)
		{
		}

		protected AlgorithmEncodeBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm)
		{
			Encoding = encoding;
		}

		public Encoding Encoding { get; set; }
	}
}