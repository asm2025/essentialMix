using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using asm.Helpers;

namespace asm.Web.Extensions
{
	public static class UserManagerExtension
	{
		public static TUser FindByNameOrEmail<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
			where TUser : class, IUser<TKey>
			where TKey : IEquatable<TKey>
		{
			return TaskHelper.Run(() => FindByNameOrEmailAsync(thisValue, userName, email));
		}

		public static Task<TUser> FindByNameOrEmailAsync<TUser, TKey>([NotNull] this UserManager<TUser, TKey> thisValue, string userName, string email)
			where TUser : class, IUser<TKey>
			where TKey : IEquatable<TKey>
		{
			Func<TUser, Task<TUser>>[] functions = {
				user => thisValue.FindByNameAsync(userName),
				user => thisValue.FindByEmailAsync(email)
			};
			return TaskHelper.Sequence(functions, true, default(TUser), user => user != null);
		}
	}
}