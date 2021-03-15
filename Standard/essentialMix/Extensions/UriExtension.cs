using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Extensions
{
	public static class UriExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Uri Append(this Uri thisValue, string path)
		{
			return UriHelper.Combine(thisValue, path);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Uri Append(this Uri thisValue, Uri path)
		{
			return UriHelper.Combine(thisValue, path);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Uri Append(this Uri thisValue, [NotNull] params string[] paths)
		{
			return UriHelper.Combine(thisValue, paths);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string String(this Uri thisValue)
		{
			return thisValue == null
						? null
						: thisValue.IsAbsoluteUri
							? thisValue.AbsoluteUri
							: RelativeUri(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string RelativeUri(this Uri thisValue)
		{
			return thisValue == null
						? null
						: thisValue.IsAbsoluteUri
							? thisValue.PathAndQuery
							: thisValue.OriginalString;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Uri MakeRelativeUri(this Uri thisValue)
		{
			if (thisValue == null) return null;
			string url = thisValue.IsAbsoluteUri
							? thisValue.PathAndQuery
							: thisValue.OriginalString;
			return new Uri(url, UriKind.Relative);
		}
	}
}