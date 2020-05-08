using System;
using System.Net;
using System.Security;
using System.Xml;
using JetBrains.Annotations;

namespace asm.Data.Helpers
{
	public static class XmlUrlResolverHelper
	{
		[NotNull]
		public static XmlUrlResolver CreateResolver()
		{
			return new XmlUrlResolver
					{
						Credentials = CredentialCache.DefaultCredentials
					};
		}

		[NotNull] public static XmlUrlResolver CreateResolver(string userName, [NotNull] SecureString password) { return CreateResolver(userName, password, null); }

		[NotNull]
		public static XmlUrlResolver CreateResolver(string userName, [NotNull] SecureString password, string domain)
		{
			userName = userName?.Trim() ?? throw new ArgumentNullException(nameof(userName));
			if (userName.Length == 0) throw new ArgumentException("User name cannot be empty", nameof(userName));
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (password.Length == 0) throw new ArgumentException("Password cannot be empty", nameof(password));

			if (domain != null)
			{
				domain = domain.Trim();
				if (domain.Length == 0) domain = null;
			}

			NetworkCredential credential = domain == null ? new NetworkCredential(userName, password) : new NetworkCredential(userName, password, domain);
			return new XmlUrlResolver { Credentials = credential };
		}

		[NotNull] public static XmlUrlResolver CreateResolver(string userName, string password) { return CreateResolver(userName, password, null); }

		[NotNull]
		public static XmlUrlResolver CreateResolver(string userName, string password, string domain)
		{
			userName = userName?.Trim() ?? throw new ArgumentNullException(nameof(userName));
			if (userName.Length == 0) throw new ArgumentException("User name cannot be empty", nameof(userName));

			password = password?.Trim() ?? throw new ArgumentNullException(nameof(password));
			if (password.Length == 0) throw new ArgumentException("Password cannot be empty", nameof(password));

			if (domain != null)
			{
				domain = domain.Trim();
				if (domain.Length == 0) domain = null;
			}

			NetworkCredential credential = domain == null ? new NetworkCredential(userName, password) : new NetworkCredential(userName, password, domain);
			return new XmlUrlResolver { Credentials = credential };
		}
	}
}