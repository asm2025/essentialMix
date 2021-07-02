using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using essentialMix.Helpers;
using essentialMix.Web.Mvc.Models;

namespace essentialMix.Web.Mvc.Controllers
{
	public abstract class Controller<TKey, TUser, TUserManager> : System.Web.Mvc.Controller
		where TKey : IEquatable<TKey>
		where TUser : class, IUser<TKey>
		where TUserManager : UserManager<TUser, TKey>
	{
		private TUserManager _userManager;
		private Regex _logOffExpression;
		private string _defaultUrl;

		protected Controller()
			: this(null)
		{
		}

		protected Controller(TUserManager userManager)
		{
			UserManager = userManager;
		}

		[NotNull]
		public CultureInfo UICulture => Thread.CurrentThread.CurrentUICulture;

		[NotNull]
		public CultureInfo Culture => Thread.CurrentThread.CurrentCulture;

		public bool IsRightToLeft => UICulture.IsRightToLeft();

		protected Regex LogOffUrlExpression => _logOffExpression ??= UriHelper.CreateBadRedirectExpression(LogOffActionData.CreateUrl(Url));

		protected string DefaultUrl => _defaultUrl ??= DefaultUrlData.CreateUrl(Url);

		[NotNull]
		protected abstract ControllerActionData LogOffActionData { get; }

		[NotNull]
		protected abstract ControllerActionData DefaultUrlData { get; }

		protected TUserManager UserManager
		{
			get => _userManager ??= Request?.GetOwinContext().GetUserManager<TUserManager>();
			private set => _userManager = value;
		}

		protected override void Initialize(RequestContext requestContext)
		{
			if (System.Web.HttpContext.Current != null)
			{
				HttpCookie cultureCookie = System.Web.HttpContext.Current.Request.Cookies[CookieNames.Culture];
				CultureInfo cultureInfo = (cultureCookie != null && CultureInfoHelper.IsCultureName(cultureCookie.Value)
					? CultureInfo.GetCultureInfo(cultureCookie.Value)
					: Thread.CurrentThread.CurrentUICulture)
					.Neutral();
				Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = cultureInfo;
			}

			base.Initialize(requestContext);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _userManager);
			base.Dispose(disposing);
		}

		[AllowAnonymous]
		[NotNull]
		public virtual ActionResult ChangeCulture(string name, string returnUrl)
		{
			name = name?.Trim();
			if (!CultureInfoHelper.IsCultureName(name)) return RedirectToLocal(returnUrl);
			Response.Cookies.Remove(CookieNames.Culture);

			HttpRequest request = System.Web.HttpContext.Current.Request;
			HttpCookie cookie = request.Cookies[CookieNames.Culture] ?? new HttpCookie(CookieNames.Culture);
			cookie.Value = name;
			cookie.Secure = request.IsSecureConnection;
			cookie.Expires = DateTime.UtcNow.AddDays(7);
			Response.SetCookie(cookie);
			return RedirectToLocal(returnUrl);
		}

		[NotNull]
		protected virtual ActionResult RedirectToLocal(string returnUrl)
		{
			return this.RedirectToLocal(returnUrl, LogOffUrlExpression, DefaultUrl);
		}
	}

	public abstract class Controller<TUser, TUserManager> : Controller<string, TUser, TUserManager>
		where TUser : class, IUser<string>
		where TUserManager : UserManager<TUser>
	{
		/// <inheritdoc />
		protected Controller()
		{
		}

		/// <inheritdoc />
		protected Controller(TUserManager userManager) 
			: base(userManager)
		{
		}
	}

	public abstract class Controller<TUserManager> : Controller<IUser<string>, TUserManager>
		where TUserManager : UserManager<IUser<string>>
	{
		/// <inheritdoc />
		protected Controller()
		{
		}

		/// <inheritdoc />
		protected Controller(TUserManager userManager) 
			: base(userManager)
		{
		}
	}

	public abstract class Controller : Controller<UserManager<IUser<string>>>
	{
		/// <inheritdoc />
		protected Controller()
		{
		}

		/// <inheritdoc />
		protected Controller(UserManager<IUser<string>> userManager) 
			: base(userManager)
		{
		}
	}
}