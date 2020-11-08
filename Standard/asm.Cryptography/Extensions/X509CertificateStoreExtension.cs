using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using asm.Cryptography;
using asm.Extensions;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Patterns.Security.Cryptography
{
	public static class X509CertificateStoreExtension
	{
		public static NetworkCredential GetCredentials([NotNull] this X509CertificateStore thisValue, [NotNull] string name, string token, char separator = '|') { return GetCredentials(thisValue, name, null, token, separator); }
		public static NetworkCredential GetCredentials([NotNull] this X509CertificateStore thisValue, [NotNull] string name, SecureString password, string token, char separator = '|')
		{
			if (string.IsNullOrWhiteSpace(token)) return null;
			X509Certificate2 certificate = thisValue.Get(name, password);
			return certificate == null
						? null
						: GetCredentials(certificate, token, separator);
		}

		public static NetworkCredential GetCredentials([NotNull] X509Certificate2 certificate, string token, char separator = '|')
		{
			if (string.IsNullOrWhiteSpace(token)) return null;
			string data = QuickCipher.AsymmetricDecrypt(certificate, token).UnSecure();
			if (string.IsNullOrEmpty(data)) return null;
			int n = data.IndexOf(separator);
			NetworkCredential credential = n > 0 && n < data.Length - 1
												? new NetworkCredential(data.Substring(0, n), data.Substring(++n).Secure())
												: null;
			// delete the string from memory
			// ReSharper disable once RedundantAssignment
			data = null;
			return credential;
		}
	}
}