using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Helpers;
using asm.Threading;

namespace asm.Extensions
{
	public static class CancellationTokenExtension
	{
		private static readonly FieldInfo SOURCE_FIELD;

		static CancellationTokenExtension()
		{
			SOURCE_FIELD = typeof(CancellationToken).GetField("m_source", Constants.BF_NON_PUBLIC_INSTANCE);
		}

		public static bool IsAwaitable(this CancellationToken thisValue) { return thisValue.CanBeCanceled; }

		public static bool IsAwaitable(this CancellationToken? thisValue) { return thisValue != null && IsAwaitable(thisValue.Value); }

		public static bool IsCancellationRequested(this CancellationToken? thisValue) { return thisValue != null && thisValue.Value.IsCancellationRequested; }

		public static Task AsManualResetAwaitable(this CancellationToken thisValue)
		{
			AsyncManualResetEvent ev = new AsyncManualResetEvent();
			if (thisValue.CanBeCanceled) thisValue.Register(() => ev.Set());
			return ev.CompleteAsync();
		}

		public static Task AsTaskResetAwaitable(this CancellationToken thisValue)
		{
			AsyncTaskResetEvent ev = new AsyncTaskResetEvent();
			if (thisValue.CanBeCanceled) thisValue.Register(() => ev.Set());
			return ev.CompleteAsync();
		}

		public static CancellationTokenSource GetSource(this CancellationToken thisValue, CancellationTokenSource defaultCancellationTokenSource = null)
		{
			if (SOURCE_FIELD == null) return defaultCancellationTokenSource;

			WaitHandle _ = thisValue.WaitHandle;
			return (CancellationTokenSource)SOURCE_FIELD.GetValue(thisValue) ?? defaultCancellationTokenSource;
		}

		public static CancellationTokenSource GetSource([NotNull] this IEnumerable<CancellationToken> thisValue, CancellationTokenSource defaultCancellationTokenSource = null)
		{
			return thisValue.Select(e => e.GetSource())
						.FirstOrDefault() ?? defaultCancellationTokenSource;
		}

		public static Task WhenCanceled(this CancellationToken thisValue)
		{
			CancellationTokenAwaiter awaiter = new CancellationTokenAwaiter(thisValue);
			Task task = awaiter.Task;
			// ReSharper disable once MethodSupportsCancellation
			return task.ContinueWith(t => ObjectHelper.Dispose(ref awaiter)).ConfigureAwait();
		}

		[NotNull]
		public static CancellationTokenSource Merge(this CancellationToken thisValue, [NotNull] params CancellationToken[] tokens)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(tokens.Prepend(thisValue));
		}

		[NotNull]
		public static CancellationTokenSource Merge(this CancellationToken thisValue, CancellationToken token)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(thisValue, token);
		}
	}
}