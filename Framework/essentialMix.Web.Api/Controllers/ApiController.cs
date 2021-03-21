using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using essentialMix.Caching;
using essentialMix.Helpers;
using essentialMix.Web.Api.Model;

namespace essentialMix.Web.Api.Controllers
{
	/// <inheritdoc />
	public abstract class ApiController<TKey, TUser, TUserManager> : System.Web.Http.ApiController
		where TKey : IEquatable<TKey>
		where TUser : class, IUser<TKey>
		where TUserManager : UserManager<TUser, TKey>
	{
		private TUserManager _userManager;
		private Regex _logOffExpression;
		private string _defaultUrl;

		protected ApiController()
			: this(null)
		{
		}

		protected ApiController(TUserManager userManager)
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
		protected abstract ActionData LogOffActionData { get; }

		[NotNull]
		protected abstract ActionData DefaultUrlData { get; }

		protected TUserManager UserManager
		{
			get => _userManager ?? Request?.GetOwinContext().GetUserManager<TUserManager>();
			private set => _userManager = value;
		}

		protected ICacheProvider Cache { get; } = new MemoryCacheProvider();

		protected override void Initialize(HttpControllerContext controllerContext)
		{
			if (HttpContext.Current != null)
			{
				HttpCookie languageCookie = HttpContext.Current.Request.Cookies[CookieNames.Culture];
				CultureInfo cultureInfo = (languageCookie != null && CultureInfoHelper.IsCultureName(languageCookie.Value)
					? CultureInfo.GetCultureInfo(languageCookie.Value)
					: Thread.CurrentThread.CurrentUICulture)
					.Neutral();
				Thread.CurrentThread.CurrentCulture = cultureInfo;
				Thread.CurrentThread.CurrentUICulture = cultureInfo;
			}

			base.Initialize(controllerContext);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) ObjectHelper.Dispose(ref _userManager);
			base.Dispose(disposing);
		}

		[NotNull]
		protected virtual IHttpActionResult LocalRedirect(string returnUrl) { return this.RedirectToLocal(returnUrl, LogOffUrlExpression, DefaultUrl); }
	}
	
	/// <inheritdoc />
	public abstract class ApiController<TUser, TUserManager> : ApiController<string, TUser, TUserManager>
		where TUser : class, IUser<string>
		where TUserManager : UserManager<TUser>
	{

		protected ApiController()
			: this(null)
		{
		}

		protected ApiController(TUserManager userManager)
			: base(userManager)
		{
		}
	}
	
	/// <inheritdoc />
	public abstract class ApiController<TUserManager> : ApiController<IUser<string>, TUserManager>
		where TUserManager : UserManager<IUser<string>>
	{

		protected ApiController()
			: this(null)
		{
		}

		protected ApiController(TUserManager userManager)
			: base(userManager)
		{
		}
	}
	
	/// <inheritdoc />
	public abstract class ApiController : ApiController<UserManager<IUser<string>>>
	{

		protected ApiController()
			: this(null)
		{
		}

		protected ApiController(UserManager<IUser<string>> userManager)
			: base(userManager)
		{
		}
	}
}