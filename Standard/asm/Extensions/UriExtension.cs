using System;
using System.Linq;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class UriExtension
	{
		[NotNull]
		public static Uri Join([NotNull] this Uri thisValue, [NotNull] params string[] path)
		{
			if (path.IsNullOrEmpty()) return thisValue;

			string baseUri = thisValue.ToString().TrimEnd(' ', '/');
			return new Uri(path.Select(UriHelper.Trim).SkipNullOrEmpty().Aggregate(baseUri, (u, p) => $"{u}/{p}"));
		}

		public static string RelativeUri([NotNull] this Uri thisValue)
		{
			return !thisValue.IsAbsoluteUri
						? thisValue.ToString()
						: thisValue.PathAndQuery;
		}
	}
}