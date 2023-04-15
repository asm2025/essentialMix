using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace essentialMix.Cryptography.Services;

public interface INetworkCredentialService
{
	NetworkCredential GetCredentials([NotNull] string name, string token, char separator = '|');
	NetworkCredential GetCredentials([NotNull] string name, SecureString password, string token, char separator = '|');
	NetworkCredential GetCredentials([NotNull] X509Certificate2 certificate, string token, char separator = '|');
}