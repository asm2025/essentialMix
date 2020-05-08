using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using asm.Extensions;
using asm.Helpers;
using asm.Web.Entity.Model.Membership;
using asm.Web.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace asm.Web.Entity.Extensions
{
	public static class IdentityDbContextExtension
	{
		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, roleIdentities));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, false, roleIdentities, null, false, false);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, roleIdentities, onUserAdded));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, false, roleIdentities, onUserAdded, false, false);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities, bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, roleIdentities, continueOnRoleError, continueOnUserError));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities, bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, false, roleIdentities, null, continueOnRoleError, continueOnUserError);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, null, false, false);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, onUserAdded));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, onUserAdded, false, false);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities, bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, continueOnRoleError, continueOnUserError));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities, bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			return await CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, null, continueOnRoleError, continueOnUserError);
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail,
			[NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded,
			bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities, onUserAdded, continueOnRoleError, continueOnUserError));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] IDictionary<string, IRoleIdentity<TUser>[]> roleIdentities,  Func<UserManager<TUser>, TUser, bool> onUserAdded, bool continueOnRoleError, bool continueOnUserError)
			where TUser : IdentityUser
		{
			if (roleIdentities.Count == 0) return true;

			using (UserStore<TUser> userStore = new UserStore<TUser>(thisValue))
			{
				using UserManager<TUser> userManager = new UserManager<TUser>(userStore);
				Type type = typeof(TUser);
				Type retType = typeof(string);
				PropertyInfo firstNameProperty = type.GetProperty("FirstName", Constants.BF_PUBLIC_INSTANCE, retType) ?? type.GetProperty("GivenName", Constants.BF_PUBLIC_INSTANCE, retType);
				PropertyInfo lastNameProperty = type.GetProperty("LastName", Constants.BF_PUBLIC_INSTANCE, retType);

				foreach (KeyValuePair<string, IRoleIdentity<TUser>[]> pair in roleIdentities)
				{
					if (string.IsNullOrEmpty(pair.Key)) continue;

					IdentityRole role = await thisValue.Roles.FirstOrDefaultAsync(r => r.Name == pair.Key);

					if (role == null)
					{
						try
						{
							role = thisValue.Roles.Add(new IdentityRole(pair.Key));
							await thisValue.SaveChangesAsync();
						}
						catch (NotSupportedException)
						{
							return false;
						}
						catch (ObjectDisposedException)
						{
							return false;
						}
						catch
						{
							if (continueOnRoleError) continue;
							return false;
						}
					}

					for (int i = 0; i < pair.Value.Length; i++)
					{
						IRoleIdentity<TUser> roleIdentity = pair.Value[i];

						if (tryNameAndEmail)
						{
							if (string.IsNullOrEmpty(roleIdentity.Identity.User.UserName) && string.IsNullOrEmpty(roleIdentity.Identity.User.Email))
							{
								if (!continueOnUserError) throw new ArgumentNullException(nameof(roleIdentities), $"IRoleUser UserName and Email are null or empty at position {i} of role '{pair.Key}'.");
								continue;
							}
						}
						else
						{
							if (string.IsNullOrEmpty(roleIdentity.Identity.User.UserName))
							{
								if (!continueOnUserError) throw new ArgumentNullException(nameof(roleIdentities), $"IRoleUser UserName == null or empty at position {i} of role '{pair.Key}'.");
								continue;
							}
						}

						TUser user = tryNameAndEmail
										? await userManager.FindByNameOrEmailAsync(roleIdentity.Identity.User.UserName, roleIdentity.Identity.User.Email)
										: await userManager.FindByNameAsync(roleIdentity.Identity.User.UserName);

						if (user == null)
						{
							user = roleIdentity.Identity.User;

							if (await userManager.CreateAsync(user, roleIdentity.Identity.Password.UnSecure()) != IdentityResult.Success)
							{
								if (continueOnUserError) continue;
								return false;
							}
						}

						if (firstNameProperty != null)
						{
							string firstName = Convert.ToString(firstNameProperty.GetValue(user));
							if (user.Claims.All(c => c.ClaimType != ClaimTypes.GivenName)) await userManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.GivenName, firstName));

							string lastName = lastNameProperty == null ? null : Convert.ToString(lastNameProperty.GetValue(user));
							if (user.Claims.All(c => c.ClaimType != ClaimTypes.Name)) await userManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.Name, lastName.IfNullOrEmpty(firstName, $"{firstName} {lastName}")));
						}

						if (user.Claims.All(c => c.ClaimType != ClaimTypes.Email)) await userManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.Email, user.Email));
						if (user.Claims.All(c => c.ClaimType != ClaimTypes.Role)) await userManager.AddClaimAsync(user.Id, new Claim(ClaimTypes.Role, role.Name));
						if (!await userManager.IsInRoleAsync(user.Id, role.Name)) await userManager.AddToRoleAsync(user.Id, role.Name);

						if (onUserAdded != null && !onUserAdded(userManager, user))
						{
							if (continueOnUserError) continue;
							return false;
						}
					}
				}
			}

			return true;
		}

		public static bool CreateRolesAndUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] params IRoleIdentity<TUser>[] roleIdentities)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAndUsersAsync(thisValue, tryNameAndEmail, roleIdentities));
		}

		public static async Task<bool> CreateRolesAndUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] params IRoleIdentity<TUser>[] roleIdentities)
			where TUser : IdentityUser
		{
			if (roleIdentities.Length == 0) return true;

			string[] allRoleNames = roleIdentities.Where(u => u?.RoleNames.Count > 0).SelectMany(u => u.RoleNames).Where(r => !string.IsNullOrEmpty(r)).Distinct().ToArray();
			if (allRoleNames.Length > 0 && !CreateRoles(thisValue, allRoleNames)) return false;
			return await CreateUsersAsync(thisValue, tryNameAndEmail, roleIdentities);
		}

		public static bool CreateRoles<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] params string[] roleNames)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateRolesAsync(thisValue, roleNames));
		}

		public static async Task<bool> CreateRolesAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, [NotNull] params string[] roleNames)
			where TUser : IdentityUser
		{
			if (roleNames.Length == 0) return false;

			using (RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(thisValue)))
			{
				foreach (string roleName in roleNames.Where(r => !string.IsNullOrEmpty(r)))
				{
					IdentityRole role = await roleManager.FindByNameAsync(roleName);
					if (role != null) continue;
					role = new IdentityRole(roleName);
					IdentityResult result = await roleManager.CreateAsync(role);
					if (result.Succeeded) continue;
					throw new AggregateException(result.Errors.Select(e => new IdentityNotMappedException(e)));
				}
			}

			return true;
		}

		public static bool CreateUsers<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] params IRoleIdentity<TUser>[] roleIdentities)
			where TUser : IdentityUser
		{
			return TaskHelper.Run(() => CreateUsersAsync(thisValue, tryNameAndEmail, roleIdentities));
		}

		public static async Task<bool> CreateUsersAsync<TUser>([NotNull] this IdentityDbContext<TUser> thisValue, bool tryNameAndEmail, [NotNull] params IRoleIdentity<TUser>[] roleIdentities)
			where TUser : IdentityUser
		{
			if (roleIdentities.Length == 0) return true;

			using (UserManager<TUser> userManager = new UserManager<TUser>(new UserStore<TUser>(thisValue)))
			{
				for (int i = 0; i < roleIdentities.Length; i++)
				{
					IRoleIdentity<TUser> roleUser = roleIdentities[i];
					if (roleUser == null) continue;

					if (tryNameAndEmail)
					{
						if (string.IsNullOrEmpty(roleUser.Identity.User.UserName) && string.IsNullOrEmpty(roleUser.Identity.User.Email))
							throw new ArgumentNullException(nameof(roleIdentities), $"IRoleUser UserName and Email are null or empty at position {i}.");
					}
					else
					{
						if (string.IsNullOrEmpty(roleUser.Identity.User.UserName))
							throw new ArgumentNullException(nameof(roleIdentities), $"IRoleUser UserName and Email are null or empty at position {i}.");
					}

					TUser user = tryNameAndEmail
						? await userManager.FindByNameOrEmailAsync(roleUser.Identity.User.UserName, roleUser.Identity.User.Email)
						: await userManager.FindByNameAsync(roleUser.Identity.User.UserName);

					if (user == null)
					{
						IdentityResult result = await userManager.CreateAsync(roleUser.Identity.User, roleUser.Identity.Password.UnSecure());
						if (!result.Succeeded) throw new AggregateException(result.Errors.Select(e => new IdentityNotMappedException(e)));
						user = tryNameAndEmail
							? await userManager.FindByNameOrEmailAsync(roleUser.Identity.User.UserName, roleUser.Identity.User.Email)
							: await userManager.FindByNameAsync(roleUser.Identity.User.UserName);
					}

					if (user == null) return false;

					foreach (string roleName in roleUser.RoleNames.Where(r => !string.IsNullOrEmpty(r)))
					{
						if (await userManager.IsInRoleAsync(user.Id, roleName)) continue;

						IdentityResult result = await userManager.AddToRoleAsync(user.Id, roleName);
						if (!result.Succeeded) throw new AggregateException(result.Errors.Select(e => new IdentityNotMappedException(e)));
					}
				}
			}

			return true;
		}
	}
}