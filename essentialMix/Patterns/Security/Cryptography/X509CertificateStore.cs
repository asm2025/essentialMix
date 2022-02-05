using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Security.Cryptography;

public class X509CertificateStore : Disposable
{
	private static readonly Lazy<X509CertificateStore> __instance = new Lazy<X509CertificateStore>(() => new X509CertificateStore(), LazyThreadSafetyMode.PublicationOnly);

	private readonly ConcurrentDictionary<string, X509Certificate2> _certificates = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

	// beforeinitfield
	static X509CertificateStore()
	{
	}

	/// <inheritdoc />
	private X509CertificateStore()
	{
	}

	[NotNull]
	public static X509CertificateStore Instance => __instance.Value;

	public void Add([NotNull] string name, SecureString password = null)
	{
		Get(name, password);
	}

	public void Add(SecureString password, [NotNull] params string[] names)
	{
		foreach (string name in names) 
			Get(name, password);
	}

	public void Add([NotNull] params (string Name, SecureString Password)[] entries)
	{
		foreach ((string name, SecureString password) in entries) 
			Get(name, password);
	}

	public X509Certificate2 Get(string name, SecureString password = null)
	{
		name = Normalize(name);
		return string.IsNullOrEmpty(name)
					? null
					: _certificates.GetOrAdd(name, fileName =>
					{
						string path = PathHelper.GetFullPath(fileName);
						return new X509Certificate2(path, password);
					});
	}

	public void Remove(string name)
	{
		name = Normalize(name);
		if (string.IsNullOrEmpty(name)) return;
		if (!_certificates.TryRemove(name, out X509Certificate2 certificate)) return;
		// It's important to dispose the certificate to release and delete any files that it has created.
		ObjectHelper.Dispose(ref certificate);
	}

	public void Remove([NotNull] params string[] names)
	{
		foreach (string n in names)
		{
			string name = Normalize(n);
			if (string.IsNullOrEmpty(name)) continue;
			if (!_certificates.TryRemove(name, out X509Certificate2 certificate)) continue;
			// It's important to dispose the certificate to release and delete any files that it has created.
			ObjectHelper.Dispose(ref certificate);
		}
	}

	public void Clear()
	{
		if (_certificates.Count == 0) return;

		foreach (string name in _certificates.Keys)
		{
			if (!_certificates.TryRemove(name, out X509Certificate2 certificate)) continue;
			// It's important to dispose the certificate to release and delete any files that it has created.
			ObjectHelper.Dispose(ref certificate);
		}
	}

	public AsymmetricAlgorithm GetPublicEncryptor(string name, SecureString password = null)
	{
		X509Certificate2 certificate = Get(name, password);
		return certificate?.GetPublicEncryptor();
	}

	public AsymmetricAlgorithm GetPrivateDecryptor(string name, SecureString password = null)
	{
		X509Certificate2 certificate = Get(name, password);
		return certificate?.GetPrivateDecryptor();
	}

	public T GetPublicEncryptor<T>(string name, SecureString password = null)
		where T : AsymmetricAlgorithm
	{
		X509Certificate2 certificate = Get(name, password);
		return certificate?.GetPublicEncryptor<T>();
	}

	public T GetPrivateDecryptor<T>(string name, SecureString password = null)
		where T : AsymmetricAlgorithm
	{
		X509Certificate2 certificate = Get(name, password);
		return certificate?.GetPrivateDecryptor<T>();
	}

	[NotNull]
	public IEnumerable<X509Certificate2> GetEncryptionCertificates(StoreLocation location = StoreLocation.LocalMachine)
	{
		using (X509Store certificateStore = new X509Store(location))
		{
			certificateStore.Open(OpenFlags.ReadOnly);
			return certificateStore.Certificates.Cast<X509Certificate2>();
		}
	}

	public X509Certificate2 GetEncryptionCertificate([NotNull] Predicate<X509Certificate2> predicate, StoreLocation location = StoreLocation.LocalMachine)
	{
		return GetEncryptionCertificates(location).FirstOrDefault(c => predicate(c));
	}

	private static string Normalize(string name)
	{
		return PathHelper.Trim(name)?.Suffix(".pfx", true);
	}
}