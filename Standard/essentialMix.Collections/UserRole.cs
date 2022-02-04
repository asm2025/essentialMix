using System;
using System.Diagnostics;
using essentialMix.Data.Model;

namespace essentialMix.Identity;

[DebuggerDisplay("User: {UserId} - Role: {RoleId}")]
[Serializable]
public class UserRole : IdentityUserRole<string>, IEntity
{
	public virtual User User { get; set; }
	public virtual Role Role { get; set; }
}