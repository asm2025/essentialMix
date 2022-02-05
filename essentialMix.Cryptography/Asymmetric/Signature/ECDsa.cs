using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Asymmetric.Signature;

public class ECDsa : ECDsaAlgorithmBase<System.Security.Cryptography.ECDsa>
{
	public ECDsa()
		: base(System.Security.Cryptography.ECDsa.Create() ?? throw new TypeInitializationException(typeof(ECDsa).FullName, new Exception($"Could not create an instance of {typeof(System.Security.Cryptography.ECDsa).FullName}.")))
	{
	}

	public ECDsa([NotNull] System.Security.Cryptography.ECDsa algorithm) 
		: base(algorithm)
	{
	}

	public ECDsa([NotNull] Encoding encoding) 
		: base(System.Security.Cryptography.ECDsa.Create() ?? throw new TypeInitializationException(typeof(ECDsa).FullName, new Exception($"Could not create an instance of {typeof(System.Security.Cryptography.ECDsa).FullName}.")), encoding)
	{
	}

	public ECDsa([NotNull] System.Security.Cryptography.ECDsa algorithm, [NotNull] Encoding encoding) 
		: base(algorithm, encoding)
	{
	}

	public override object Clone() { return new ECDsa(Algorithm, Encoding); }
}