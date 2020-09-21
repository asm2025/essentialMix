using System;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using asm.Core.Web.Mvc.Formatters.Raw;
using asm.Core.Web.Mvc.Formatters.Raw.Internal;
using TextInputFormatter = asm.Core.Web.Mvc.Formatters.Text.TextInputFormatter;
using TextOutputFormatter = asm.Core.Web.Mvc.Formatters.Text.TextOutputFormatter;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class MvcOptionsExtension
	{
		[NotNull]
		public static MvcOptions SkipSsl([NotNull] this MvcOptions thisValue)
		{
			RequireHttpsAttribute requireHttps = thisValue.Filters.OfType<RequireHttpsAttribute>().FirstOrDefault();
			if (requireHttps != null) thisValue.Filters.Remove(requireHttps);
			return thisValue;
		}

		[NotNull]
		public static MvcOptions RequireSsl([NotNull] this MvcOptions thisValue)
		{
			RequireHttpsAttribute requireHttps = thisValue.Filters.OfType<RequireHttpsAttribute>().FirstOrDefault();
			if (requireHttps == null) thisValue.Filters.Add<RequireHttpsAttribute>();
			return thisValue;
		}

		[NotNull]
		public static MvcOptions ConfigureRawFormatter([NotNull] this MvcOptions thisValue)
		{
			thisValue.RespectBrowserAcceptHeader = true;
			RawInputFormatter input = thisValue.InputFormatters.OfType<RawInputFormatter>()
															.FirstOrDefault();
			
			if (input == null)
			{
				input = new RawInputFormatter();
				thisValue.InputFormatters.Add(input);
			}

			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("plain", MediaTypeHeaderValues.TextPlain);
			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("octet-stream", MediaTypeHeaderValues.OctetStream);
			return thisValue;
		}

		[NotNull]
		public static MvcOptions ConfigureTextFormatter([NotNull] this MvcOptions thisValue)
		{
			thisValue.RespectBrowserAcceptHeader = true;
			TextInputFormatter input = thisValue.InputFormatters.OfType<TextInputFormatter>()
												.FirstOrDefault();
			
			if (input == null)
			{
				input = new TextInputFormatter();
				thisValue.InputFormatters.Add(input);
			}
			
			TextOutputFormatter output = thisValue.OutputFormatters.OfType<TextOutputFormatter>()
												.FirstOrDefault();
			
			if (output == null)
			{
				output = new TextOutputFormatter();
				thisValue.OutputFormatters.Add(output);
			}

			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("plain", Core.Web.Mvc.Formatters.Text.Internal.MediaTypeHeaderValues.TextPlain);
			return thisValue;
		}

		[NotNull]
		public static MvcOptions ConfigureXmlFormatter([NotNull] this MvcOptions thisValue, [NotNull] Action<XmlWriterSettings> configureXml)
		{
			thisValue.RespectBrowserAcceptHeader = true;
			XmlSerializerInputFormatter input = thisValue.InputFormatters.OfType<XmlSerializerInputFormatter>()
															.FirstOrDefault();
			
			if (input == null)
			{
				input = new XmlSerializerInputFormatter(thisValue);
				thisValue.InputFormatters.Add(input);
			}
			
			XmlSerializerOutputFormatter output = thisValue.OutputFormatters.OfType<XmlSerializerOutputFormatter>()
															.FirstOrDefault();
			
			if (output == null)
			{
				output = new XmlSerializerOutputFormatter();
				thisValue.OutputFormatters.Add(output);
			}

			configureXml(output.WriterSettings);
			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeHeaderValue.Parse("application/xml"));
			return thisValue;
		}

		[NotNull]
		public static MvcOptions ConfigureXmlDataContractFormatter([NotNull] this MvcOptions thisValue, [NotNull] Action<XmlWriterSettings> configureXml)
		{
			thisValue.RespectBrowserAcceptHeader = true;

			XmlDataContractSerializerInputFormatter dataContractInput = thisValue.InputFormatters.OfType<XmlDataContractSerializerInputFormatter>()
																				.FirstOrDefault();
			
			if (dataContractInput == null)
			{
				dataContractInput = new XmlDataContractSerializerInputFormatter(thisValue);
				thisValue.InputFormatters.Add(dataContractInput);
			}

			XmlDataContractSerializerOutputFormatter dataContractOutput = thisValue.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()
																					.FirstOrDefault();
			
			if (dataContractOutput == null)
			{
				dataContractOutput = new XmlDataContractSerializerOutputFormatter();
				thisValue.OutputFormatters.Add(dataContractOutput);
			}

			configureXml(dataContractOutput.WriterSettings);
			thisValue.FormatterMappings.SetMediaTypeMappingForFormat("dcxml", MediaTypeHeaderValue.Parse("application/dcxml"));
			return thisValue;
		}
	}
}