using asm.Extensions;
using asm.Web;

namespace asm.Core.Web.Middleware
{
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
}