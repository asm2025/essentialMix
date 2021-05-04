using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using essentialMix.Threading.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class UserManagerExtension
	{
		public static TUser FindByNameOrEmail<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
			where TUser : class, IUser<TKey>
			where TKey : IEquatable<TKey>
		{
			return TaskHelper.Run(() => FindByNameOrEmailAsync(thisValue, userName, email));
		}

		[NotNull]
		public static Task<TUser> FindByNameOrEmailAsync<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
			where TUser : class, IUser<TKey>
			where TKey : IEquatable<TKey>
		{
			Func<TUser, Task<TUser>>[] functions = {
				_ => thisValue.FindByNameAsync(userName),
				_ => thisValue.FindByEmailAsync(email)
			};
			return TaskHelper.SequenceAsync(default(TUser), user => user != null, functions);
		}
	}
}