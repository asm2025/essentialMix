using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Security.Membership;

public class RoleUser<TUser, TKey>([NotNull] TUser user, [NotNull] ICollection<string> roleNames)
	: IRoleUser<TUser, TKey>
	where TUser : IUser<TKey>
{
	public RoleUser([NotNull] TUser user)
		: this(user, Array.Empty<string>())
	{
	}

	public TUser User { get; } = user;
	public ICollection<string> RoleNames { get; } = roleNames;
}

public class RoleUser<TUser> : RoleUser<TUser, string>, IRoleUser<TUser>
	where TUser : IUser<string>
{
	public RoleUser([NotNull] TUser user) 
		: base(user)
	{
	}

	public RoleUser([NotNull] TUser user, [NotNull] ICollection<string> roleNames) 
		: base(user, roleNames)
	{
	}
}