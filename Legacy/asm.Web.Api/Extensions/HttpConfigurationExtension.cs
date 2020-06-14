using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Xml;
using asm.Helpers;
using asm.Newtonsoft.Helpers;
using asm.Web.Handlers;
using JetBrains.Annotations;
using Microsoft.Owin.Security.OAuth;
using asm.Web.Api.Http;
using Newtonsoft.Json;

namespace asm.Web.Api.Extensions
{
	public static class HttpConfigurationExtension
	{
		[NotNull]
		public static HttpConfiguration UseBearerTokenAuthenticationOnly([NotNull] this HttpConfiguration thisValue)
		{
			thisValue.SuppressDefaultHostAuthentication();
			thisValue.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
			return thisValue;
		}

		[NotNull]
		public static HttpConfiguration MapDefaultRoutes([NotNull] this HttpConfiguration thisValue, object defaults = null, string prefix = null, string swaggerUrl = null)
		{
			prefix = UriHelper.Trim(prefix);
			if (!string.IsNullOrEmpty(prefix)) prefix += "/";
			prefix ??= string.Empty;

			thisValue.MapHttpAttributeRoutes();

			thisValue.Routes.MapHttpRoute(
										null,
										prefix + "{controller}",
										new { action = "Index", id = RouteParameter.Optional }
										);
			
			if (!string.IsNullOrWhiteSpace(swaggerUrl))
			{
				thisValue.Routes.MapHttpRoute(
											"Swagger",
											string.Empty,
											null,
											null,
											new RedirectHandler(swaggerUrl));
			}

			thisValue.Routes.MapHttpRoute(
										"Default",
										prefix + "{controller}/{action}/{id}",
										defaults ?? new { controller = "Home", action = "Index", id = RouteParameter.Optional });
			return thisValue;
		}

		[NotNull]
		public static HttpConfiguration UseOptimizationHandlers([NotNull] this HttpConfiguration thisValue)
		{
			thisValue.MessageHandlers.Add(new ChunkedTransferEncodingHandler());
			return thisValue;
		}

		[NotNull]
		public static HttpConfiguration UseCultureHandler([NotNull] this HttpConfiguration thisValue, string parameterName = null)
		{
			thisValue.MessageHandlers.Add(new CultureHandler(parameterName));
			return thisValue;
		}

		[NotNull]
		public static HttpConfiguration ConfigureXmlFormatter([NotNull] this HttpConfiguration thisValue, [NotNull] Action<XmlWriterSettings> configureXml)
		{
			XmlMediaTypeFormatter xmlFormatter = thisValue.Formatters.XmlFormatter;
			if (xmlFormatter != null) configureXml(xmlFormatter.WriterSettings);
			return thisValue;
		}

		[NotNull]
		public static HttpConfiguration ConfigureJsonFormatter([NotNull] this HttpConfiguration thisValue, [NotNull] Action<JsonSerializerSettings> configureJson)
		{
			JsonSerializerSettings settings = JsonHelper.CreateSettings();
			configureJson(settings);
			JsonConvert.DefaultSettings = () => settings;
			JsonMediaTypeFormatter jsonFormatter = thisValue.Formatters.JsonFormatter;
			if (jsonFormatter != null) jsonFormatter.SerializerSettings = settings;
			return thisValue;
		}
	}
}