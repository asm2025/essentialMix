using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using essentialMix.Extensions;
using essentialMix.Patterns.Security.Cryptography;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Services;

public class NetworkCredentialService([NotNull] IQuickCipherService cipherService) : INetworkCredentialService
{
	private readonly IQuickCipherService _cipherService = cipherService;

	/// <inheritdoc />
	public NetworkCredential GetCredentials(string name, string token, char separator = '|') { return GetCredentials(name, null, token, separator); }

	/// <inheritdoc />
	public NetworkCredential GetCredentials(string name, SecureString password, string token, char separator = '|')
	{
		if (string.IsNullOrWhiteSpace(token)) return null;
		X509Certificate2 certificate = X509CertificateStore.Instance.Get(name, password);
		return certificate == null
					? null
					: GetCredentials(certificate, token, separator);
	}

	/// <inheritdoc />
	public NetworkCredential GetCredentials(X509Certificate2 certificate, string token, char separator = '|')
	{
		if (string.IsNullOrWhiteSpace(token)) return null;
		string data = _cipherService.AsymmetricDecrypt(certificate, token).UnSecure();
		if (string.IsNullOrEmpty(data)) return null;
		int n = data.IndexOf(separator);
		NetworkCredential credential = n > 0 && n < data.Length - 1
											? new NetworkCredential(data.Left(n), data.Right(data.Length - n - 1).Secure())
											: null;
		// delete the string from memory
		// ReSharper disable once RedundantAssignment
#pragma warning disable IDE0059 // Unnecessary assignment of a value
		data = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
		return credential;
	}
}