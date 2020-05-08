using asm.Numeric;

namespace asm.Cryptography.Encoders
{
	public interface INumericEncoder : IAlgorithmBase, IEncode
	{
		bool CanChange { get; }
		BitVectorMode Mode { get; set; }
	}
}