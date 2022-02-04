using System.Diagnostics;
using essentialMix.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace essentialMix.Core.Authorization;

public static class Role
{
	public const string Administrators = "Administrators";
	public const string Members = "Members";

	public static readonly string[] Roles =
	{
		Administrators,
		Members
	};
}

[DebuggerDisplay("{Name}")]
[Serializable]
public class Role<TKey> : IdentityRole<TKey>, IEntity<TKey>
	where TKey : IComparable<TKey>, IEquatable<TKey>
{
	public virtual ICollection<UserRole<TKey>> UserRoles { get; set; }
}