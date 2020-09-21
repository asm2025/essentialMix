using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using JetBrains.Annotations;
using Microsoft.Owin.Security;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class IPrincipalExtension
	{
		public static void AddOrUpdateClaim([NotNull] this IPrincipal thisValue, [NotNull] string key, string value)
		{
			if (!(thisValue.Identity is ClaimsIdentity identity)) return;

			HttpContext ctx = HttpContext.Current ?? throw new InvalidOperationException("No HTTP context is currently running.");
			IAuthenticationManager authenticationManager = ctx.GetOwinContext()?.Authentication ?? throw new InvalidOperationException("Could not get an Owin context's authentication manager.");

			// check for existing claim and remove it
			Claim existingClaim = identity.FindFirst(key);
			if (existingClaim != null) identity.RemoveClaim(existingClaim);
			
			// add new claim
			identity.AddClaim(new Claim(key, value ?? string.Empty));
			authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });
		}

		public static string GetClaimValue([NotNull] this IPrincipal thisValue, [NotNull] string key)
		{
			ClaimsIdentity identity = thisValue.Identity as ClaimsIdentity;
			Claim claim = identity?.Claims.FirstOrDefault(c => c.Type == key);
			return claim?.Value;
		}
	}
}