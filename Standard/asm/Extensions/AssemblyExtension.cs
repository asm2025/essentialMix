using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Threading;

namespace asm.Extensions
{
	public static class AssemblyExtension
	{
		private static readonly Regex __asmFileProtocolTrim = new Regex(@"\Afile:///?", RegexHelper.OPTIONS_I);

		[NotNull]
		public static string GetPath([NotNull] this Assembly thisValue)
		{
			string codeBase = thisValue.CodeBase; // "pseudo" because it is not properly escaped
			
			if (string.IsNullOrEmpty(codeBase))
			{
				return string.IsNullOrEmpty(thisValue.Location)
							? string.Empty
							: Path.GetFullPath(thisValue.Location);
			}

			string path = __asmFileProtocolTrim.Replace(codeBase, string.Empty)
												.Replace('/', '\\');
			return Path.GetFullPath(path);
		}

		public static string GetDirectoryPath([NotNull] this Assembly thisValue)
		{
			string path = GetPath(thisValue);
			return string.IsNullOrEmpty(path) 
				? null 
				: PathHelper.AddDirectorySeparator(Path.GetDirectoryName(path));
		}

		public static System.Configuration.Configuration GetConfiguration([NotNull] this Assembly thisValue)
		{
			string path;

			try
			{
				path = GetPath(thisValue);
			}
			catch
			{
				path = AppDomain.CurrentDomain.BaseDirectory;
			}

			return string.IsNullOrEmpty(path) ? null : ConfigurationManager.OpenExeConfiguration(path);
		}

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
				string streamName = GetResourceName(thisValue, resourceLocation, resourceName, out string fileName);
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

		public static Assembly LoadAssemblyFromResource([NotNull] this Assembly thisValue, string resourceLocation, string resourceName, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_STATIC, Binder binder = null, CultureInfo culture = null)
		{
			return LoadAssemblyFromResource(thisValue, resourceLocation, resourceName, null, bindingFlags, binder, culture);
		}

		[SecuritySafeCritical]
		public static Assembly LoadAssemblyFromResource([NotNull] this Assembly thisValue, string resourceLocation, string resourceName, object[] parameters, BindingFlags bindingFlags = Constants.BF_PUBLIC_NON_PUBLIC_STATIC, Binder binder = null, CultureInfo culture = null)
		{
			resourceName = resourceName?.Trim(' ', '.');
			if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException(nameof(resourceName));
			resourceLocation = resourceLocation?.Trim();

			string streamName = GetResourceName(thisValue, resourceLocation, resourceName);
			byte[] buffer;

			using (Stream stream = thisValue.GetManifestResourceStream(streamName))
			{
				if (stream == null) return null;

				buffer = new byte[stream.Length];
				stream.Read(buffer);
			}

			Assembly assembly = Assembly.Load(buffer);
			assembly.EntryPoint?.Invoke(null, bindingFlags, binder, parameters, culture ?? CultureInfoHelper.Default);
			return assembly;
		}

		[SecuritySafeCritical]
		public static Stream GetEmbeddedResource([NotNull] this Assembly thisValue, string resourceLocation, string resourceName)
		{
			return GetEmbeddedResources(thisValue, resourceLocation, resourceName).FirstOrDefault();
		}

		/// <summary>
		/// Get stream(s) of embedded resource.
		/// </summary>
		/// <param name="thisValue">Assembly should be a value returned by Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), typeof(x).Assembly etc.</param>
		/// <param name="resourceLocation">The resource location. This would be Namespace (+ Dir Names) like [Namespace].Properties.Resources or [Namespace].Properties.Resources.[MyResource]</param>
		/// <param name="resourceNames">Names of the case-sensitive resource name(s) to extract.</param>
		/// <returns>IEnumerable&lt;string&gt;</returns>
		[SecuritySafeCritical]
		[ItemNotNull]
		public static IEnumerable<Stream> GetEmbeddedResources([NotNull] this Assembly thisValue, string resourceLocation, params string[] resourceNames)
		{
			if (resourceNames.Count(string.IsNullOrWhiteSpace) > 1) throw new ArgumentException("More than one empty resource name encountered. This will save the same resource repeatedly.", nameof(resourceNames));

			resourceLocation = resourceLocation?.Trim();
			if (string.IsNullOrEmpty(resourceLocation)) resourceLocation = thisValue.GetName().Name;

			if (resourceNames.IsNullOrEmpty()) resourceNames = new[] { string.Empty };

			foreach (string resourceName in resourceNames)
			{
				string streamName = GetResourceName(thisValue, resourceLocation, resourceName);
				Stream stream = null;

				try
				{
					stream = thisValue.GetManifestResourceStream(streamName);
					if (stream == null) continue;
					yield return stream;
				}
				finally
				{
					ObjectHelper.Dispose(ref stream);
				}
			}
		}

