using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading;

public sealed class SerialQueue
{
	private readonly object _locker = new object();

	private WeakReference<Task> _lastTask;

	public SerialQueue()
	{
	}

	[NotNull]
	public Task Enqueue([NotNull] Action action) { return Enqueue(action, CancellationToken.None); }
	[NotNull]
	public Task Enqueue([NotNull] Action action, CancellationToken token)
	{
		Task task;

		lock(_locker)
		{
			task = _lastTask != null && _lastTask.TryGetTarget(out Task lastTask) 
						? lastTask.ContinueWith(_ => action(), token) 
						: TaskHelper.Run(action, TaskCreationOptions.PreferFairness, token);
			_lastTask = new WeakReference<Task>(task);
		}

		return task;
	}

	[NotNull]
	public Task<T> Enqueue<T>([NotNull] Func<T> func) { return Enqueue(func, CancellationToken.None); }
	[NotNull]
	public Task<T> Enqueue<T>([NotNull] Func<T> func, CancellationToken token)
	{
		Task<T> task;

		lock(_locker)
		{
			task = _lastTask != null && _lastTask.TryGetTarget(out Task lastTask) 
						? lastTask.ContinueWith(_ => func(), token) 
						: TaskHelper.Run(func, TaskCreationOptions.PreferFairness, token);
			_lastTask = new WeakReference<Task>(task);
		}

		return task;
	}
}