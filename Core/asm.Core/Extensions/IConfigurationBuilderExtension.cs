﻿using System;
using System.IO;
using System.Reflection;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace asm.Core.Extensions
{
	public static class IConfigurationBuilderExtension
	{
		[NotNull]
		public static IConfigurationBuilder Setup([NotNull] this IConfigurationBuilder thisValue, [NotNull] IHostEnvironment environment)
		{
			return Setup(thisValue, environment.ContentRootPath);
		}

		[NotNull]
		public static IConfigurationBuilder Setup([NotNull] this IConfigurationBuilder thisValue, string contentRoot = null)
		{
			contentRoot = PathHelper.Trim(contentRoot);
			if (string.IsNullOrEmpty(contentRoot) || !Directory.Exists(contentRoot)) contentRoot = Directory.GetCurrentDirectory();
			thisValue.SetBasePath(contentRoot);
			return thisValue;
		}

		public static IConfigurationBuilder AddConfigurationFiles([NotNull] this IConfigurationBuilder thisValue, [NotNull] IHostEnvironment environment)
		{
			return AddConfigurationFiles(thisValue, environment.ContentRootPath, environment.EnvironmentName);
		}

		public static IConfigurationBuilder AddConfigurationFiles([NotNull] this IConfigurationBuilder thisValue, string environmentName) { return AddConfigurationFiles(thisValue, null, environmentName); }
		[NotNull]
		public static IConfigurationBuilder AddConfigurationFiles([NotNull] this IConfigurationBuilder thisValue, string path, string environmentName)
		{
			return AddConfigurationFile(thisValue, path, "appsettings.json", false, environmentName);
		}

		[NotNull]
		public static IConfigurationBuilder AddConfigurationFile([NotNull] this IConfigurationBuilder thisValue, [NotNull] string fileName, bool optional, [NotNull] IHostEnvironment environment)
		{
			return AddConfigurationFile(thisValue, environment.ContentRootPath, fileName, optional, environment.EnvironmentName);
		}

		[NotNull] public static IConfigurationBuilder AddConfigurationFile([NotNull] this IConfigurationBuilder thisValue, [NotNull] string fileName, bool optional, string environmentName) { return AddConfigurationFile(thisValue, null, fileName, optional, environmentName); }
		[NotNull]
		public static IConfigurationBuilder AddConfigurationFile([NotNull] this IConfigurationBuilder thisValue, string path, [NotNull] string fileName, bool optional, string environmentName)
		{
			//https://jumpforjoysoftware.com/2018/09/aspnet-core-shared-settings/
			fileName = PathHelper.Trim(fileName) ?? throw new ArgumentNullException(nameof(fileName));
			environmentName = environmentName.ToNullIfEmpty() ?? Environments.Development;

			if (Path.IsPathFullyQualified(fileName))
			{
				string tmp = PathHelper.Trim(Path.GetDirectoryName(fileName));
				if (!string.IsNullOrEmpty(tmp)) path = tmp;
			}
			else
			{
				path = PathHelper.Trim(path);
			}

			if (string.IsNullOrEmpty(path)) path = Directory.GetCurrentDirectory();

			string fileBase = Path.GetFileNameWithoutExtension(fileName).ToNullIfEmpty() ?? string.Empty;
			string fileExtension = Path.GetExtension(fileName)?.Trim('.', ' ').ToLowerInvariant().ToNullIfEmpty() ?? string.Empty;
			Func<string, bool, bool, IConfigurationBuilder> addFunc;
			
			switch (fileExtension)
			{
				case "ini":
					addFunc = thisValue.AddIniFile;
					break;
				case "xml":
					addFunc = thisValue.AddXmlFile;
					break;
				default:
					addFunc = thisValue.AddJsonFile;
					break;
			}

			if (!string.IsNullOrEmpty(path))
			{
				string parentPath = Directory.GetParent(path)?.FullName;
				string sharedFile = Path.Combine(parentPath, $"{fileBase}.{fileExtension}");
				if (File.Exists(sharedFile)) addFunc(sharedFile, optional, true);
				sharedFile = Path.Combine(parentPath, $"{fileBase}.{environmentName}.{fileExtension}");
				if (File.Exists(sharedFile)) addFunc(sharedFile, true, true);
			}

			addFunc(Path.Combine(path, $"{fileBase}.{fileExtension}"), optional, true);
			addFunc(Path.Combine(path, $"{fileBase}.{environmentName}.{fileExtension}"), true, true);
			return thisValue;
		}

		[NotNull]
		public static IConfigurationBuilder AddUserSecrets([NotNull] this IConfigurationBuilder thisValue)
		{
			Assembly assembly = AssemblyHelper.GetEntryAssembly();
			if (assembly != null) thisValue.AddUserSecrets(assembly, true);
			return thisValue;
		}

		[NotNull]
		public static IConfigurationBuilder AddArguments([NotNull] this IConfigurationBuilder thisValue, string[] args)
		{
			if (!args.IsNullOrEmpty()) thisValue.AddCommandLine(args);
			return thisValue;
		}
	}
}
