using System;
using System.Globalization;
using System.Linq;
using essentialMix.Core.Web.Formatters.Csv.Mvc.Formatters.Csv;
using essentialMix.Core.Web.Formatters.Csv.Mvc.Formatters.Csv.Internal;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class CoreWebFormattersCsvMvcOptionsExtension
{
	[NotNull]
	public static MvcOptions ConfigureCsvFormatter([NotNull] this MvcOptions thisValue, [NotNull] Action<CsvConfiguration> configureCsv)
	{
		thisValue.RespectBrowserAcceptHeader = true;

		CsvConfiguration settings = new CsvConfiguration(CultureInfo.InvariantCulture);
		configureCsv(settings);
			
		CsvInputFormatter input = thisValue.InputFormatters.OfType<CsvInputFormatter>()
											.FirstOrDefault();

		if (input == null)
		{
			input = new CsvInputFormatter(settings);
			thisValue.InputFormatters.Add(input);
		}
			
		CsvOutputFormatter output = thisValue.OutputFormatters.OfType<CsvOutputFormatter>()
											.FirstOrDefault();
			
		if (output == null)
		{
			output = new CsvOutputFormatter(settings);
			thisValue.OutputFormatters.Add(output);
		}

		thisValue.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValues.ApplicationCsv);
		return thisValue;
	}
}