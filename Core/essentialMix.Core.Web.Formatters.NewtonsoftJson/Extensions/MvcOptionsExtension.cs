using System;
using System.Buffers;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class MvcOptionsExtension
	{
		[NotNull]
		public static MvcOptions ConfigureJsonFormatter([NotNull] this MvcOptions thisValue, [NotNull] Action<JsonSerializerSettings> configureJson, [NotNull] ILogger logger)
		{
			thisValue.RespectBrowserAcceptHeader = true;

			MvcNewtonsoftJsonOptions mvcJsonOptions = new MvcNewtonsoftJsonOptions
			{
				AllowInputFormatterExceptionMessages = true
			};
			JsonSerializerSettings settings = mvcJsonOptions.SerializerSettings;
			configureJson(settings);

			NewtonsoftJsonInputFormatter input = thisValue.InputFormatters.OfType<NewtonsoftJsonInputFormatter>()
												.FirstOrDefault();

			if (input == null)
			{
				input = new NewtonsoftJsonInputFormatter(logger, settings, ArrayPool<char>.Shared, new DefaultObjectPoolProvider(), thisValue, mvcJsonOptions);
				thisValue.InputFormatters.Add(input);
			}

			NewtonsoftJsonOutputFormatter output = thisValue.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()
															.FirstOrDefault();

			if (output == null)
			{
				output = new NewtonsoftJsonOutputFormatter(settings, ArrayPool<char>.Shared, thisValue, mvcJsonOptions);
				thisValue.OutputFormatters.Add(output);
			}

			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("json", MediaTypeHeaderValues.ApplicationJson);
			return thisValue;
		}
	}
}