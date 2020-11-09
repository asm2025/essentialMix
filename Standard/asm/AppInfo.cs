using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm
{
	public static class AppInfo
	{
		private static readonly Lazy<string> __executablePath = new Lazy<string>(() =>
		{
			Assembly assembly = AssemblyHelper.GetEntryAssembly();
			return assembly == null
						? string.Empty
						: assembly.GetPath();
		}, LazyThreadSafetyMode.PublicationOnly);

		private static readonly Lazy<string> __appGuid = new Lazy<string>(() =>
		{
			Assembly assembly = AssemblyHelper.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
			GuidAttribute guidAttribute = assembly.GetAttribute<GuidAttribute>();
			return guidAttribute == null
						? Guid.Empty.ToString()
						: guidAttribute.Value;
		}, LazyThreadSafetyMode.PublicationOnly);

		private static string __directory;

		[NotNull]
		public static string ExecutablePath => __executablePath.Value;

		[NotNull]
		public static string Directory => __directory ??= PathHelper.AddDirectorySeparator(Path.GetDirectoryName(ExecutablePath));

		[NotNull]
		public static string AppGuid => __appGuid.Value;
	}
}