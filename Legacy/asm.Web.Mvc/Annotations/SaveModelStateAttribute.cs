using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Annotations
{
	/// <summary>
	/// Register this in Global.asax Application_Start()
	/// GlobalFilters.Filters.Add(new SaveModelStateAttribute());
	/// or in App_Start/FilterConfig.cs RegisterGlobalFilters
	/// filters.Add(new SaveModelStateAttribute());
	/// </summary>
	public class SaveModelStateAttribute : ActionFilterAttribute
	{
		public SaveModelStateAttribute()
		{
		}

		public override void OnActionExecuted([NotNull] ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			filterContext.Controller.TempData["ModelState"] = filterContext.Controller.ViewData.ModelState;
		}
	}
}