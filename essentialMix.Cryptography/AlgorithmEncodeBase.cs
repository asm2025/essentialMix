using System.Text;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Text;

namespace essentialMix.Cryptography;

public abstract class AlgorithmEncodeBase<T>([NotNull] T algorithm, [NotNull] Encoding encoding)
	: AlgorithmBase<T>(algorithm), IEncoding
{
	protected AlgorithmEncodeBase([NotNull] T algorithm)
		: this(algorithm, EncodingHelper.Default)
	{
	}

	public Encoding Encoding { get; set; } = encoding;
}