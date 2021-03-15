using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Patterns.Security.Membership
{
	public class RoleUser<TUser, TKey> : IRoleUser<TUser, TKey>
		where TUser : IUser<TKey>
	{
		public RoleUser([NotNull] TUser user)
			: this(user, Array.Empty<string>())
		{
		}

		public RoleUser([NotNull] TUser user, [NotNull] ICollection<string> roleNames)
		{
			User = user;
			RoleNames = roleNames;
		}

		public TUser User { get; }
		public ICollection<string> RoleNames { get; }
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
}