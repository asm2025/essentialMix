using JetBrains.Annotations;

namespace essentialMix.Cryptography;

/// <summary>
/// Interface for Vigenère cipher implementation using byte-wise operations (mod 256).
/// </summary>
public interface IVigenereCipher : IEncrypt
{
	/// <summary>
	/// Gets or sets the encryption key.
	/// </summary>
	[NotNull]
	byte[] Key { get; set; }
}

