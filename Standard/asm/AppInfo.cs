using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm
{
	public static class AppInfo
	{
		private static string __directory;

		[NotNull]
		public static string Directory
		{
			get
			{
				if (__directory == null)
				{
					try
					{
						Assembly assembly = AssemblyHelper.GetEntryAssembly();
						__directory = assembly?.GetDirectoryPath() ?? string.Empty;
					}
					catch
					{
						__directory = string.Empty;
					}
				}

				return __directory;
			}
		}
	}
}