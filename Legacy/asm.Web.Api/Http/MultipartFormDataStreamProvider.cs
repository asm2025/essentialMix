using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Web.Api.Http
{
	public class MultipartFormDataStreamProvider : MultipartStreamProvider
	{
		// Set of indexes of which HttpContents we designate as form data  
		private readonly Collection<bool> _isFormData = new Collection<bool>();

		/// <inheritdoc />
		public MultipartFormDataStreamProvider()
		{
		}

		[NotNull]
		public override Stream GetStream(HttpContent parent, [NotNull] HttpContentHeaders headers)
		{
			// For form data, Content-Disposition header is a requirement  
			ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;

			if (contentDisposition != null)
			{
				// We will post process this as form data  
				_isFormData.Add(string.IsNullOrEmpty(contentDisposition.FileName));
				return new MemoryStream();
			}

			// If no Content-Disposition header was present.  
			throw new InvalidOperationException("Did not find required \'Content-Disposition\' header field in MIME multipart body part.");
		}

		/// <inheritdoc />
		/// <summary>
		/// Read the non-file contents as form data.
		/// </summary>
		/// <returns></returns>
		public override async Task ExecutePostProcessingAsync()
		{
			// Find instances of non-file HttpContents and read them asynchronously  
			// to get the string content and then add that as form data  
			for (int index = 0; index < Contents.Count; index++)
			{
				if (_isFormData[index])
				{
					HttpContent formContent = Contents[index];
					// Extract name from Content-Disposition header. We know from earlier that the header is present.  
					ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
					string formFieldName = UnquoteToken(contentDisposition.Name) ?? string.Empty;

					// Read the contents as string data and add to form data  
					string formFieldValue = await formContent.ReadAsStringAsync();
					FormData.Add(formFieldName, formFieldValue);
				}
				else
				{
					Files.Add(Contents[index]);
				}
			}
		}

		/// <summary>
		/// Gets a <see cref="NameValueCollection" /> of form data passed as part of the multipart form data.
		/// </summary>
		public NameValueCollection FormData { get; } = new NameValueCollection();

		/// <summary>
		/// Gets list of <see cref="HttpContent" />s which contain uploaded files as in-memory representation.
		/// </summary>
		public List<HttpContent> Files { get; } = new List<HttpContent>();

		/// <summary>
		/// Remove bounding quotes on a token if present
		/// </summary>
		/// <param name="token">Token to unquote.</param>
		/// <returns>Unquoted token.</returns>
		private static string UnquoteToken(string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return token;

			if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
				return token.Substring(1, token.Length - 2);

			return token;
		}
	}
}