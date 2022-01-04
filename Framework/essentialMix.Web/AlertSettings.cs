namespace essentialMix.Web;

public class AlertSettings
{
	public AlertSettings()
	{
		SuccessIsDismissable = true;
		InfoIsDismissable = true;
	}

	public bool ErrorIsDismissable { get; set; }
	public bool WarningIsDismissable { get; set; } 
	public bool SuccessIsDismissable { get; set; } 
	public bool InfoIsDismissable { get; set; }

	public string CssClass { get; set; } = "alert";
	public string CssDismissableClass { get; set; } = "alert-dismissable";
	public string CssErrorClass { get; set; } = "alert-danger";
	public string CssWarningClass { get; set; } = "alert-warning";
	public string CssSuccessClass { get; set; } = "alert-success";
	public string CssInfoClass { get; set; } = "alert-info";
}