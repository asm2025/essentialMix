using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class UriExtension
	{
		[NotNull]
		public static Uri Join([NotNull] this Uri thisValue, [NotNull] params string[] paths)
		{
			if (paths.Length == 0) return thisValue;

			StringBuilder sb = new StringBuilder(thisValue.ToString().TrimEnd(' ', '/'));

			foreach (string path in paths)
			{
				string p = UriHelper.Trim(path);
				if (string.IsNullOrEmpty(p)) continue;
				sb.Separator(Path.AltDirectorySeparatorChar).Append(p);
			}

			return new Uri(sb.ToString());
		}

		public static string RelativeUri([NotNull] this Uri thisValue)
		{
			return !thisValue.IsAbsoluteUri
						? thisValue.ToString()
						: thisValue.PathAndQuery;
		}
	}
}