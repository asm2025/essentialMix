using JetBrains.Annotations;

namespace essentialMix.Cryptography.Services;

public interface ICryptoService
{
	[NotNull]
	public string Encrypt([NotNull] string value);
	[NotNull]
	public string Decrypt([NotNull] string value);
}