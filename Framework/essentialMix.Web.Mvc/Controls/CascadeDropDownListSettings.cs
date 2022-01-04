using System;

namespace essentialMix.Web.Mvc.Controls;

public class CascadeDropDownListSettings : JQuerySettings
{
	public Uri Url { get; set; }
	public string AjaxActionParamName { get; set; }
	public string Id { get; set; }
	public string OptionLabel { get; set; }
	public string SelectedValue { get; set; }
	public bool DisabledWhenParentNotSelected { get; set; }
}