using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using asm.Extensions;
using asm.Patterns.Cryptography;
using JetBrains.Annotations;

namespace asm.Cryptography.Helpers
{
	public static class NetworkCredentialHelper
	{
		public static NetworkCredential GetCredentials([NotNull] string name, string token, char separator = '|') { return GetCredentials(name, null, token, separator); }
		public static NetworkCredential GetCredentials([NotNull] string name, SecureString password, string token, char separator = '|')
		{
			if (string.IsNullOrWhiteSpace(token)) return null;
			X509Certificate2 certificate = X509CertificateStore.Instance.Get(name, password);
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
