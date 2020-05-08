using JetBrains.Annotations;

namespace asm.Cryptography.RandomNumber
{
	public class RNGCryptoServiceProvider : RandomNumberGeneratorBase<System.Security.Cryptography.RNGCryptoServiceProvider>
	{
		public RNGCryptoServiceProvider()
			: base(new System.Security.Cryptography.RNGCryptoServiceProvider())
		{
		}

		public RNGCryptoServiceProvider([NotNull] System.Security.Cryptography.RNGCryptoServiceProvider algorithm) 
			: base(algorithm)
		{
		}

		public override object Clone() { return new RNGCryptoServiceProvider(Algorithm); }
	}
}