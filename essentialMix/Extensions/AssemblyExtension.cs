using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class AssemblyExtension
{
	private const string VERSION_DEF = "0.0.0.0";

	private static readonly Regex __fileProtocolTrim = new Regex(@"\Afile:///?", RegexHelper.OPTIONS_I);

	[NotNull]
	public static string GetCode([NotNull] this Assembly thisValue)
	{
		return (thisValue.GetCustomAttribute<GuidAttribute>()?.Value ?? Guid.Empty.ToString("B")).ToUpperInvariant();
	}

	[NotNull]
	public static string GetTitle([NotNull] this Assembly thisValue)
	{
		string title = thisValue.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
		if (string.IsNullOrWhiteSpace(title)) title = thisValue.GetName().Name;
		if (string.IsNullOrWhiteSpace(title)) title = Path.GetFileNameWithoutExtension(GetPath(thisValue));
		return title;
	}

	[NotNull]
	public static string GetAssemblyName([NotNull] this Assembly thisValue)
	{
		return Path.GetFileNameWithoutExtension(GetPath(thisValue));
	}

	[NotNull]
	public static string GetDescription([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;
	}

	[NotNull]
	public static string GetProductName([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? string.Empty;
	}

	[NotNull]
	public static string GetCompany([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;
	}

	[NotNull]
	public static string GetCopyright([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
	}

	[NotNull]
	public static string GetTrademark([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark ?? string.Empty;
	}

	[NotNull]
	public static string GetVersion([NotNull] this Assembly thisValue)
	{
		AssemblyVersionAttribute assemblyVersion = thisValue.GetCustomAttribute<AssemblyVersionAttribute>();
		if (assemblyVersion != null) return assemblyVersion.Version;
		return Convert.ToString(thisValue.GetName().Version);
	}

	[NotNull]
	public static string GetFileVersion([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? VERSION_DEF;
	}

	[NotNull]
	public static string GetCulture([NotNull] this Assembly thisValue)
	{
		return thisValue.GetCustomAttribute<AssemblyCultureAttribute>()?.Culture ?? CultureInfo.CurrentCulture.Name;
	}

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

		string path = __fileProtocolTrim.Replace(codeBase, string.Empty)
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

		if (resourceNames.IsNullOrEmpty()) resourceNames = [string.Empty];

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

		if (resourceNames.IsNullOrEmpty()) resourceNames = [string.Empty];

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

		if (resourceNames.IsNullOrEmpty()) resourceNames = [string.Empty];

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
	public static string GetResourceName([NotNull] this Assembly thisValue, string resourceLocation, string resourceName)
	{
		return GetResourceName(thisValue, resourceLocation, resourceName, out _);
	}

	[NotNull]
	public static string GetResourceName([NotNull] this Assembly thisValue, string resourceLocation, string resourceName, out string fileName)
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