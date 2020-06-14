using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using asm.Core.Web.Formatters.Csv.Mvc.Formatters.Csv.Internal;
using asm.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;

namespace asm.Core.Web.Formatters.Csv.Mvc.Formatters.Csv
{
	public class CsvInputFormatter : TextInputFormatter, IInputFormatterExceptionPolicy
	{
		/// <inheritdoc />
		public CsvInputFormatter([NotNull] CsvConfiguration configuration)
		{
			Configuration = configuration;

			SupportedEncodings.Add(UTF8EncodingWithoutBOM);
			SupportedEncodings.Add(UTF16EncodingLittleEndian);
			if (configuration.Encoding != null && !SupportedEncodings.Contains(configuration.Encoding)) SupportedEncodings.Add(configuration.Encoding);

			SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationCsv);
			SupportedMediaTypes.Add(MediaTypeHeaderValues.TextCsv);
			SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyCsvSyntax);
		}

		[NotNull]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CsvConfiguration Configuration { get; }

		/// <inheritdoc />
		public virtual InputFormatterExceptionPolicy ExceptionPolicy => GetType() == typeof (CsvInputFormatter) ? InputFormatterExceptionPolicy.MalformedInputExceptions : InputFormatterExceptionPolicy.AllExceptions;

		/// <inheritdoc />
		public override async Task<InputFormatterResult> ReadRequestBodyAsync([NotNull] InputFormatterContext context, [NotNull] Encoding encoding)
		{
			if (context == null) throw new ArgumentNullException(nameof (context));
			if (encoding == null) throw new ArgumentNullException(nameof (encoding));

			HttpRequest request = context.HttpContext.Request;
			
			if (!request.Body.CanSeek)
			{
				request.EnableBuffering();
				await request.Body.DrainAsync(context.HttpContext.RequestAborted);
				if (context.HttpContext.RequestAborted.IsCancellationRequested) return await InputFormatterResult.NoValueAsync();
				request.Body.Seek(0L, SeekOrigin.Begin);
			}

			using (TextReader reader = context.ReaderFactory(request.Body, encoding))
			{
				using (CsvReader csvReader = new CsvReader(reader, Configuration))
				{
					Type modelType = context.ModelType;

					try
					{
						await csvReader.ReadAsync();
						csvReader.ReadHeader();

						if (modelType.Implements<IEnumerable>())
						{
							modelType = modelType.ResolveGenericType();
							IEnumerable<object> models = csvReader.GetRecords(modelType);
							return models != null || context.TreatEmptyInputAsDefaultValue
										? await InputFormatterResult.SuccessAsync(models)
										: await InputFormatterResult.NoValueAsync();
						}

						object model = csvReader.GetRecord(modelType);
						return model != null || context.TreatEmptyInputAsDefaultValue
									? await InputFormatterResult.SuccessAsync(model)
									: await InputFormatterResult.NoValueAsync();
					}
					catch (Exception e)
					{
						ExceptionDispatchInfo.Capture(e).Throw();
						return await InputFormatterResult.FailureAsync();
					}
				}
			}
		}
	}
}