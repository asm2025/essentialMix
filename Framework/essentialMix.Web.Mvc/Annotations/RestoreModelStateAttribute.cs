using System.Web.Mvc;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Annotations;

/// <summary>
/// Register this in Global.asax Application_Start()
/// GlobalFilters.Filters.Add(new RestoreModelStateAttribute());
/// or in App_Start/FilterConfig.cs RegisterGlobalFilters
/// filters.Add(new RestoreModelStateAttribute());
/// </summary>
public class RestoreModelStateAttribute : ActionFilterAttribute
{
	public RestoreModelStateAttribute()
	{
	}

	public override void OnActionExecuting([NotNull] ActionExecutingContext filterContext)
	{
		base.OnActionExecuting(filterContext);
		if (!filterContext.Controller.TempData.ContainsKey("ModelState")) return;
		filterContext.Controller.ViewData.ModelState.Merge((ModelStateDictionary)filterContext.Controller.TempData["ModelState"]);
	}
}