using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using asm.Web.Mvc.Extensions;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Controls
{
	/// <summary>
	/// https://github.com/alexanderar/Mvc.CascadeDropDown/blob/master/Mvc.CascadeDropDown/DropDownListExtensions.cs
	/// </summary>
	public static class CascadeDropDownListExtension
	{
		#region Js Functions
		/// <summary>
		/// 0 - cascading dropdown element Id
		/// 1 - triggeredByProperty - id of parent element that triggers data loading
		/// 2 - preselected value
		/// 3 - if element was initially disabled, will contain removeAttribute('disabled') command
		/// 4 - if optionLabel is set should be set to '<option value="">optionLabel</option>' otherwise should be set to ""
		/// 5 - if element should be disabled when parent not selected, will contain setAttribute('disabled','disabled') command
		/// </summary>
		private const string Js1CreateInitFunction = @"
function initCascadeDropDownFor{0}() {{
	var triggerElement = document.getElementById('{1}');
	var targetElement = document.getElementById('{0}');
	var preselectedValue = '{2}';
	var triggerElementChanged = function(e) {{
		{3}
		var value = triggerElement.value;
		var items = {4};
		if (!value) {{
			targetElement.innerHTML = items;
			targetElement.value = '';
			var event = document.createEvent('HTMLEvents');
			event.initEvent('change', true, false);
			targetElement.dispatchEvent(event);
			{5}
			return;
		}}";

		/// <summary>
		/// {0} - CascadeDropDownListSettings.BeforeSend function name
		/// {1} - ajaxActionParamName
		/// </summary>
		private const string Js2GenerateJsonToSendFromFunctionFormat = @"
	var jsonToSend = {{ {1} : value }};
	var updatedJson = {0}(jsonToSend);
	if(updatedJson){{jsonToSend = updatedJson;}}
	";

		/// <summary>
		/// {0} - ajaxActionParamName
		/// </summary>
		private const string Js2SimpleGenerateJsonToSendFormat = @"
	var jsonToSend = {{ {0} : value }};";

		/// <summary>
		/// used when CascadeDropDownListSettings.HttpMethod is set to POST
		/// </summary>
		private const string Js3InitializePostRequest = @"
	var request = new XMLHttpRequest();			
	var url = targetElement.dataset.cascadeDdUrl;
	request.open('POST', url, true);
	request.setRequestHeader('Content-Type', 'application/json');";

		/// <summary>
		/// used when CascadeDropDownListSettings.HttpMethod is not set, or set to GET.
		/// </summary>
		private const string Js3InitializeGetRequest = @"
	var request = new XMLHttpRequest();			
	var url = targetElement.dataset.cascadeDdUrl;var appndSgn = url.indexOf('?') > -1 ? '&' : '?';
	var qs = Object.keys(jsonToSend).map(function(key){return key+'='+encodeURIComponent(jsonToSend[key])}).join('&');
	request.open('GET', url+appndSgn+qs, true);";

		/// <summary>
		/// {0} -  will have a call to CascadeDropDownListSettings.ClientSideEvents.OnComplete if it was set.
		/// {1} -  will have a call to CascadeDropDownListSettings.ClientSideEvents.OnSuccess if it was set.
		/// {2} -  will have a call to CascadeDropDownListSettings.ClientSideEvents.OnFailure if it was set.
		/// </summary>
		private const string Js4OnLoadFormat = @"
	var isSelected = false;
	request.onload = function () {{
	if (request.status >= 200 && request.status < 400) {{
		var data = JSON.parse(request.responseText);
		{0}
		{1}
		if (data) {{
			data.forEach(function(item, i) {{
				items += '<option value=""' + item.Value + '""'
				if(item.Disabled){{items += ' disabled'}}
				if(item.Selected){{items += ' selected'}}
				items += '>' + item.Text + '</option>';
			}});
			targetElement.innerHTML = items;
			if(preselectedValue)
			{{
				targetElement.value = preselectedValue;
				preselectedValue = null;
			}}
			var event = document.createEvent('HTMLEvents');
			event.initEvent('change', true, false);
			targetElement.dispatchEvent(event);
		}}
	}}
	
	if (request.status >= 400){{
		var placeholder = targetElement.dataset.optionLbl;
		targetElement.innerHTML = null;
		if(placeholder)
		{{
			targetElement.innerHTML = '<option value="""">' + placeholder + '</option>';
		}}
		{2}
	}}
}};";

		/// <summary>
		/// {0} -  will have a call to CascadeDropDownListSettings.ClientSideEvents.OnComplete if it was set.
		/// {1} -  will have a call to CascadeDropDownListSettings.ClientSideEvents.OnFailure if it was set.
		/// </summary>
		private const string Js5ErrorCallback = @"
	request.onerror = function () {{
	{0}
	{1}
	}};";

		private const string Js6SendPostRequest = @"
	request.send(JSON.stringify(jsonToSend));";

		private const string Js6SendGetRequest = @"
	request.send();";

		/// <summary>
		/// {0} - cascading dropdown element Id
		/// </summary>
		private const string Js7EndFormat = @"
		}};
		
		triggerElement.addEventListener('change', triggerElementChanged);
		
		if(triggerElement.value && !targetElement.value)
		{{
			triggerElementChanged();
		}} 
	}};
	
	if (document.readyState != 'loading') {{
		initCascadeDropDownFor{0}();
	}}
	else {{
		document.addEventListener('DOMContentLoaded', initCascadeDropDownFor{0});
	}}";

		/*
		/// <summary>
		///	 The pure JavaScript  format.
		/// </summary>
		/// <remarks>
		///	 0 - triggerMemberInfo.Name
		///	 1 - URL
		///	 2 - ajaxActionParamName
		///	 3 - optionLabel
		///	 4 - dropdownElementId
		///	 5 - Preselected Value
		///	 6 - if element should be disabled when parent not selected, will contain setAttribute('disabled','disabled') command
		///	 7 - if element was initially disabled, will contain removeAttribute('disabled') command
		///	 8 - if modify data before send function is provided, will contain the code that invokes this function
		/// </remarks>
		private const string PureJsScriptFormat = @"
<script>
	function initCascadeDropDownFor{4}() {{
		var triggerElement = document.getElementById('{0}');
		var targetElement = document.getElementById('{4}');
		var preselectedValue = '{5}';
		triggerElement.addEventListener('change', function(e) {{
			{7}
			var value = triggerElement.value;
			var items = '<option value="""">{3}</option>';
			
			if (!value) {{
				targetElement.innerHTML = items;
				targetElement.value = '';
				var event = document.createEvent('HTMLEvents');
				event.initEvent('change', true, false);
				targetElement.dispatchEvent(event);
				{6}
				return;
			}}

			var jsonToSend = {{ {2} : value }};
			var request = new XMLHttpRequest();
			request.open('POST', '{1}', true);
			request.setRequestHeader('Content-Type', 'application/json');
			var isSelected = false;
			request.onload = function () {{
				if (request.status >= 200 && request.status < 400) {{
					// Success!
					var data = JSON.parse(request.responseText);
					if (data) {{
						data.forEach(function(item, i) {{
							items += '<option value=""' + item.Value + '"">' + item.Text + '</option>';
						}});
						targetElement.innerHTML = items;
						if(preselectedValue)
						{{
							targetElement.value = preselectedValue;
							preselectedValue = null;
						}}
						var event = document.createEvent('HTMLEvents');
						event.initEvent('change', true, false);
						targetElement.dispatchEvent(event);
					}}
				}} else {{
					console.log(request.statusText);
				}}
			}};
			request.onerror = function (error) {{
				console.log(error);
			}};
			{8}
			request.send(JSON.stringify(jsonToSend));
		}});
		if(triggerElement.value && !targetElement.value)
		{{
			var event = document.createEvent('HTMLEvents');
			event.initEvent('change', true, false);
			triggerElement.dispatchEvent(event);
		}}
	}};
	if (document.readyState != 'loading') {{
		initCascadeDropDownFor{4}();
	}} else {{
		document.addEventListener('DOMContentLoaded', initCascadeDropDownFor{4});
	}}
</script>";
		*/
		#endregion

		[NotNull]
		public static MvcHtmlString CascadingDropDownList<TModel, TProperty>([NotNull] this HtmlHelper<TModel> thisValue, [NotNull] string name, [NotNull] Expression<Func<TModel, TProperty>> triggeredByProperty, CascadeDropDownListSettings settings = null, object htmlAttributes = null)
		{
			string triggeredByPropId = thisValue.GetElementIdFromExpression(triggeredByProperty)?.Trim();
			if (string.IsNullOrEmpty(triggeredByPropId)) throw new ArgumentException("Triggered by property id is missing.", nameof(triggeredByProperty));
			return CascadingDropDownList(thisValue, name, triggeredByPropId, settings, htmlAttributes);
		}

		[NotNull]
		public static MvcHtmlString CascadingDropDownList([NotNull] this HtmlHelper thisValue, [NotNull] string name, [NotNull] string triggeredByProperty, CascadeDropDownListSettings settings = null, object htmlAttributes = null)
		{
			settings ??= new CascadeDropDownListSettings();
			settings.SelectedValue = GetPropStringValue(thisValue.ViewData.Model, name);
			return CascadingDropDownListInternal(thisValue, name, triggeredByProperty, settings, Helpers.HtmlHelper.ToHtmlAttributes(htmlAttributes));
		}

		[NotNull]
		public static MvcHtmlString CascadingDropDownListFor<TModel, TProperty, TProperty2>([NotNull] this HtmlHelper<TModel> thisValue, [NotNull] Expression<Func<TModel, TProperty>> expression, [NotNull] Expression<Func<TModel, TProperty2>> triggeredByProperty, CascadeDropDownListSettings settings = null, object htmlAttributes = null)
		{
			string dropDownElementName = thisValue.GetElementNameFromExpression(expression)?.Trim();
			if (string.IsNullOrEmpty(dropDownElementName)) throw new ArgumentException("Drop down element name is missing.", nameof(expression));

			string triggeredByPropId = thisValue.GetElementIdFromExpression(triggeredByProperty)?.Trim();
			if (string.IsNullOrEmpty(triggeredByPropId)) throw new ArgumentException("Triggered by property id is missing.", nameof(triggeredByProperty));

			settings ??= new CascadeDropDownListSettings();
			if (string.IsNullOrWhiteSpace(settings.Id)) settings.Id = thisValue.GetElementIdFromExpression(expression);
			settings.SelectedValue = GetPropStringValue(thisValue.ViewData.Model, expression);

			return CascadingDropDownListInternal(thisValue, dropDownElementName, triggeredByPropId, settings, Helpers.HtmlHelper.ToHtmlAttributes(htmlAttributes));
		}

		[NotNull]
		public static MvcHtmlString CascadingDropDownListFor<TModel, TProperty>([NotNull] this HtmlHelper<TModel> thisValue, [NotNull] Expression<Func<TModel, TProperty>> expression, [NotNull] string triggeredByPropertyWithId, CascadeDropDownListSettings settings = null, object htmlAttributes = null)
		{
			string dropDownElementName = thisValue.GetElementNameFromExpression(expression)?.Trim();
			if (string.IsNullOrEmpty(dropDownElementName)) throw new ArgumentException("Drop down element name is missing.", nameof(expression));

			settings ??= new CascadeDropDownListSettings();
			if (string.IsNullOrWhiteSpace(settings.Id)) settings.Id = thisValue.GetElementIdFromExpression(expression);
			settings.SelectedValue = GetPropStringValue(thisValue.ViewData.Model, expression);

			return CascadingDropDownListInternal(thisValue, dropDownElementName, triggeredByPropertyWithId, settings, Helpers.HtmlHelper.ToHtmlAttributes(htmlAttributes));
		}

		[NotNull]
		public static MvcHtmlString CascadingDropDownList([NotNull] this HtmlHelper thisValue, [NotNull] string name, [NotNull] string triggeredByProperty, CascadeDropDownListSettings settings = null, RouteValueDictionary htmlAttributes = null)
		{
			settings ??= new CascadeDropDownListSettings();
			settings.SelectedValue = GetPropStringValue(thisValue.ViewData.Model, name);
			return CascadingDropDownListInternal(thisValue, name, triggeredByProperty, settings, htmlAttributes);
		}

		[NotNull]
		private static MvcHtmlString CascadingDropDownListInternal([NotNull] HtmlHelper thisValue, [NotNull] string name, [NotNull] string triggeredByProperty, CascadeDropDownListSettings settings = null, RouteValueDictionary htmlAttributes = null)
		{
			name = name.Trim();
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			settings ??= new CascadeDropDownListSettings();
			htmlAttributes ??= new RouteValueDictionary();
			
			string cascadeDdElementId = settings.Id?.Trim();
			if (string.IsNullOrEmpty(cascadeDdElementId)) cascadeDdElementId = GetDropDownElementId(htmlAttributes);
			if (string.IsNullOrEmpty(cascadeDdElementId)) cascadeDdElementId = name;

			htmlAttributes.Add("data-cascade-dd-url", settings.Url);

			string setDisableString = string.Empty;
			string removeDisabledString = string.Empty;
			if (settings.OptionLabel != null) htmlAttributes.Add("data-option-lbl", settings.OptionLabel);

			if (settings.DisabledWhenParentNotSelected)
			{
				htmlAttributes.Add("disabled", "disabled");
				setDisableString = "targetElement.setAttribute('disabled','disabled');";
				removeDisabledString = "targetElement.removeAttribute('disabled');";
			}

			MvcHtmlString defaultDropDownHtml = thisValue.DropDownList(name, Array.Empty<SelectListItem>(), settings.OptionLabel, htmlAttributes);
			StringBuilder builder = new StringBuilder(Constants.BUFFER_KB);
			string optionLblStr = settings.OptionLabel == null ? "''" : $@"'<option value="""">{settings.OptionLabel}</option>'";
			builder.AppendFormat(Js1CreateInitFunction, cascadeDdElementId, triggeredByProperty, settings.SelectedValue, removeDisabledString, optionLblStr, setDisableString);
			builder.Append(string.IsNullOrEmpty(settings.ClientSideEvents.BeforeSend)
				? string.Format(Js2SimpleGenerateJsonToSendFormat, settings.AjaxActionParamName)
				: string.Format(Js2GenerateJsonToSendFromFunctionFormat, settings.ClientSideEvents.BeforeSend, settings.AjaxActionParamName));
			builder.Append(settings.HttpMethod == null || settings.HttpMethod != HttpMethod.Post
				? Js3InitializeGetRequest
				: Js3InitializePostRequest);

			string onComplete = null;
			string onSuccess = null;
			string onFailure = null;

			if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnSuccess))
				onSuccess = $"{settings.ClientSideEvents.OnSuccess}(data);";

			if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnFailure))
				onFailure = $"{settings.ClientSideEvents.OnFailure}(request.responseText, request.status, request.statusText);";

			if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnComplete))
				onComplete = $"{settings.ClientSideEvents.OnComplete}(data, null);";

			builder.AppendFormat(Js4OnLoadFormat, onComplete ?? string.Empty, onSuccess ?? string.Empty, onFailure ?? string.Empty);

			onComplete = null;
			onFailure = null;


			if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnComplete) || !string.IsNullOrEmpty(settings.ClientSideEvents.OnFailure))
			{
				if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnComplete))
					onComplete = $"{settings.ClientSideEvents.OnComplete}(null, request.responseText);";

				if (!string.IsNullOrEmpty(settings.ClientSideEvents.OnSuccess))
					onFailure = $"{settings.ClientSideEvents.OnFailure}(request.responseText, request.status, request.statusText);";

				builder.AppendFormat(Js5ErrorCallback, onComplete, onFailure);
			}

			builder.Append(settings.HttpMethod == null || settings.HttpMethod != HttpMethod.Post
				? Js6SendGetRequest
				: Js6SendPostRequest);

			builder.AppendFormat(Js7EndFormat, cascadeDdElementId);
			return new MvcHtmlString(string.Concat(defaultDropDownHtml, Environment.NewLine, "<script type='text/javascript'>", builder, "</script>", Environment.NewLine));
		}

		private static string GetPropStringValue(object src, string propName)
		{
			string stringVal = null;

			if (src != null)
			{
				PropertyInfo property = src.GetType().GetProperty(propName);
				object propVal = property?.GetValue(src, null);
				stringVal = propVal?.ToString();
			}

			return stringVal;
		}

		[NotNull]
		private static string GetPropStringValue<TModel, TProp>(TModel src, [NotNull] Expression<Func<TModel, TProp>> expression)
		{
			Func<TModel, TProp> func = expression.Compile();
			string selectedValString = string.Empty;
			Type type = typeof(TProp);
			string defaultValString = type.IsValueType && Nullable.GetUnderlyingType(type) == null
					   ? Convert.ToString(Activator.CreateInstance(type))
					   : string.Empty;
			try
			{
				if (!ReferenceEquals(src, null))
				{
					TProp propVal = func(src);

					if (defaultValString != string.Empty && propVal.ToString() != defaultValString ||
						defaultValString == string.Empty && propVal != null)
					{
						selectedValString = propVal.ToString();
					}
				}
			}
			catch (Exception)
			{
				selectedValString = defaultValString;
			}

			return selectedValString;
		}

		private static string GetDropDownElementId(object htmlAttributes)
		{
			if (htmlAttributes == null) return null;

			PropertyInfo[] properties = htmlAttributes.GetType().GetProperties();
			PropertyInfo prop = Array.Find(properties, p => string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase));
			if (prop != null) return prop.GetValue(htmlAttributes, null).ToString();
			return null;
		}
	}
}
