﻿using System;
using System.Linq;
using System.Security.Claims;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class ClaimsPrincipalExtension
{
	public static bool IsInRole([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2)
	{
		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)) return true;
			}
		}

		return false;
	}

	public static bool IsInRole([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3)
	{
		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase)) return true;
			}
		}

		return false;
	}

	public static bool IsInRole([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4)
	{
		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role4, StringComparison.OrdinalIgnoreCase)) return true;
			}
		}

		return false;
	}

	public static bool IsInRole([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4, [NotNull] string role5)
	{
		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role4, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role5, StringComparison.OrdinalIgnoreCase)) return true;
			}
		}

		return false;
	}

	public static bool IsInRoleEx([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role)
	{
		bool isInRole = false;
		bool isInOthers = false;

		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (Claim claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType))
			{
				if (string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase))
				{
					isInRole = true;
				}
				else
				{
					isInOthers = true;
					if (isInRole) break;
				}
			}
		}

		return isInRole && !isInOthers;
	}

	public static bool IsInRoleEx([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2)
	{
		bool isInRole = false;
		bool isInOthers = false;

		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase))
				{
					isInRole = true;
				}
				else
				{
					isInOthers = true;
					if (isInRole) break;
				}
			}
		}

		return isInRole && !isInOthers;
	}

	public static bool IsInRoleEx([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3)
	{
		bool isInRole = false;
		bool isInOthers = false;

		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase))
				{
					isInRole = true;
				}
				else
				{
					isInOthers = true;
					if (isInRole) break;
				}
			}
		}

		return isInRole && !isInOthers;
	}

	public static bool IsInRoleEx([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4)
	{
		bool isInRole = false;
		bool isInOthers = false;

		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role4, StringComparison.OrdinalIgnoreCase))
				{
					isInRole = true;
				}
				else
				{
					isInOthers = true;
					if (isInRole) break;
				}
			}
		}

		return isInRole && !isInOthers;
	}

	public static bool IsInRoleEx([NotNull] this ClaimsPrincipal thisValue, [NotNull] string role1, [NotNull] string role2, [NotNull] string role3, [NotNull] string role4, [NotNull] string role5)
	{
		bool isInRole = false;
		bool isInOthers = false;

		foreach (ClaimsIdentity identity in thisValue.Identities)
		{
			foreach (string claim in identity.Claims.Where(e => e.Type == identity.RoleClaimType).Select(e => e.Value))
			{
				if (string.Equals(claim, role1, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role2, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role3, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role4, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(claim, role5, StringComparison.OrdinalIgnoreCase))
				{
					isInRole = true;
				}
				else
				{
					isInOthers = true;
					if (isInRole) break;
				}
			}
		}

		return isInRole && !isInOthers;
	}
}