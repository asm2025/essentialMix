using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using asm.Helpers;
using asm.Web;
using JetBrains.Annotations;

namespace asm.Core.Web
{
	// todo: convert to middleware?
	public class CultureMessageHandler : DelegatingHandler
	{
		private readonly string _parameterName;

		/// <inheritdoc />
		public CultureMessageHandler()
			: this(RequestParameterNames.Culture)
		{
		}

		/// <inheritdoc />
		public CultureMessageHandler(string parameterName)
		{
			parameterName = parameterName?.Trim();
			if (string.IsNullOrEmpty(parameterName)) parameterName = RequestParameterNames.Culture;
			_parameterName = parameterName;
		}

		protected override Task<HttpResponseMessage> SendAsync([NotNull] HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// here you can chose to get the lang from database, cookie or from the request if the culture is stored on local storage.
			CultureInfo ci = null;

			if (!string.IsNullOrEmpty(request.RequestUri.Query))
			{
				NameValueCollection queryString = HttpUtility.ParseQueryString(request.RequestUri.Query);
				string name = queryString[_parameterName]?.Trim();
				if (string.IsNullOrEmpty(name)) name = CultureInfoHelper.Default.Name;
				ci = CultureInfoHelper.Get(name);
			}

			if (ci == null && request.Headers.AcceptLanguage.Count > 0)
			{
				foreach (StringWithQualityHeaderValue headerValue in request.Headers.AcceptLanguage)
				{
					if (string.IsNullOrEmpty(headerValue.Value) || headerValue.Value.Length < 2) continue;
					ci = CultureInfoHelper.Get(headerValue.Value.Substring(0, 2));
					if (ci != null) break;
				}
			}

			if (ci != null)
			{
				request.Headers.AcceptLanguage.Clear();
				request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(ci.Name));
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}