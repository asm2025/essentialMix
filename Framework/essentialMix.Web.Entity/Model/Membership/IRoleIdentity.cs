using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace essentialMix.Web.Entity.Model.Membership;

public interface IRoleIdentity<out TUser>
	where TUser : IdentityUser
{
	[NotNull]
	IIdentity<TUser> Identity { get; }

	[NotNull]
	ICollection<string> RoleNames { get; }
}

public interface IRoleIdentity : IRoleIdentity<IdentityUser>
{
}