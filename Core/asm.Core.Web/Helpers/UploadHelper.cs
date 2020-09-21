using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using asm.Extensions;
using asm.Core.Web.Model;
using asm.Exceptions.Web;
using asm.Helpers;
using Microsoft.Extensions.Hosting;

namespace asm.Core.Web.Helpers
{
	public static class UploadHelper
	{
		public static async Task<string[]> UploadMultipartAsync([NotNull] HttpRequest request, [NotNull] UploadSettings uploadSettings)
		{
			if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType)) throw new ArgumentException("Request is not a multipart type.");

			CancellationToken token = request.HttpContext.RequestAborted;
			if (token.IsCancellationRequested) return null;
			
			string path = PathHelper.AddDirectorySeparator(uploadSettings.TargetPath);

			if (string.IsNullOrEmpty(path))
			{
				IHostEnvironment env = request.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
				path = PathHelper.AddDirectorySeparator(Path.Combine(env.ContentRootPath, "Uploads"));
			}

			if (!DirectoryHelper.Ensure(path)) throw new DirectoryNotFoundException();

			string boundary = MediaTypeHeaderValue.Parse(request.ContentType)?.GetBoundary();
			if (string.IsNullOrEmpty(boundary)) throw new InvalidDataException("Could not get request boundary.");

			IList<string> uploadedFiles = new List<string>();
			MultipartReader reader = new MultipartReader(boundary, request.Body);
			MultipartSection section = await reader.ReadNextSectionAsync(token);

			while (!token.IsCancellationRequested && section != null)
			{
				bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

				if (hasContentDispositionHeader)
				{
					if (contentDisposition.HasFileContentDisposition())
					{
						StringSegment fileNameSegment = contentDisposition.FileNameStar.Trim();
						if (StringSegment.IsNullOrEmpty(fileNameSegment)) fileNameSegment = contentDisposition.FileName.Trim();
						if (StringSegment.IsNullOrEmpty(fileNameSegment)) continue;

						string fileName = WebUtility.UrlDecode(fileNameSegment.Value)?.Trim('\"', ' ');
						if (string.IsNullOrEmpty(fileName)) continue;
						if (Path.IsPathFullyQualified(fileName)) fileName = Path.GetFileName(fileName);
						
						string fileNameBase = Path.GetFileNameWithoutExtension(fileName);
						string extension = Path.GetExtension(fileName);
						string destinationFileName = Path.Combine(path, fileName);

						if (Directory.Exists(destinationFileName))
						{
							if (!uploadSettings.Rename) throw new HttpException("A directory with the same name already exists.");

							int n = 1;

							do
							{
								destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
							}
							while (File.Exists(destinationFileName)); // File.Exists tests both file and directory
						}

						if (File.Exists(destinationFileName))
						{
							if (uploadSettings.Overwrite) File.Delete(destinationFileName);
							if (!uploadSettings.Rename) throw new HttpException("A file with the same name already exists.");

							int n = 1;

							do
							{
								destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
							}
							while (File.Exists(destinationFileName)); // File.Exists tests both file and directory
						}

						await using (FileStream targetStream = File.Create(destinationFileName))
						{
							await section.Body.CopyToAsync(targetStream, token);
						}

						if (token.IsCancellationRequested) return null;
						uploadedFiles.Add(destinationFileName);
					}
				}

				// Drain any remaining section body that has not been consumed and read the headers for the next section.
				section = await reader.ReadNextSectionAsync(token);
			}

			return token.IsCancellationRequested
						? null
						: uploadedFiles.ToArray();
		}

		public static async Task<string[]> UploadAsync([NotNull] IReadOnlyCollection<IFormFile> files, [NotNull] UploadSettings uploadSettings, CancellationToken token = default(CancellationToken))
		{
			if (token.IsCancellationRequested) return null;
			if (files.Count == 0) return Array.Empty<string>();
			
			string path = PathHelper.AddDirectorySeparator(uploadSettings.TargetPath);
			if (string.IsNullOrEmpty(path)) throw new ArgumentException($"{nameof(UploadSettings.TargetPath)} cannot be empty.");
			if (!DirectoryHelper.Ensure(path)) throw new DirectoryNotFoundException();
			
			IList<string> uploadedFiles = new List<string>();

			foreach (IFormFile file in files.TakeWhile(e => !token.IsCancellationRequested))
			{
				string fileName = WebUtility.UrlDecode(file.FileName)?.Trim('\"', ' ');
				if (string.IsNullOrEmpty(fileName)) continue;
				if (Path.IsPathFullyQualified(fileName)) fileName = Path.GetFileName(fileName);
						
				string fileNameBase = Path.GetFileNameWithoutExtension(fileName);
				string extension = Path.GetExtension(fileName);
				string destinationFileName = Path.Combine(path, fileName);

				if (Directory.Exists(destinationFileName))
				{
					if (!uploadSettings.Rename) throw new HttpException("A directory with the same name already exists.");

					int n = 1;

					do
					{
						destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
					}
					while (!token.IsCancellationRequested && File.Exists(destinationFileName)); // File.Exists tests both file and directory
				}

				if (File.Exists(destinationFileName))
				{
					if (uploadSettings.Overwrite) File.Delete(destinationFileName);
					if (!uploadSettings.Rename) throw new HttpException("A file with the same name already exists.");

					int n = 1;

					do
					{
						destinationFileName = Path.Combine(path, $"{fileNameBase} ({n++}){extension}");
					}
					while (!token.IsCancellationRequested && File.Exists(destinationFileName)); // File.Exists tests both file and directory
				}

				await using (Stream sourceStream = file.OpenReadStream())
				{
					await using (FileStream targetStream = File.Create(destinationFileName))
					{
						await sourceStream.CopyToAsync(targetStream, token);
					}
				}

				if (token.IsCancellationRequested) return null;
				uploadedFiles.Add(destinationFileName);
			}

			return token.IsCancellationRequested
						? null
						: uploadedFiles.ToArray();
		}
	}
}