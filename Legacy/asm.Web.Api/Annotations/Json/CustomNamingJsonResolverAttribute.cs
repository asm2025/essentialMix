using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace asm.Web.Api.Annotations.Json
{
	public abstract class CustomJsonResolverAttribute : Attribute, IControllerConfiguration
	{
		/// <inheritdoc />
		protected CustomJsonResolverAttribute([NotNull] NamingStrategy strategy)
			: this(new DefaultContractResolver { NamingStrategy = strategy })
		{
		}

		/// <inheritdoc />
		protected CustomJsonResolverAttribute([NotNull] IContractResolver resolver)
		{
			Resolver = resolver;
		}

		protected IContractResolver Resolver { get; }

		/// <inheritdoc />
		public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
		{
			JsonMediaTypeFormatter formatter = controllerSettings.Formatters.JsonFormatter;
			controllerSettings.Formatters.Remove(formatter);

			JsonSerializerSettings settings = JsonHelper.CreateSettings(true, contractResolver: Resolver).AddConverters();
			formatter = new JsonMediaTypeFormatter
			{
				SerializerSettings = settings
			};

			controllerSettings.Formatters.Insert(0, formatter);
		}
	}
}
