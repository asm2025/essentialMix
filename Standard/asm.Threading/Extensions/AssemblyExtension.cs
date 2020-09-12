using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using asm.Extensions;
using asm.Helpers;
using asm.Threading.Helpers;
using JetBrains.Annotations;

namespace asm.Threading.Extensions
{
	public static class AssemblyExtension
	{
		public static string ExtractEmbeddedResource([NotNull] this Assembly thisValue, string resourceName, bool skipExisting)
		{
			return ExtractEmbeddedResource(thisValue, null, null, resourceName, skipExisting);
		}

		public static string ExtractEmbeddedResource([NotNull] this Assembly thisValue, string resourceLocation, string resourceName, bool skipExisting)
		{
			return ExtractEmbeddedResource(thisValue, null, resourceLocation, resourceName, skipExisting);
		}

		/// <summary>
		/// Extracts the embedded resource.
		/// </summary>
		/// <param name="thisValue">Assembly should be a value returned by Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), typeof(x).Assembly etc.</param>
		/// <param name="directoryPath">The destination path.</param>
		/// <param name="resourceLocation">The resource location. This would be Namespace (+ Dir Names) like [Namespace].Properties.Resources or [Namespace].Properties.Resources.[MyResource]</param>
		/// <param name="skipExisting">Will skip extracting resources that exist on file system.</param>
		/// <param name="resourceName">Name of the case-sensitive resource name(s) to extract.</param>
		/// <returns>string</returns>
		[SecuritySafeCritical]
		public static string ExtractEmbeddedResource([NotNull] this Assembly thisValue, string directoryPath, string resourceLocation, string resourceName, bool skipExisting)
		{
			return ExtractEmbeddedResources(thisValue, directoryPath, resourceLocation, skipExisting, resourceName).FirstOrDefault();
		}

		public static IEnumerable<string> ExtractEmbeddedResources([NotNull] this Assembly thisValue, bool skipExisting, [NotNull] params string[] resourceNames)
		{
			return ExtractEmbeddedResources(thisValue, null, null, skipExisting, resourceNames);
		}

		public static IEnumerable<string> ExtractEmbeddedResources([NotNull] this Assembly thisValue, string resourceLocation, bool skipExisting, [NotNull] params string[] resourceNames)
		{
			return ExtractEmbeddedResources(thisValue, null, resourceLocation, skipExisting, resourceNames);
		}

		/// <summary>
		/// Extracts the embedded resource.
		/// </summary>
		/// <param name="thisValue">Assembly should be a value returned by Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), typeof(x).Assembly etc.</param>
		/// <param name="directoryPath">The destination path.</param>
		/// <param name="resourceLocation">The resource location. This would be Namespace (+ Dir Names) like [Namespace].Properties.Resources or [Namespace].Properties.Resources.[MyResource]</param>
		/// <param name="skipExisting">Will skip extracting resources that exist on file system.</param>
		/// <param name="resourceNames">Names of the case-sensitive resource name(s) to extract.</param>
		/// <returns>IEnumerable&lt;string&gt;</returns>
		[SecuritySafeCritical]
		[ItemNotNull]
		public static IEnumerable<string> ExtractEmbeddedResources([NotNull] this Assembly thisValue, string directoryPath, string resourceLocation, bool skipExisting, params string[] resourceNames)
		{
			directoryPath = PathHelper.Trim(directoryPath);
			if (string.IsNullOrEmpty(directoryPath)) directoryPath = Directory.GetCurrentDirectory();

			if (resourceNames.Count(string.IsNullOrWhiteSpace) > 1) throw new ArgumentException("More than one empty resource name encountered. This will save the same resource repeatedly.", nameof(resourceNames));

			resourceLocation = resourceLocation?.Trim();
			if (string.IsNullOrEmpty(resourceLocation)) resourceLocation = thisValue.GetName().Name;

			directoryPath = PathHelper.AddDirectorySeparator(directoryPath);
			if (!DirectoryHelper.Ensure(directoryPath)) throw new IOException($"Cannot access or create directory '{directoryPath}'");

			if (resourceNames.IsNullOrEmpty()) resourceNames = new[]{ string.Empty };

			foreach (string resourceName in resourceNames)
			{
				string streamName = thisValue.GetResourceName(resourceLocation, resourceName, out string fileName);
				string fullPath = directoryPath + fileName;
				if (skipExisting && File.Exists(fullPath)) continue;

				using (Stream stream = thisValue.GetManifestResourceStream(streamName))
				{
					if (stream == null) continue;

					using (FileStream file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
						stream.CopyTo(file);

					yield return fullPath;
				}
			}
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, string resourceName)
		{
			return ExtractAndRunResource(thisValue, null, null, null, resourceName);
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, string directoryPath, string resourceName)
		{
			return ExtractAndRunResource(thisValue, null, directoryPath, null, resourceName);
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, string directoryPath, string resourceLocation, string resourceName)
		{
			return ExtractAndRunResource(thisValue, null, directoryPath, resourceLocation, resourceName);
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, RunSettings settings, string resourceName)
		{
			return ExtractAndRunResource(thisValue, settings, null, null, resourceName);
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, RunSettings settings, string resourceLocation, string resourceName)
		{
			return ExtractAndRunResource(thisValue, settings, null, resourceLocation, resourceName);
		}

		public static Process ExtractAndRunResource([NotNull] this Assembly thisValue, RunSettings settings, string directoryPath, string resourceLocation, string resourceName)
		{
			return ExtractAndRunResources(thisValue, settings, directoryPath, resourceLocation, resourceName).FirstOrDefault();
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, [NotNull] params string[] resourceNames)
		{
			return ExtractAndRunResources(thisValue, null, null, null, resourceNames);
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, string resourceLocation, [NotNull] params string[] resourceNames)
		{
			return ExtractAndRunResources(thisValue, null, null, resourceLocation, resourceNames);
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, string directoryPath, string resourceLocation, [NotNull] params string[] resourceNames)
		{
			return ExtractAndRunResources(thisValue, null, directoryPath, resourceLocation, resourceNames);
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, RunSettings settings, [NotNull] params string[] resourceNames)
		{
			return ExtractAndRunResources(thisValue, settings, null, null, resourceNames);
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, RunSettings settings, string resourceLocation, [NotNull] params string[] resourceNames)
		{
			return ExtractAndRunResources(thisValue, settings, null, resourceLocation, resourceNames);
		}

		public static IEnumerable<Process> ExtractAndRunResources([NotNull] this Assembly thisValue, RunSettings settings, string directoryPath, string resourceLocation, [NotNull] params string[] resourceNames)
		{
			settings ??= RunSettings.Default;

			foreach (string resource in ExtractEmbeddedResources(thisValue, directoryPath, resourceLocation, true, resourceNames))
			{
				yield return ProcessHelper.Run(resource, null, settings);
			}
		}
	}
}