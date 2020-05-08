using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class X509CertificateHelper
	{
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
	}
}