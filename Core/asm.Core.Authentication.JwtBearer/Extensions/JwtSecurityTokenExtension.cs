using System.IdentityModel.Tokens.Jwt;
using asm.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class JwtSecurityTokenExtension
	{
		public static string Value([NotNull] this JwtSecurityToken thisValue)
		{
			return SecurityTokenHelper.Value(thisValue);
		}
	}
}