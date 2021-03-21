using System;
using Microsoft.AspNet.Identity;
using essentialMix.Web.Mvc.Controllers;

namespace essentialMix.Web.Mvc.Views
{
	public abstract class WebViewPage<TModel, TKey, TUser, TUserManager, TController> : System.Web.Mvc.WebViewPage<TModel>
		where TKey : IEquatable<TKey>
		where TUser : class, IUser<TKey>
		where TUserManager : UserManager<TUser, TKey>
		where TController : Controller<TKey, TUser, TUserManager>
	{
		protected WebViewPage()
		{
		}

		//public override void InitHelpers()
		//{
		//	base.InitHelpers();
		//	// initialize any helpers here
		//}

		public TController Controller => (TController)ViewContext.Controller;

		public bool IsRightToLeft => Controller.IsRightToLeft;
	}

	public abstract class WebViewPage<TModel, TUser, TUserManager, TController> : WebViewPage<TModel, string, TUser, TUserManager, TController>
		where TUser : class, IUser<string>
		where TUserManager : UserManager<TUser>
		where TController : Controller<TUser, TUserManager>
	{
		protected WebViewPage() { }
	}

	public abstract class WebViewPage<TModel, TUserManager, TController> : WebViewPage<TModel, IUser<string>, TUserManager, TController>
		where TUserManager : UserManager<IUser<string>>
		where TController : Controller<TUserManager>
	{
		protected WebViewPage() { }
	}

	public abstract class WebViewPage<TModel, TController> : WebViewPage<TModel, UserManager<IUser<string>>, TController>
		where TController : Controller<UserManager<IUser<string>>>
	{
		protected WebViewPage() { }
	}

	public abstract class WebViewPage<TModel> : WebViewPage<TModel, Controller>
	{
		protected WebViewPage() { }
	}

	public abstract class WebViewPage : WebViewPage<object>
	{
		protected WebViewPage() { }
	}
}