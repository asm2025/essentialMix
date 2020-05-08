using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace asm.Cryptography.Asymmetric
{
	/*
		Usage
			x.FromXmlString(string);
			x.ToXmlString(bool);
	*/
	public abstract class AsymmetricAlgorithmBase<T> : EncryptBase<T>, IAsymmetricAlgorithm
		where T : AsymmetricAlgorithm
	{
		protected AsymmetricAlgorithmBase([NotNull] T algorithm)
			: base(algorithm)
		{
		}

		protected AsymmetricAlgorithmBase([NotNull] T algorithm, [NotNull] Encoding encoding)
			: base(algorithm, encoding)
		{
		}

		public int KeySize
		{
			get => Algorithm.KeySize;
			set => Algorithm.KeySize = value;
		}

		[NotNull]
		public KeySizes[] KeySizes => Algorithm.LegalKeySizes;

		public void FromXmlString([NotNull] string value) { Algorithm.FromXmlString(value); }
		[NotNull] public string ToXmlString(bool includePrivateParameters) { return Algorithm.ToXmlString(includePrivateParameters); }

		public void Clear() { Algorithm.Clear(); }
	}
}