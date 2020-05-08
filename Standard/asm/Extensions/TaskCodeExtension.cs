using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class TaskCodeExtension
	{
		[NotNull]
		public static Task ConfigureAwait([NotNull] this Task thisValue, bool continueOnCapturedContext = false)
		{
			return ConfigureAwait(thisValue, out _, continueOnCapturedContext);
		}

		[NotNull]
		public static Task ConfigureAwait([NotNull] this Task thisValue, out ConfiguredTaskAwaitable awaitable, bool continueOnCapturedContext = false)
		{
			awaitable = thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}

		[NotNull]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue, bool continueOnCapturedContext = false)
		{
			return ConfigureAwait(thisValue, out _, continueOnCapturedContext);
		}

		[NotNull]
		public static Task<TResult> ConfigureAwait<TResult>([NotNull] this Task<TResult> thisValue, out ConfiguredTaskAwaitable<TResult> awaitable, bool continueOnCapturedContext = false)
		{
			awaitable = thisValue.ConfigureAwait(continueOnCapturedContext);
			return thisValue;
		}
	}
}