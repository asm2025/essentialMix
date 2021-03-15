using System.Security;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace essentialMix.Web.Entity.Model.Membership
{
	public class Identity<TUser> : IIdentity<TUser>
		where TUser : IdentityUser
	{
		public Identity([NotNull] TUser user)
			: this(user, null)
		{
		}

		public Identity([NotNull] TUser user, SecureString password)
		{
			User = user;
			Password = password;
		}

		public TUser User { get; }
		public SecureString Password { get; set; }
	}

	public class Identity : Identity<IdentityUser>, IIdentity
	{
		public Identity([NotNull] IdentityUser user) 
			: base(user)
		{
		}

		public Identity([NotNull] IdentityUser user, SecureString password) 
			: base(user, password)
		{
		}
	}
}