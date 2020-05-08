using System.Net.Http;

namespace asm.Web
{
	/// <summary>
	/// https://github.com/alexanderar/Mvc.CascadeDropDown/blob/master/Mvc.CascadeDropDown/CascadeDropDownOptions.cs
	/// </summary>
	public class JQuerySettings
	{
		/// <summary>
		/// GET or POST (GET is default)
		/// </summary>
		public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

		public JQueryClientSideEvents ClientSideEvents { get; } = new JQueryClientSideEvents();
	}
}
