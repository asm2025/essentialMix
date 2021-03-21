using System.Web.Mvc;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Annotations
{
	/// <summary>
	/// Register this in Global.asax Application_Start()
	/// GlobalFilters.Filters.Add(new AllowJsonGetAttribute());
	/// or in App_Start/FilterConfig.cs RegisterGlobalFilters
	/// filters.Add(new AllowJsonGetAttribute());
	/// </summary>
	public class AllowJsonGetAttribute : ActionFilterAttribute, IActionFilter
	{
		public AllowJsonGetAttribute()
		{
		}
		void IActionFilter.OnActionExecuted([NotNull] ActionExecutedContext filterContext)
		{
			if (filterContext.Result is JsonResult jsonResult)
				jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
		}

		public override void OnResultExecuting([NotNull] ResultExecutingContext filterContext)
		{
			if (filterContext.Result is JsonResult jsonResult)
				jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

			base.OnResultExecuting(filterContext);
		}
	}
}