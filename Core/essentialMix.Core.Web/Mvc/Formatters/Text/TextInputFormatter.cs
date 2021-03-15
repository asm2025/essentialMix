using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using essentialMix.Core.Web.Mvc.Formatters.Text.Internal;
using JetBrains.Annotations;

namespace essentialMix.Core.Web.Mvc.Formatters.Text
{
	public class TextInputFormatter : global::Microsoft.AspNetCore.Mvc.Formatters.TextInputFormatter, IInputFormatterExceptionPolicy
	{
		/// <inheritdoc />
		public TextInputFormatter()
		{
			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);

			SupportedMediaTypes.Add(MediaTypeHeaderValues.TextPlain);
		}

		/// <inheritdoc />
		public virtual InputFormatterExceptionPolicy ExceptionPolicy => GetType() == typeof (TextInputFormatter) ? InputFormatterExceptionPolicy.MalformedInputExceptions : InputFormatterExceptionPolicy.AllExceptions;

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
				try
				{
					object model = await reader.ReadToEndAsync();
					return await InputFormatterResult.SuccessAsync(model);
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