using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class UriBuilderExtension
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static UriBuilder AddPathSeparator([NotNull] this UriBuilder thisValue)
		{
			if (thisValue.Path.Length == 0 || thisValue.Path[thisValue.Path.Length - 1] != '/') thisValue.Path += '/';
			return thisValue;
		}
	}
}