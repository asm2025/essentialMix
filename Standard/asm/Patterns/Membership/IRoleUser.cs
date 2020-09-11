using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Patterns.Membership
{
	public interface IRoleUser<out TUser, out TKey>
		where TUser : IUser<TKey>
	{
		[NotNull]
		TUser User { get; }

		[NotNull]
		ICollection<string> RoleNames { get; }
	}

	public interface IRoleUser<out TUser> : IRoleUser<TUser, string>
		where TUser : IUser<string>
	{
	}
}