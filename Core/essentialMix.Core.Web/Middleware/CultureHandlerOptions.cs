using essentialMix.Extensions;
using essentialMix.Web;

namespace essentialMix.Core.Web.Middleware;

public class CultureHandlerOptions
{
	private string _parameterName = RequestParameterNames.Culture;

	public CultureHandlerOptions() 
	{
	}

	public string ParameterName
	{
		get => _parameterName;
		set => _parameterName = value.ToNullIfEmpty() ?? RequestParameterNames.Culture;
	}
}