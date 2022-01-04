using System;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Models;

public class ControllerActionData
{
	public ControllerActionData([AspMvcController, NotNull]string controller, [AspMvcAction, NotNull]string action)
		: this(null, controller, action)
	{
	}
		
	public ControllerActionData([AspMvcArea]string area, [AspMvcController, NotNull]string controllerName, [AspMvcAction, NotNull]string actionName)
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
		if (RouteData == null) return urlHelper.Action(ActionName, ControllerName, new {area = Area});
		RouteData.Values["area"] = Area;
		return urlHelper.Action(ActionName, ControllerName, RouteData);
	}
}