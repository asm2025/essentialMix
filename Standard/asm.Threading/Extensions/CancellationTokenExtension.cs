using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using asm.Helpers;
using asm.Threading;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class CancellationTokenExtension
	{
		private static readonly Lazy<FieldInfo> __sourceField = new Lazy<FieldInfo>(() => typeof(CancellationToken).GetField("m_source", Constants.BF_NON_PUBLIC_INSTANCE), LazyThreadSafetyMode.PublicationOnly);

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAwaitable(this CancellationToken thisValue) { return thisValue.CanBeCanceled; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAwaitable(this CancellationToken? thisValue) { return thisValue != null && IsAwaitable(thisValue.Value); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsCancelledOrDisposed(this CancellationToken? thisValue)
		{
			try { return thisValue == null || thisValue.Value.IsCancellationRequested; }
			catch (ObjectDisposedException) { return true; }
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static CancellationTokenSource GetSource(this CancellationToken thisValue, CancellationTokenSource defaultCancellationTokenSource = null)
		{
			if (__sourceField == null) return defaultCancellationTokenSource;
			WaitHandle _ = thisValue.WaitHandle;
			return (CancellationTokenSource)__sourceField.Value.GetValue(thisValue) ?? defaultCancellationTokenSource;
		}

		public static CancellationTokenSource GetSource([NotNull] this IEnumerable<CancellationToken> thisValue, CancellationTokenSource defaultCancellationTokenSource = null)
		{
			return thisValue.Select(e => e.GetSource())
						.FirstOrDefault() ?? defaultCancellationTokenSource;
		}

		[NotNull]
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