using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class X509CertificateHelper
	{
		private static readonly ConcurrentDictionary<string, X509Certificate2> __certificates = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);

		public static void Add([NotNull] string name, SecureString password = null)
		{
			Get(name, password);
		}

		public static void Add(SecureString password, [NotNull] params string[] names)
		{
			foreach (string name in names) 
				Get(name, password);
		}

		public static void Add([NotNull] params (string Name, SecureString Password)[] entries)
		{
			foreach ((string name, SecureString password) in entries) 
				Get(name, password);
		}

		public static X509Certificate2 Get(string name, SecureString password = null)
		{
			name = Normalize(name);
			if (string.IsNullOrEmpty(name)) return null;
			return __certificates.GetOrAdd(name, fileName =>
			{
				string path = Path.Combine(AppInfo.Directory, fileName);
				return new X509Certificate2(path, password);
			});
		}

		public static void Remove(string name)
		{
			name = Normalize(name);
			if (string.IsNullOrEmpty(name)) return;
			__certificates.TryRemove(name, out _);
		}

		public static void Remove([NotNull] params string[] names)
		{
			foreach (string n in names)
			{
				string name = Normalize(n);
				if (string.IsNullOrEmpty(name)) continue;
				__certificates.TryRemove(name, out _);
			}
		}

		public static void Clear()
		{
			__certificates.Clear();
		}

		public static AsymmetricAlgorithm GetPublicEncryptor(string name, SecureString password = null)
		{
			X509Certificate2 certificate = Get(name, password);
			return certificate?.GetPublicEncryptor();
		}

		public static AsymmetricAlgorithm GetPrivateDecryptor(string name, SecureString password = null)
		{
			X509Certificate2 certificate = Get(name, password);
			return certificate?.GetPrivateDecryptor();
		}

		public static T GetPublicEncryptor<T>(string name, SecureString password = null)
			where T : AsymmetricAlgorithm
		{
			X509Certificate2 certificate = Get(name, password);
			return certificate?.GetPublicEncryptor<T>();
		}

		public static T GetPrivateDecryptor<T>(string name, SecureString password = null)
			where T : AsymmetricAlgorithm
		{
			X509Certificate2 certificate = Get(name, password);
			return certificate?.GetPrivateDecryptor<T>();
		}

		[NotNull]
		public static IEnumerable<X509Certificate2> GetEncryptionCertificates(StoreLocation location = StoreLocation.LocalMachine)
		{
			using (X509Store certificateStore = new X509Store(location))
			{
				certificateStore.Open(OpenFlags.ReadOnly);
				return certificateStore.Certificates.Cast<X509Certificate2>();
			}
		}

		public static X509Certificate2 GetEncryptionCertificate([NotNull] Predicate<X509Certificate2> predicate, StoreLocation location = StoreLocation.LocalMachine)
		{
			return GetEncryptionCertificates(location).FirstOrDefault(c => predicate(c));
		}

		private static string Normalize(string name)
		{
			return PathHelper.Trim(name)?.Suffix(".pfx", true);
		}
	}
}