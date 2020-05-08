using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.IdentityModel.Tokens;

namespace asm.Core.Authentication.JwtBearer.Helpers
{
	public static class SecurityTokenHelper
	{
		private static readonly Lazy<JwtSecurityTokenHandler> __handler = new Lazy<JwtSecurityTokenHandler>(() => new JwtSecurityTokenHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

		public static JwtSecurityToken CreateToken([NotNull] SecurityTokenDescriptor value)
		{
			return __handler.Value.CreateJwtSecurityToken(value);
		}

		public static string Value([NotNull] SecurityToken value)
		{
			return __handler.Value.WriteToken(value);
		}

		public static void WriteToken([NotNull] XmlWriter writer, [NotNull] SecurityToken value)
		{
			__handler.Value.WriteToken(writer, value);
		}
	}
}