		public static (string FileName, Stream Stream) GetEmbeddedFile([NotNull] this Assembly thisValue, string resourceLocation, string resourceName)
		{
			return GetEmbeddedFiles(thisValue, resourceLocation, resourceName).FirstOrDefault();
		}

		/// <summary>
		/// Get stream(s) of embedded resource with their file names.
		/// </summary>
		/// <param name="thisValue">Assembly should be a value returned by Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly(), typeof(x).Assembly etc.</param>
		/// <param name="resourceLocation">The resource location. This would be Namespace (+ Dir Names) like [Namespace].Properties.Resources or [Namespace].Properties.Resources.[MyResource]</param>
		/// <param name="resourceNames">Names of the case-sensitive resource name(s) to extract.</param>
		/// <returns>IEnumerable&lt;string&gt;</returns>
		[SecuritySafeCritical]
		public static IEnumerable<(string FileName, Stream Stream)> GetEmbeddedFiles([NotNull] this Assembly thisValue, string resourceLocation, params string[] resourceNames)
		{
			if (resourceNames.Count(string.IsNullOrWhiteSpace) > 1) throw new ArgumentException("More than one empty resource name encountered. This will save the same resource repeatedly.", nameof(resourceNames));

			resourceLocation = resourceLocation?.Trim();
			if (string.IsNullOrEmpty(resourceLocation)) resourceLocation = thisValue.GetName().Name;

			if (resourceNames.IsNullOrEmpty()) resourceNames = new[] { string.Empty };

			foreach (string resourceName in resourceNames)
			{
				string streamName = GetResourceName(thisValue, resourceLocation, resourceName, out string fileName);
				Stream stream = null;

				try
				{
					stream = thisValue.GetManifestResourceStream(streamName);
					if (stream == null) continue;
					yield return (fileName, stream);
				}
				finally
				{
					ObjectHelper.Dispose(ref stream);
				}
			}
		}

		[NotNull]
		public static IEnumerable<MethodBase> GetEntryPoint([NotNull] this Assembly thisValue, Predicate<Type> typeFilter = null, Predicate<MethodBase> methodFilter = null)
		{
			return thisValue.GetTypes()
				.Where(type => typeFilter == null || typeFilter(type))
				.SelectMany(type => type.GetEntryPoint(methodFilter));
		}

		[NotNull]
		public static string GetResourceName([NotNull] Assembly thisValue, string resourceLocation, string resourceName)
		{
			return GetResourceName(thisValue, resourceLocation, resourceName, out _);
		}

		[NotNull]
		public static string GetResourceName([NotNull] Assembly thisValue, string resourceLocation, string resourceName, out string fileName)
		{
			/*
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			 * | resourceLocation | resourceName | streamName                      | fileName                          |
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			 * | empty            | empty        | empty                           | thisValue.Name (only one allowed) |
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			 * | empty            | not empty    | resourceName                    | resourceName                      |
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			 * | not empty        | empty        | resourceLocation                | resourceLocation                  |
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			 * | not empty        | not empty    | resourceLocation . resourceName | resourceName                      |
			 * +------------------+--------------+---------------------------------+-----------------------------------+
			*/
			string streamName;

			if (string.IsNullOrEmpty(resourceLocation))
			{
				if (string.IsNullOrEmpty(resourceName))
				{
					streamName = string.Empty;
					fileName = thisValue.GetName().Name;
				}
				else
				{
					streamName = resourceName;
					fileName = $"{thisValue.GetName().Name}.{resourceName}";
				}
			}
			else
			{
				if (string.IsNullOrEmpty(resourceName))
				{
					streamName = fileName = resourceLocation;
				}
				else
				{
					streamName = $"{resourceLocation}.{resourceName}";
					fileName = resourceName;
				}
			}

			return streamName;
		}
	}
}