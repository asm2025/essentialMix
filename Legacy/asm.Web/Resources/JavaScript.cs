using System;
using System.IO;
using System.Web.UI;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

[assembly: WebResource("asm.Web.Resources.cookies.js", "application/x-javascript")]
namespace asm.Web.Resources
{
	public static class JavaScript
	{
		private const string EXT = ".js";

		public static void Include([NotNull] ClientScriptManager manager, [NotNull] string name)
		{
			// this should be any file name from the folder Scripts. i.e. cookies.js
			name = PathHelper.Trim(name);
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (name.ContainsAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.PathSeparator, Path.VolumeSeparatorChar)) throw new InvalidOperationException("Invalid resource name.");
			name = name.Suffix(EXT, true);
			
			Type type = typeof(JavaScript);
			manager.RegisterClientScriptResource(type, $"{type.Namespace}.{name}");
		}
	}
}