using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class X509Certificate2Extension
	{
		[NotNull] public static AsymmetricAlgorithm GetPublicEncryptor([NotNull] this X509Certificate2 thisValue) { return GetPublicEncryptor<AsymmetricAlgorithm>(thisValue); }

		public static AsymmetricAlgorithm GetPrivateDecryptor([NotNull] this X509Certificate2 thisValue) { return GetPrivateDecryptor<AsymmetricAlgorithm>(thisValue); }

		[NotNull]
		public static T GetPublicEncryptor<T>([NotNull] this X509Certificate2 thisValue)
			where T : AsymmetricAlgorithm
		{
			return (T)thisValue.PublicKey.Key;
		}

		public static T GetPrivateDecryptor<T>([NotNull] this X509Certificate2 thisValue)
			where T : AsymmetricAlgorithm
		{
			return (T)thisValue.PrivateKey;
		}
	}
}