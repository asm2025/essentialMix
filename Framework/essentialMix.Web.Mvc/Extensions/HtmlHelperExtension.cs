using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI;
using essentialMix.Web;
using JetBrains.Annotations;
using PagerSettings = essentialMix.Web.Mvc.PagerSettings;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class HtmlHelperExtension
	{
		[NotNull]
		public static ViewContext RootContext([NotNull] this HtmlHelper thisValue)
		{
			ViewContext viewContext = thisValue.ViewContext;

			while (viewContext.ParentActionViewContext != null)
				viewContext = viewContext.ParentActionViewContext;

			return viewContext;
		}

		public static T If<T>([NotNull] this HtmlHelper thisValue, [NotNull] Func<bool> evaluator, T trueResponse, T falseResponse = default(T))
		{
			return evaluator().Iif(trueResponse, falseResponse);
		}

		public static T IfNot<T>([NotNull] this HtmlHelper thisValue, [NotNull] Func<bool> evaluator, T trueResponse, T falseResponse = default(T))
		{
			return evaluator().IifNot(trueResponse, falseResponse);
		}

		public static bool IsSameController([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return controller.IsSame((string)context.RouteData.Values["controller"]);
		}

		public static T IfSameController<T>([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameController(thisValue, controller, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsSameAction([NotNull] this HtmlHelper thisValue, [AspMvcAction] string action, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return action.IsSame((string)context.RouteData.Values["action"]);
		}

		public static T IfSameAction<T>([NotNull] this HtmlHelper thisValue, [AspMvcAction] string action, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameAction(thisValue, action, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsSameArea([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return area.IsSame((string)context.RouteData.Values["area"]);
		}

		public static T IfSameArea<T>([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameArea(thisValue, area, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsSameControllerAndAction([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, [AspMvcAction] string action, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return controller.IsSame((string)context.RouteData.Values["controller"]) && action.IsSame((string)context.RouteData.Values["action"]);
		}

		public static T IfSameControllerAndAction<T>([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, [AspMvcAction] string action, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameControllerAndAction(thisValue, controller, action, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsSameAreaAndController([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return area.IsSame((string)context.RouteData.Values["area"]) && controller.IsSame((string)context.RouteData.Values["controller"]);
		}

		public static T IfSameAreaAndController<T>([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameAreaAndController(thisValue, area, controller, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsSameAreaAndControllerAndAction([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, [AspMvcAction] string action, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			return area.IsSame((string)context.RouteData.Values["area"]) && controller.IsSame((string)context.RouteData.Values["controller"]) && action.IsSame((string)context.RouteData.Values["action"]);
		}

		public static T IfSameAreaAndControllerAndAction<T>([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, [AspMvcAction] string action, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsSameAreaAndControllerAndAction(thisValue, area, controller, action, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyController([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcController] string[] controllers, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string controller = (string)context.RouteData.Values["controller"];
			return controller.IsSameAsAny(controllers);
		}

		public static T IfAnyController<T>([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcController] string[] controllers, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyController(thisValue, controllers, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyAction([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcAction] string[] actions, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string action = (string)context.RouteData.Values["action"];
			return action.IsSameAsAny(actions);
		}

		public static T IfAnyAction<T>([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcAction] string[] actions, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyAction(thisValue, actions, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyArea([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcAction] string[] areas, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string area = (string)context.RouteData.Values["area"];
			return area.IsSameAsAny(areas);
		}

		public static T IfAnyArea<T>([NotNull] this HtmlHelper thisValue, [NotNull][AspMvcAction] string[] areas, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyArea(thisValue, areas, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyControllerAction([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, [NotNull][AspMvcAction] string[] actions, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string action = (string)context.RouteData.Values["action"];
			return controller.IsSame((string)context.RouteData.Values["controller"]) && action.IsSameAsAny(actions);
		}

		public static T IfAnyControllerAction<T>([NotNull] this HtmlHelper thisValue, [AspMvcController] string controller, [NotNull][AspMvcAction] string[] actions, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyControllerAction(thisValue, controller, actions, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyAreaController([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [NotNull][AspMvcController] string[] controllers, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string controller = (string)context.RouteData.Values["controller"];
			return area.IsSame((string)context.RouteData.Values["area"]) && controller.IsSameAsAny(controllers);
		}

		public static T IfAnyAreaController<T>([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [NotNull][AspMvcController] string[] controllers, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyAreaController(thisValue, area, controllers, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static bool IsAnyAreaControllerAction([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, [NotNull][AspMvcAction] string[] actions, bool compareToRoot = true)
		{
			ViewContext context = compareToRoot ? RootContext(thisValue) : thisValue.ViewContext;
			string action = (string)context.RouteData.Values["action"];
			return area.IsSame((string)context.RouteData.Values["area"]) && controller.IsSame((string)context.RouteData.Values["controller"]) && action.IsSameAsAny(actions);
		}

		public static T IfAnyAreaControllerAction<T>([NotNull] this HtmlHelper thisValue, [AspMvcArea] string area, [AspMvcController] string controller, [NotNull][AspMvcAction] string[] actions, T trueResponse, T falseResponse = default(T), bool compareToRoot = true)
		{
			return IsAnyAreaControllerAction(thisValue, area, controller, actions, compareToRoot)
						? trueResponse
						: falseResponse;
		}

		public static TagBuilder PageLinks([NotNull] this HtmlHelper thisValue, [NotNull] PagerSettings pagerSettings)
		{
			return Web.Mvc.Helpers.HtmlHelper.PageLinks(pagerSettings);
		}

		public static MvcHtmlString PageLinksAction([NotNull] this HtmlHelper thisValue, [NotNull] PagerSettings pagerSettings)
		{
			return Web.Mvc.Helpers.HtmlHelper.PageLinksAction(pagerSettings);
		}

		[NotNull]
		public static MvcHtmlString Messages([NotNull] this HtmlHelper thisValue, AlertSettings alertSettings = null)
		{
			AlertSettings settings = alertSettings ?? new AlertSettings();
			StringBuilder sb = new StringBuilder();
			IList<TagBuilder> tags = new List<TagBuilder>();

			// Error
			tags.Clear();
			string value = Convert.ToString(thisValue.ViewBag.Error);
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			value = thisValue.ViewContext.TempData.ContainsKey("Error") ? Convert.ToString(thisValue.ViewContext.TempData["Error"]) : null;
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			if (!thisValue.ViewContext.ViewData.ModelState.IsValid
				&& thisValue.ViewContext.ViewData.ModelState.ContainsKey(string.Empty)
				&& thisValue.ViewContext.ViewData.ModelState[string.Empty].Errors.Count > 0)
			{
				value = thisValue.ValidationSummary(true)?.ToHtmlString();

				if (tags.Count > 0 || !string.IsNullOrEmpty(value))
				{
					TagBuilder divError = new TagBuilder(HtmlTextWriterTag.Div.ToString());
					divError.AddCssClass(settings.CssClass);
					if (settings.ErrorIsDismissable) divError.AddCssClass(settings.CssDismissableClass);
					divError.AddCssClass(settings.CssErrorClass);
					if (tags.Count > 0) divError.InnerHtml += string.Concat(tags);
					if (!string.IsNullOrEmpty(value)) divError.InnerHtml += value;
					sb.Append(divError);
				}
			}

			// Warning
			tags.Clear();
			value = Convert.ToString(thisValue.ViewBag.Warning);
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			value = thisValue.ViewContext.TempData.ContainsKey("Warning") ? Convert.ToString(thisValue.ViewContext.TempData["Warning"]) : null;
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			if (tags.Count > 0)
			{
				TagBuilder divWarning = new TagBuilder(HtmlTextWriterTag.Div.ToString());
				divWarning.AddCssClass(settings.CssClass);
				if (settings.ErrorIsDismissable) divWarning.AddCssClass(settings.CssDismissableClass);
				divWarning.AddCssClass(settings.CssWarningClass);
				divWarning.InnerHtml = string.Concat(tags);
				sb.Append(divWarning);
			}

			// Success
			tags.Clear();
			value = Convert.ToString(thisValue.ViewBag.Success);
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			value = thisValue.ViewContext.TempData.ContainsKey("Success") ? Convert.ToString(thisValue.ViewContext.TempData["Success"]) : null;
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			if (tags.Count > 0)
			{
				TagBuilder divSuccess = new TagBuilder(HtmlTextWriterTag.Div.ToString());
				divSuccess.AddCssClass(settings.CssClass);
				if (settings.ErrorIsDismissable) divSuccess.AddCssClass(settings.CssDismissableClass);
				divSuccess.AddCssClass(settings.CssSuccessClass);
				divSuccess.InnerHtml = string.Concat(tags);
				sb.Append(divSuccess);
			}

			// Message
			tags.Clear();
			value = Convert.ToString(thisValue.ViewBag.Message);
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			value = thisValue.ViewContext.TempData.ContainsKey("Message") ? Convert.ToString(thisValue.ViewContext.TempData["Message"]) : null;
			if (!string.IsNullOrEmpty(value)) tags.Add(new TagBuilder(HtmlTextWriterTag.Div.ToString()) { InnerHtml = value });

			if (tags.Count > 0)
			{
				TagBuilder divMessage = new TagBuilder(HtmlTextWriterTag.Div.ToString());
				divMessage.AddCssClass(settings.CssClass);
				if (settings.ErrorIsDismissable) divMessage.AddCssClass(settings.CssDismissableClass);
				divMessage.AddCssClass(settings.CssInfoClass);
				divMessage.InnerHtml += string.Concat(tags);
				sb.Append(divMessage);
			}

			return new MvcHtmlString(sb.ToString());
		}

		[NotNull]
		public static MvcHtmlString Breadcrumb([NotNull] this HtmlHelper thisValue, string defaultRouteName = "Home")
		{
			ViewContext context = RootContext(thisValue);
			TagBuilder rootDiv = new TagBuilder(HtmlTextWriterTag.Ol.ToString());
			rootDiv.AddCssClass("breadcrumb");

			IList<TagBuilder> tags = new List<TagBuilder>();
			tags.Add(new TagBuilder(HtmlTextWriterTag.Li.ToString()) { InnerHtml = thisValue.RouteLink("Home", defaultRouteName).ToHtmlString() });

			string controllerName = Convert.ToString(context.RouteData.Values["controller"]);
			if (!controllerName.IsSame("Home")) tags.Add(new TagBuilder(HtmlTextWriterTag.Li.ToString()) { InnerHtml = thisValue.ActionLink(controllerName, "Index", controllerName).ToHtmlString() });

			string actionName = Convert.ToString(context.RouteData.Values["action"]);
			if (!actionName.IsSame("Index")) tags.Add(new TagBuilder(HtmlTextWriterTag.Li.ToString()) { InnerHtml = thisValue.ActionLink(controllerName, actionName, controllerName).ToHtmlString() });

			tags[tags.Count - 1].AddCssClass("active");
			rootDiv.InnerHtml = string.Concat(tags);
			return new MvcHtmlString(rootDiv.ToString());
		}

		public static string GetSortOrder([NotNull] this HtmlHelper thisValue, string fieldName, string currentValue)
		{
			if (string.IsNullOrEmpty(fieldName)) return null;
			if (string.IsNullOrEmpty(currentValue)) return fieldName;
			if (currentValue.StartsWith(fieldName, StringComparison.OrdinalIgnoreCase)) return fieldName + " desc";
			return null;
		}

		public static bool IsAuthenticated([NotNull] this HtmlHelper thisValue)
		{
			HttpRequestBase request = thisValue.ViewContext.GetRequest();
			return request != null && request.IsAuthenticated;
		}

		[NotNull]
		public static string NameOfController<T>([NotNull] this HtmlHelper thisValue)
			where T : Controller
		{
			return typeof(T).Name.Replace("Controller", string.Empty);
		}

		public static string NameOfAction<T>([NotNull] this HtmlHelper thisValue, [NotNull] Expression<Func<T>> expression)
			where T : Controller
		{
			Type type = typeof(T);
			MethodBase methodInfo = type.GetMethod(expression);
			ActionNameAttribute actionName = methodInfo.GetAttribute<ActionNameAttribute>(true);
			return actionName == null ? methodInfo.Name : actionName.Name;
		}

		public static string GetElementIdFromExpression<TModel, TProperty>([NotNull] this HtmlHelper<TModel> thisValue, [NotNull] Expression<Func<TModel, TProperty>> expression)
		{
			return thisValue.ViewData?.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
		}

		public static string GetElementNameFromExpression<TModel, TProperty>([NotNull] this HtmlHelper<TModel> thisValue, [NotNull] Expression<Func<TModel, TProperty>> expression)
		{
			return thisValue.ViewData?.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
		}
	}
}
