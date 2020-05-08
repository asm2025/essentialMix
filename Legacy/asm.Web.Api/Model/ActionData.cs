using System;
using System.Web.Http.Routing;
using System.Web.Routing;
using JetBrains.Annotations;

namespace asm.Web.Api.Model
{
	public class ActionData
	{
		public ActionData([NotNull, AspMvcController] string controller, [NotNull, AspMvcAction] string action)
			: this(null, controller, action)
		{
		}
		
		public ActionData([CanBeNull, AspMvcArea] string area, [NotNull, AspMvcController] string controllerName, [NotNull, AspMvcAction] string actionName)
		{
			Area = area;
			ControllerName = controllerName;
			ActionName = actionName;
		}
		
		public string Area { get; set; }

		[NotNull]
		public string ControllerName { get; set; }

		[NotNull]
		public string ActionName { get; set; }

		public RouteData RouteData { get; set; }

		public string CreateUrl([NotNull] UrlHelper urlHelper)
		{
			if (urlHelper == null) throw new ArgumentNullException(nameof(urlHelper));
			if (RouteData == null) return urlHelper.Link(null, new { controller = ControllerName, action = ActionName, area = Area });
			RouteData.Values["area"] = Area;
			RouteData.Values["controller"] = ControllerName;
			RouteData.Values["action"] = ActionName;
			return urlHelper.Link(null, RouteData);
		}
	}
}