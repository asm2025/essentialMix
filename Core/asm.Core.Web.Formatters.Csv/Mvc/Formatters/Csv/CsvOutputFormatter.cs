using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using asm.Core.Web.Formatters.Csv.Mvc.Formatters.Csv.Internal;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace asm.Core.Web.Formatters.Csv.Mvc.Formatters.Csv
{
	public class CsvOutputFormatter : TextOutputFormatter
	{
		/// <inheritdoc />
		public CsvOutputFormatter([NotNull] CsvConfiguration configuration) 
		{
			Configuration = configuration;

			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);
			if (configuration.Encoding != null && !SupportedEncodings.Contains(configuration.Encoding)) SupportedEncodings.Add(configuration.Encoding);

			SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationCsv);
			SupportedMediaTypes.Add(MediaTypeHeaderValues.TextCsv);
			SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyCsvSyntax);
		}

		[NotNull]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CsvConfiguration Configuration { get; }

		/// <inheritdoc />
		public override Task WriteResponseBodyAsync([NotNull] OutputFormatterWriteContext context, [NotNull] Encoding selectedEncoding)
		{
			if (context == null) throw new ArgumentNullException(nameof (context));
			if (selectedEncoding == null) throw new ArgumentNullException(nameof (selectedEncoding));
			if (context.Object == null) return Task.CompletedTask;
			
			using (TextWriter writer = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding))
			{
				Write(writer, context.Object);
				return writer.FlushAsync();
			}
		}

		public void Write([NotNull] TextWriter writer, object value)
		{
			if (value == null) return;

			using (CsvWriter csvWriter = new CsvWriter(writer, Configuration))
			{
				if (value is IEnumerable enumerable) 
					csvWriter.WriteRecords(enumerable);
				else
					csvWriter.WriteRecord(value);
			}
		}

		public void Write<T>([NotNull] TextWriter writer, T value)
		{
			if (value == null) return;

			using (CsvWriter csvWriter = new CsvWriter(writer, Configuration))
			{
				if (value is IEnumerable enumerable) 
					csvWriter.WriteRecords(enumerable);
				else
					csvWriter.WriteRecord(value);
			}
		}
	}
}
