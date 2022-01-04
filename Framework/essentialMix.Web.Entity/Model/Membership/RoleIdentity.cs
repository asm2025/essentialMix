using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace essentialMix.Web.Entity.Model.Membership;

public class RoleIdentity<TUser> : IRoleIdentity<TUser>
	where TUser : IdentityUser
{
	public RoleIdentity([NotNull] IIdentity<TUser> identity)
		: this(identity, Array.Empty<string>())
	{
	}

	public RoleIdentity([NotNull] IIdentity<TUser> identity, [NotNull] ICollection<string> roleNames)
	{
		Identity = identity;
		RoleNames = roleNames;
	}

	public IIdentity<TUser> Identity { get; }
	public ICollection<string> RoleNames { get; }
}

public class RoleIdentity : RoleIdentity<IdentityUser>, IRoleIdentity
{
	public RoleIdentity([NotNull] IIdentity<IdentityUser> identity) 
		: base(identity)
	{
	}

	public RoleIdentity([NotNull] IIdentity<IdentityUser> identity, [NotNull] ICollection<string> roleNames) 
		: base(identity, roleNames)
	{
	}
}