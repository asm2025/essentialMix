using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders
{
	public interface INumericEncoder : IAlgorithmBase, IEncode
	{
		bool CanChange { get; }
		BitVectorMode Mode { get; set; }
	}
}