using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class UserManagerExtension
{
	public static TUser FindByNameOrEmail<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
		where TUser : class, IUser<TKey>
		where TKey : IEquatable<TKey>
	{
		TUser user = thisValue.FindByNameAsync(userName).Execute();
		return user ?? FindByNameOrEmailAsync(thisValue, userName, email).Execute();
	}

	[NotNull]
	public static async Task<TUser> FindByNameOrEmailAsync<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
		where TUser : class, IUser<TKey>
		where TKey : IEquatable<TKey>
	{
		TUser user = await thisValue.FindByNameAsync(userName);
		return user ?? await thisValue.FindByEmailAsync(email);
	}
}