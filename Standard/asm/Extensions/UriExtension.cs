using System;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class UriExtension
	{
		public static Uri Append(this Uri thisValue, string path)
		{
			return UriHelper.Combine(thisValue, path);
		}

		public static Uri Append(this Uri thisValue, Uri path)
		{
			return UriHelper.Combine(thisValue, path);
		}

		public static Uri Append(this Uri thisValue, [NotNull] params string[] paths)
		{
			return UriHelper.Combine(thisValue, paths);
		}

		public static string String(this Uri thisValue)
		{
			return thisValue == null
						? null
						: thisValue.IsAbsoluteUri
							? thisValue.AbsoluteUri
							: thisValue.PathAndQuery;
		}

		public static string RelativeUri(this Uri thisValue)
		{
			return thisValue == null
						? null
						: !thisValue.IsAbsoluteUri
							? thisValue.ToString()
							: thisValue.PathAndQuery;
		}
	}
}