using System.IdentityModel.Tokens.Jwt;
using essentialMix.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class JwtSecurityTokenExtension
	{
		public static string Value([NotNull] this JwtSecurityToken thisValue)
		{
			return SecurityTokenHelper.Value(thisValue);
		}
	}
}