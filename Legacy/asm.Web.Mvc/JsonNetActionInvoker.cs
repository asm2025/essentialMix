using System.Collections.Generic;
using System.Web.Mvc;

namespace asm.Web.Mvc
{
	public class JsonNetActionInvoker : ControllerActionInvoker
	{
		protected override ActionResult InvokeActionMethod(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
		{
			ActionResult invokeActionMethod = base.InvokeActionMethod(controllerContext, actionDescriptor, parameters);
			return invokeActionMethod is JsonResult jsonResult 
				? JsonNetResult.FromJsonResult(jsonResult) 
				: invokeActionMethod;
		}
	}
}