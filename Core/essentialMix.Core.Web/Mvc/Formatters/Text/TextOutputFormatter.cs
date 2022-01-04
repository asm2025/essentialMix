using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Formatters;
using essentialMix.Core.Web.Mvc.Formatters.Text.Internal;

namespace essentialMix.Core.Web.Mvc.Formatters.Text;

public class TextOutputFormatter : global::Microsoft.AspNetCore.Mvc.Formatters.TextOutputFormatter
{
	/// <inheritdoc />
	public TextOutputFormatter()
	{
		SupportedEncodings.Add(Encoding.UTF8);
		SupportedEncodings.Add(Encoding.Unicode);

		SupportedMediaTypes.Add(MediaTypeHeaderValues.TextPlain);
	}

	/// <inheritdoc />
	[NotNull]
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
		switch (value)
		{
			case null:
				return;
			case IEnumerable enumerable:
			{
				foreach (object obj in enumerable)
				{
					writer.WriteLine(obj);
				}

				break;
			}
			default:
				writer.WriteLine(value);
				break;
		}

		writer.Write(value);
	}

	public void Write<T>([NotNull] TextWriter writer, T value)
	{
		switch (value)
		{
			case IEnumerable enumerable:
			{
				foreach (object obj in enumerable)
				{
					writer.WriteLine(obj);
				}

				break;
			}
			default:
				writer.WriteLine(value);
				break;
		}
	}
}