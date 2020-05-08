using System.Net;
using System.Web.Mvc;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Annotations
{
	/// <summary>
	/// Register this in Global.asax Application_Start()
	/// GlobalFilters.Filters.Add(new ValidationErrorAttribute());
	/// or in App_Start/FilterConfig.cs RegisterGlobalFilters
	/// filters.Add(new ValidationErrorAttribute());
	/// </summary>
	public class ValidationErrorAttribute : FilterAttribute, IExceptionFilter
	{
		public ValidationErrorAttribute()
		{
		}

		[NotNull]
		public string ViewName { get; set; } = "_ValidationError";

		public virtual void OnException([NotNull] ExceptionContext filterContext)
		{
			if (filterContext.Exception == null) return;
			filterContext.ExceptionHandled = true;
			filterContext.HttpContext.Response.Clear();
			filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
			filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			
			//return the partial view containing the validationsummary and set the ViewData
			//so that it displays the validation error messages
			filterContext.Result = new PartialViewResult
			{
				ViewName = ViewName,
				ViewData = filterContext.Controller.ViewData
			};
		}
	}
}