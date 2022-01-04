using System.Security;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace essentialMix.Web.Entity.Model.Membership;

public interface IIdentity<out TUser>
	where TUser : IdentityUser
{
	[NotNull]
	TUser User { get; }

	SecureString Password { get; }
}

public interface IIdentity : IIdentity<IdentityUser>
{
}