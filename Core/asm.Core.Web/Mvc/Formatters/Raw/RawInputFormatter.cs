using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using asm.Core.Web.Mvc.Formatters.Raw.Internal;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Core.Web.Mvc.Formatters.Raw
{
	public class RawInputFormatter : InputFormatter
	{
		/// <inheritdoc />
		public RawInputFormatter() 
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValues.TextPlain);
			SupportedMediaTypes.Add(MediaTypeHeaderValues.OctetStream);
		}

		/// <inheritdoc />
		public override bool CanRead([NotNull] InputFormatterContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			string contentType = context.HttpContext.Request.ContentType.ToNullIfEmpty();
			return string.IsNullOrEmpty(contentType) || SupportedMediaTypes.Any(e => e.IsSame(contentType));
		}

		/// <inheritdoc />
		public override async Task<InputFormatterResult> ReadRequestBodyAsync([NotNull] InputFormatterContext context)
		{
			if (CanRead(context))
			{
				HttpRequest request = context.HttpContext.Request;
				string contentType = context.HttpContext.Request.ContentType.ToNullIfEmpty();

				if (string.IsNullOrEmpty(contentType) || contentType.IsSame("text/plain"))
				{
					using (StreamReader reader = new StreamReader(request.Body))
					{
						string content = await reader.ReadToEndAsync();
						return await InputFormatterResult.SuccessAsync(content);
					}
				}

				await using (MemoryStream ms = new MemoryStream(Constants.GetBufferKB(2)))
				{
					await request.Body.CopyToAsync(ms);
					byte[] content = ms.ToArray();
					return await InputFormatterResult.SuccessAsync(content);
				}
			}

			return await InputFormatterResult.FailureAsync();		}
	}
}