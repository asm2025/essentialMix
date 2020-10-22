using System;
using System.Runtime.CompilerServices;
using System.Threading;
using asm.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class WaitHandleExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAwaitable(this WaitHandle thisValue) { return thisValue != null && thisValue.SafeWaitHandle.IsAwaitable(); }

		[NotNull]
		public static WaitHandle[] Merge(this WaitHandle thisValue, [NotNull] params WaitHandle[] waitHandles) { return waitHandles.Prepend(thisValue); }

		public static bool WaitOne([NotNull] this WaitHandle thisValue, CancellationToken token = default(CancellationToken)) { return WaitOne(thisValue, TimeSpanHelper.INFINITE, false, false, token); }

		public static bool WaitOne([NotNull] this WaitHandle thisValue, bool setEvent, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, TimeSpanHelper.INFINITE, setEvent, false, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, TimeSpanHelper.INFINITE, setEvent, exitContext, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, timeout.TotalIntMilliseconds(), false, false, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, bool setEvent, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, timeout.TotalIntMilliseconds(), setEvent, false, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, timeout.TotalIntMilliseconds(), setEvent, exitContext, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, millisecondsTimeout, false, false, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, bool setEvent, CancellationToken token = default(CancellationToken))
		{
			return WaitOne(thisValue, millisecondsTimeout, setEvent, false, token);
		}

		public static bool WaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, bool setEvent, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			bool result;

			if (!token.IsAwaitable())
			{
				result = thisValue.WaitOne(millisecondsTimeout, exitContext);
			}
			else if (token.IsCancellationRequested)
			{
				result = false;
			}
			else
			{
				int n = WaitHandle.WaitAny(new[] { thisValue, token.WaitHandle }, millisecondsTimeout, exitContext);
				result = n == 0;
			}

			if (result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
			return result;
		}

		public static bool WaitAll([NotNull] this WaitHandle[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitAll(thisValue, TimeSpanHelper.INFINITE, false, token); }

		public static bool WaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitAll(thisValue, timeout.TotalIntMilliseconds(), false, token);
		}

		public static bool WaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitAll(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
		}

		public static bool WaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			return WaitAll(thisValue, millisecondsTimeout, false, token);
		}

		public static bool WaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			if (!token.IsAwaitable()) return WaitHandle.WaitAll(thisValue, millisecondsTimeout, exitContext);
			if (token.IsCancellationRequested) return false;

			bool result = false;
			SpinWait.SpinUntil(() =>
			{
				result = WaitHandle.WaitAll(thisValue, millisecondsTimeout, exitContext);
				return !result || token.IsCancellationRequested;
			}, millisecondsTimeout);

			return result && !token.IsCancellationRequested;
		}

		public static int WaitAny([NotNull] this WaitHandle[] thisValue, CancellationToken token = default(CancellationToken)) { return WaitAny(thisValue, TimeSpanHelper.INFINITE, false, token); }

		public static int WaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			return WaitAny(thisValue, timeout.TotalIntMilliseconds(), false, token);
		}

		public static int WaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			return WaitAny(thisValue, timeout.TotalIntMilliseconds(), exitContext, token);
		}

		public static int WaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, CancellationToken token = default(CancellationToken))
		{
			return WaitAny(thisValue, millisecondsTimeout, false, token);
		}

		public static int WaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, bool exitContext, CancellationToken token = default(CancellationToken))
		{
			if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!token.IsAwaitable()) return WaitHandle.WaitAny(thisValue, millisecondsTimeout, exitContext);

			WaitHandle[] handles = thisValue.Append(token.WaitHandle);
			return WaitHandle.WaitAny(handles, millisecondsTimeout, exitContext);
		}

		public static bool SignalAndWait([NotNull] this WaitHandle thisValue, [NotNull] WaitHandle toWaitOn, TimeSpan timeout, bool exitContext = false)
		{
			return SignalAndWait(thisValue, toWaitOn, timeout.TotalIntMilliseconds(), exitContext);
		}

		public static bool SignalAndWait([NotNull] this WaitHandle thisValue, [NotNull] WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext = false)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			return WaitHandle.SignalAndWait(thisValue, toWaitOn, millisecondsTimeout, exitContext);
		}

		public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, Func<bool> evalFunc, bool setEvent = false, bool exitContext = false)
		{
			return SpinWaitOne(thisValue, TimeSpanHelper.INFINITE, evalFunc, setEvent, exitContext);
		}

		public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool setEvent = false, bool exitContext = false)
		{
			return SpinWaitOne(thisValue, timeout.TotalIntMilliseconds(), evalFunc, setEvent, exitContext);
		}

		public static bool SpinWaitOne([NotNull] this WaitHandle thisValue, int millisecondsTimeout, Func<bool> evalFunc, bool setEvent = false, bool exitContext = false)
		{
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
			if (!IsAwaitable(thisValue)) return true;

			Func<bool> predicate = evalFunc == null
								? (Func<bool>)(() => !IsAwaitable(thisValue) || thisValue.WaitOne(TimeSpanHelper.MINIMUM, exitContext))
								: () => !IsAwaitable(thisValue) || thisValue.WaitOne(TimeSpanHelper.MINIMUM, exitContext) || evalFunc();
			bool result = SpinWait.SpinUntil(predicate, millisecondsTimeout);
			if (result && setEvent && thisValue is EventWaitHandle evt) evt.Set();
			return result;
		}

		public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, Func<bool> evalFunc, bool exitContext = false)
		{
			return SpinWaitAll(thisValue, TimeSpanHelper.INFINITE, evalFunc, exitContext);
		}

		public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool exitContext = false)
		{
			return SpinWaitAll(thisValue, timeout.TotalIntMilliseconds(), evalFunc, exitContext);
		}

		public static bool SpinWaitAll([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, Func<bool> evalFunc, bool exitContext = false)
		{
			if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			Func<bool> predicate = evalFunc == null
								? (Func<bool>)(() => WaitHandle.WaitAll(thisValue, TimeSpanHelper.MINIMUM, exitContext))
								: () => WaitHandle.WaitAll(thisValue, TimeSpanHelper.MINIMUM, exitContext) || evalFunc();
			return SpinWait.SpinUntil(predicate, millisecondsTimeout);
		}

		public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, Func<bool> evalFunc = null) { return SpinWaitAny(thisValue, TimeSpanHelper.INFINITE, evalFunc); }

		public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, TimeSpan timeout, Func<bool> evalFunc = null, bool exitContext = false)
		{
			return SpinWaitAny(thisValue, timeout.TotalIntMilliseconds(), evalFunc, exitContext);
		}

		public static int SpinWaitAny([NotNull] this WaitHandle[] thisValue, int millisecondsTimeout, Func<bool> evalFunc = null, bool exitContext = false)
		{
			if (thisValue.Length == 0) throw new ArgumentNullException(nameof(thisValue));
			if (millisecondsTimeout < TimeSpanHelper.INFINITE) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

			int index = -1;
			Func<bool> predicate = evalFunc == null
								? (Func<bool>)(() =>
												{
													index = WaitHandle.WaitAny(thisValue, TimeSpanHelper.MINIMUM, exitContext);
													return index == 0;
												})
								: () =>
								{
									index = WaitHandle.WaitAny(thisValue, TimeSpanHelper.MINIMUM, exitContext);
									return index == 0 || evalFunc();
								};

			SpinWait.SpinUntil(predicate, millisecondsTimeout);
			return index;
		}
	}
}