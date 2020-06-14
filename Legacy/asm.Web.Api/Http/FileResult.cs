using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using JetBrains.Annotations;
using asm.Helpers;
using asmHeaderNames = asm.Web.HeaderNames;

namespace asm.Web.Api.Http
{
	public class FileResult : PushStreamResult
	{
		/// <inheritdoc />
		public FileResult([NotNull] HttpRequestMessage request, [NotNull] Uri destination)
			: this(request, destination, null, null)
		{
		}

		/// <inheritdoc />
		public FileResult([NotNull] HttpRequestMessage request, [NotNull] Uri destination, Action<Exception, Stream, HttpContent, TransportContext> onError)
			: this(request, destination, null, onError)
		{
		}

		/// <inheritdoc />
		public FileResult([NotNull] HttpRequestMessage request, [NotNull] Uri destination, Action onCompleted, Action<Exception, Stream, HttpContent, TransportContext> onError) 
			: base(request, onCompleted, onError)
		{
			Destination = destination;
		}

		[NotNull]
		public Uri Destination { get; set; }
		public bool Overwrite { get; set; }
		public bool Rename { get; set; }

		/// <inheritdoc />
		public override Task<HttpResponseMessage> ExecuteAsync(CancellationToken token)
		{
			if (!Request.Content.IsMimeMultipartContent()) throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			return base.ExecuteAsync(token);
		}

		/// <inheritdoc />
		protected override async Task WriteContent(HttpResponseMessage response, Stream stream, HttpContent content, TransportContext context, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested) return;

			MultipartFormDataStreamProvider provider = await Request.Content.ReadAsMultipartAsync(new MultipartFormDataStreamProvider(), token);
			//access files  
			IList<HttpContent> files = provider.Files;
			if (files.Count == 0) return;

			HttpContent file = files[0];
			string uploadedFileName = file.Headers.ContentDisposition.FileName.Trim('\"', ' ');
			if (PathHelper.IsPathQualified(uploadedFileName)) uploadedFileName = Path.GetFileName(uploadedFileName);

			string path = HttpRuntime.AppDomainAppPath;
			string destination = UriHelper.LocalPath(Destination.ToString());
			if (!string.IsNullOrEmpty(destination) && destination != "\\") path = Path.Combine(path, destination);

			string fileNameBase = Path.GetFileNameWithoutExtension(uploadedFileName);
			string extension = Path.GetExtension(uploadedFileName);
			string destinationFileName = Path.Combine(path, uploadedFileName);

			if (Directory.Exists(destinationFileName))
			{
				if (!Rename) throw new HttpException("A directory with the same name already exists.");

				int n = 1;

				do
				{
					destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
				}
				while (File.Exists(destinationFileName)); // File.Exists tests both file and directory
			}

			if (File.Exists(destinationFileName))
			{
				if (Overwrite) File.Delete(destinationFileName);
				if (!Rename) throw new HttpException("A file with the same name already exists.");

				int n = 1;

				do
				{
					destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
				}
				while (File.Exists(destinationFileName)); // File.Exists tests both file and directory
			}

			using (Stream input = await file.ReadAsStreamAsync())
			{
				using (Stream output = File.OpenWrite(destinationFileName))
				{
					await input.CopyToAsync(output);
				}
			}

			Uri fileUrl = UriHelper.Combine(Request.RequestUri, Destination.ToString().TrimEnd('/') + '/' + Path.GetFileName(destinationFileName));
			response.Headers.Add(asmHeaderNames.UploadedFile, fileUrl.ToString());
		}
	}
}
