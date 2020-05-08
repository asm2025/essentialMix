using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.IdentityModel.Tokens;

namespace asm.Core.Authentication.JwtBearer.Helpers
{
	public static class SecurityKeyHelper
	{
		[NotNull]
		public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

		[NotNull]
		public static SymmetricSecurityKey CreateSymmetricKey([NotNull] string key, int size, Encoding encoding = null)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
			if (size < 8) size = 8;
			int m = size % 8;
			if (m > 0) size -= m;
			if (encoding == null) encoding = DefaultEncoding;
			byte[] bytes = encoding.GetBytes(key);
			byte[] buffer = new byte[size / 8];
			Array.Copy(bytes, buffer, Math.Min(bytes.Length, buffer.Length));
			return new SymmetricSecurityKey(buffer);
		}
	}
}
