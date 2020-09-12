using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Threading.Internal.Extensions
{
	internal static class TaskExtension
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task ConfigureAwait([NotNull] this Task thisValue, bool continueOnCapturedContext = false)
		{
			return ConfigureAwait(thisValue, out _, continueOnCapturedContext);
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task ConfigureAwait([NotNull] this Task thisValue, out ConfiguredTaskAwaitable awaitable, bool continueOnCapturedContext = false)
		{
			awaitable = thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue, bool continueOnCapturedContext = false)
		{
			return ConfigureAwait(thisValue, out _, continueOnCapturedContext);
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue, out ConfiguredTaskAwaitable<TResult> awaitable, bool continueOnCapturedContext = false)
		{
			awaitable = thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}
	}
}