using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using asm.Core.Web.Annotations;
using asm.Extensions;
using asm.Core.Web.Helpers;
using asm.Core.Web.Mvc.Filters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace asm.Core.Web.Controllers
{
	// https://github.com/aspnet/AspNetCore.Docs/blob/master/aspnetcore/mvc/models/file-uploads/sample/FileUploadSample/Controllers/StreamingController.cs
	public abstract class StreamingController : MvcController
	{
		/// <inheritdoc />
		protected StreamingController(IConfiguration configuration, ILogger logger)
			: base(configuration, logger)
		{
		}

		[HttpGet]
		[AntiForgeryToken]
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[DisableFormValueModelBinding]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Upload()
		{
			if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType)) return BadRequest($"Expected a multipart request, got {Request.ContentType}");

			// Used to accumulate all the form url encoded key value pairs in the 
			// request.
			string targetFilePath = null;
			string boundary = MediaTypeHeaderValue.Parse(Request.ContentType).GetBoundary(FormOptions.MultipartBoundaryLengthLimit);
			KeyValueAccumulator formAccumulator = new KeyValueAccumulator();
			MultipartReader reader = new MultipartReader(boundary, HttpContext.Request.Body);
			MultipartSection section = await reader.ReadNextSectionAsync();

			while (section != null)
			{
				bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

				if (hasContentDispositionHeader)
				{
					if (contentDisposition.HasFileContentDisposition())
					{
						targetFilePath = Path.GetTempFileName();

						await using (FileStream targetStream = System.IO.File.Create(targetFilePath))
						{
							await section.Body.CopyToAsync(targetStream);
						}
					}
					else if (contentDisposition.HasFileContentDisposition())
					{
						// Content-Disposition: form-data; name="key"
						//
						// value

						// Do not limit the key name length here because the 
						// multipart headers length limit is already in effect.
						StringSegment key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
						Encoding encoding = GetEncoding(section);

						using (StreamReader streamReader = new StreamReader(section.Body, encoding, true, Constants.BUFFER_KB, true))
						{
							// The value length limit is enforced by MultipartBodyLengthLimit
							string value = await streamReader.ReadToEndAsync();
							if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase)) value = string.Empty;
							formAccumulator.Append(key.Value, value);
							if (formAccumulator.ValueCount > FormOptions.ValueCountLimit) throw new InvalidDataException($"Form key count limit {FormOptions.ValueCountLimit} exceeded.");
						}
					}
				}

				// Drains any remaining section body that has not been consumed and
				// reads the headers for the next section.
				section = await reader.ReadNextSectionAsync();
			}

			if (string.IsNullOrWhiteSpace(targetFilePath)) return BadRequest("File could not be read or content disposition header is invalid.");

			FormValueProvider formValueProvider = new FormValueProvider(BindingSource.Form, new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);
			return await FileUploaded(targetFilePath, formValueProvider);
		}

		protected abstract Task<IActionResult> FileUploaded([NotNull] string targetFilePath, [NotNull] FormValueProvider provider);
		
		private static Encoding GetEncoding([NotNull] MultipartSection section)
		{
			bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue mediaType);
			// UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
			// most cases.
			return !hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType.Encoding)
						? Encoding.UTF8
						: mediaType.Encoding;
		}
	}
}