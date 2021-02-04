using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm
{
	public static class AppInfo
	{
		private static readonly Lazy<string> __executablePath = new Lazy<string>(() => AssemblyHelper.GetEntryAssembly().GetPath(), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<string> __appGuid = new Lazy<string>(() => AssemblyHelper.GetEntryAssembly().GetAttribute<GuidAttribute>().Value.ToUpperInvariant(), LazyThreadSafetyMode.PublicationOnly);

		private static string __directory;

		[NotNull]
		public static string ExecutablePath => __executablePath.Value;

		[NotNull]
		public static string Directory => __directory ??= PathHelper.AddDirectorySeparator(Path.GetDirectoryName(ExecutablePath));

		[NotNull]
		public static string AppGuid => __appGuid.Value;
	}
}