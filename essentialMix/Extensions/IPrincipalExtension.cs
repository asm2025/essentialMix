using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IPrincipalExtension
{
	public static bool IsInRole([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRole(role1, role2);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRole(role1, role2)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRole([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRole(role1, role2, role3);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRole(role1, role2, role3)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRole([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRole(role1, role2, role3, role4);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRole(role1, role2, role3, role4)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRole([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4, [NotNull] string role5)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRole(role1, role2, role3, role4, role5);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRole(role1, role2, role3, role4, role5)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRole([NotNull] this IPrincipal thisValue, [NotNull] IEnumerable<string> roles)
	{
		ICollection<string> collection = roles as ICollection<string> ?? new HashSet<string>(roles);
		if (collection.Count == 0) return false;

		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRole(collection);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRole(collection)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] string role)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(role);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(role)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(role1, role2);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(role1, role2)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(role1, role2, role3);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(role1, role2, role3)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(role1, role2, role3, role4);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(role1, role2, role3, role4)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4, [NotNull] string role5)
	{
		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(role1, role2, role3, role4, role5);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(role1, role2, role3, role4, role5)) continue;
			return true;
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this IPrincipal thisValue, [NotNull] IEnumerable<string> roles)
	{
		ICollection<string> collection = roles as ICollection<string> ?? new HashSet<string>(roles);
		if (collection.Count == 0) return false;

		if (thisValue is not ClaimsPrincipal claimsPrincipal)
			return thisValue.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsInRoleEx(collection);

		foreach (ClaimsIdentity identity in claimsPrincipal.Identities)
		{
			if (!identity.IsInRoleEx(collection)) continue;
			return true;
		}

		return false;
	}
}