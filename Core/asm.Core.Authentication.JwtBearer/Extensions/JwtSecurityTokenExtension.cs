using System.IdentityModel.Tokens.Jwt;
using asm.Core.Authentication.JwtBearer.Helpers;
using JetBrains.Annotations;

namespace asm.Core.Authentication.JwtBearer.Extensions
{
	public static class JwtSecurityTokenExtension
	{
		public static string Value([NotNull] this JwtSecurityToken thisValue)
		{
			return SecurityTokenHelper.Value(thisValue);
		}
	}
}