using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class TaskExtension
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task ConfigureAwait([NotNull] this Task thisValue, bool continueOnCapturedContext = false)
		{
			thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
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
			thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue, out ConfiguredTaskAwaitable<TResult> awaitable, bool continueOnCapturedContext = false)
		{
			awaitable = thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Execute([NotNull] this Task thisValue, bool continueOnCapturedContext = false)
		{
			thisValue.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static TResult Execute<TResult>([NotNull] this Task<TResult> thisValue, bool continueOnCapturedContext = false)
		{
			return thisValue.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
		}
	}
}