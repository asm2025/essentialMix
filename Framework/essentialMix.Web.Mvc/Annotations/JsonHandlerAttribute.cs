using System.Web.Mvc;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Annotations;

/// <summary>
/// Register this in Global.asax Application_Start()
/// GlobalFilters.Filters.Add(new JsonHandlerAttribute());
/// or in App_Start/FilterConfig.cs RegisterGlobalFilters
/// filters.Add(new JsonHandlerAttribute());
/// </summary>
public class JsonHandlerAttribute : ActionFilterAttribute, IActionFilter
{
	public JsonHandlerAttribute()
	{
	}
	void IActionFilter.OnActionExecuted([NotNull] ActionExecutedContext filterContext)
	{
		OnActionExecuted(filterContext);
	}

	public override void OnActionExecuted([NotNull] ActionExecutedContext filterContext)
	{
		if (filterContext.Result is JsonResult jsonResult)
		{
			filterContext.Result = new JsonNetResult
			{
				ContentEncoding = jsonResult.ContentEncoding,
				ContentType = jsonResult.ContentType,
				Data = jsonResult.Data,
				JsonRequestBehavior = jsonResult.JsonRequestBehavior
			};
		}

		base.OnActionExecuted(filterContext);
	}
}