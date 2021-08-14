using System;
using System.Security.AccessControl;
using JetBrains.Annotations;

namespace essentialMix.Threading.Patterns.ProducerConsumer
{
	public class SemaphoreQueueOptions<T> : ProducerConsumerThreadNamedQueueOptions<T>
	{
		/// <inheritdoc />
		public SemaphoreQueueOptions([NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(int threads, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(int threads, bool waitOnDispose, [NotNull] Action<IProducerConsumer<T>, T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions([NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(waitOnDispose, executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(int threads, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(threads, executeCallback)
		{
		}

		/// <inheritdoc />
		public SemaphoreQueueOptions(int threads, bool waitOnDispose, [NotNull] ExecuteCallbackDelegates<T> executeCallback)
			: base(threads, waitOnDispose, executeCallback)
		{
		}

		/// <summary>
		/// To create a named semaphore with access control security, see this <see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphore.openexisting">example</see>
		/// </summary>
		public SemaphoreSecurity Security { get; set; }
	}
}