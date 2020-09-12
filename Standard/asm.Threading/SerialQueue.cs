using System;
using System.Threading.Tasks;
using asm.Threading.Internal.Extensions;
using JetBrains.Annotations;

namespace asm.Threading
{
	public sealed class SerialQueue
	{
		private readonly object _locker = new object();
		private WeakReference<Task> _lastTask;

		public SerialQueue() { }

		[NotNull]
		public Task Enqueue([NotNull] Action action)
		{
			Task<object> task;

			lock(_locker)
			{
				task = Enqueue<object>(() =>
				{
					action();
					return null;
				});
			}

			return task;
		}

		[NotNull]
		public Task<T> Enqueue<T>([NotNull] Func<T> func)
		{
			Task<T> task;

			lock (_locker)
			{
				task = _lastTask != null && _lastTask.TryGetTarget(out Task lastTask) 
					? lastTask.ContinueWith(_ => func()).ConfigureAwait() 
					: Task.Run(func).ConfigureAwait();
				_lastTask = new WeakReference<Task>(task);
			}

			return task;
		}
	}
}