using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class TaskCompletionSourceExtension
{
	public static bool TryCompleteFromTask([NotNull] this TaskCompletionSource<object> thisValue, [NotNull] Task task)
	{
		if (task.IsFaulted) return thisValue.TrySetException(task.Exception!.InnerExceptions);

		if (task.IsCanceled)
		{
			try
			{
				task.Wait();
			}
			catch (OperationCanceledException ex)
			{
				CancellationToken token = ex.CancellationToken;
				return token.IsCancellationRequested
							? thisValue.TrySetCanceled(token)
							: thisValue.TrySetCanceled();
			}
		}

		return thisValue.TrySetResult(task.GetResult());
	}

	public static bool TryCompleteFromTask<T>([NotNull] this TaskCompletionSource<T> thisValue, [NotNull] Task<T> task)
	{
		if (task.IsFaulted) return thisValue.TrySetException(task.Exception!.InnerExceptions);

		if (task.IsCanceled)
		{
			try
			{
				task.Wait();
			}
			catch (OperationCanceledException ex)
			{
				CancellationToken token = ex.CancellationToken;
				return token.IsCancellationRequested
							? thisValue.TrySetCanceled(token)
							: thisValue.TrySetCanceled();
			}
		}

		return thisValue.TrySetResult(task.Result);
	}
}