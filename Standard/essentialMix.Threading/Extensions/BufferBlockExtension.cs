using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using essentialMix.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class BufferBlockExtension
	{
		[NotNull]
		public static IPropagatorBlock<T, T> CreateDelayedBlock<T>([NotNull] this BufferBlock<T> thisValue, TimeSpan delay, CancellationToken token = default(CancellationToken))
		{
			return CreateDelayedBlock(thisValue, delay, TimeSpan.Zero, token);
		}

		[NotNull]
		public static IPropagatorBlock<T, T> CreateDelayedBlock<T>([NotNull] this BufferBlock<T> thisValue, TimeSpan delay, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			BufferBlock<T> target = new BufferBlock<T>();
			Action receiveAndPost;

			if (delay <= TimeSpan.Zero)
			{
				if (timeout <= TimeSpan.Zero)
				{
					receiveAndPost = () =>
					{
						if (token.IsCancellationRequested) return;
						T item = thisValue.Receive(token);
						if (token.IsCancellationRequested) return;
						target.Post(item);
					};
				}
				else
				{
					receiveAndPost = () =>
					{
						if (token.IsCancellationRequested) return;
						T item = thisValue.Receive(timeout, token);
						if (token.IsCancellationRequested) return;
						target.Post(item);
					};
				}
			}
			else
			{
				if (timeout <= TimeSpan.Zero)
				{
					receiveAndPost = () =>
					{
						if (token.IsCancellationRequested) return;
						T item = thisValue.Receive(token);
						if (token.IsCancellationRequested) return;
						TimeSpanHelper.WasteTime(delay, token);
						if (token.IsCancellationRequested) return;
						target.Post(item);
					};
				}
				else
				{
					receiveAndPost = () =>
					{
						if (token.IsCancellationRequested) return;
						T item = thisValue.Receive(timeout, token);
						if (token.IsCancellationRequested) return;
						TimeSpanHelper.WasteTime(delay, token);
						if (token.IsCancellationRequested) return;
						target.Post(item);
					};
				}
			}

			do
			{
				bool outputAvailable = thisValue.OutputAvailableAsync(token).Execute();
				if (token.IsCancellationRequested || !outputAvailable) break;
				receiveAndPost();
			}
			while (!token.IsCancellationRequested);

			target.Complete();
			return DataflowBlock.Encapsulate(thisValue, target);
		}

		public static Task<IPropagatorBlock<T, T>> CreateDelayedBlockAsync<T>([NotNull] this BufferBlock<T> thisValue, TimeSpan delay, CancellationToken token = default(CancellationToken))
		{
			return CreateDelayedBlockAsync(thisValue, delay, TimeSpan.Zero, token);
		}

		[ItemNotNull]
		public static async Task<IPropagatorBlock<T, T>> CreateDelayedBlockAsync<T>([NotNull] this BufferBlock<T> thisValue, TimeSpan delay, TimeSpan timeout, CancellationToken token = default(CancellationToken))
		{
			BufferBlock<T> target = new BufferBlock<T>();

			if (delay <= TimeSpan.Zero)
			{
				if (timeout <= TimeSpan.Zero)
				{
					while (await thisValue.OutputAvailableAsync(token).ConfigureAwait())
						await ReceiveAndSend(thisValue, target, token);
				}
				else
				{
					while (await thisValue.OutputAvailableAsync(token).ConfigureAwait())
						await ReceiveAndSendWithTimeout(thisValue, target, timeout, token);
				}
			}
			else
			{
				if (timeout <= TimeSpan.Zero)
				{
					while (await thisValue.OutputAvailableAsync(token).ConfigureAwait())
						await ReceiveAndSendWithDelay(thisValue, target, delay, token);
				}
				else
				{
					while (await thisValue.OutputAvailableAsync(token).ConfigureAwait())
						await ReceiveAndSendWithDelayAndTimeout(thisValue, target, delay, timeout, token);
				}
			}

			target.Complete();
			return DataflowBlock.Encapsulate(thisValue, target);

			static async Task ReceiveAndSend(BufferBlock<T> source, BufferBlock<T> target, CancellationToken token)
			{
				if (token.IsCancellationRequested) return;
				T item = await source.ReceiveAsync(token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await target.SendAsync(item, token).ConfigureAwait();
			}

			static async Task ReceiveAndSendWithTimeout(BufferBlock<T> source, BufferBlock<T> target, TimeSpan timeout, CancellationToken token)
			{
				if (token.IsCancellationRequested) return;
				T item = await source.ReceiveAsync(timeout, token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await target.SendAsync(item, token).ConfigureAwait();
			}

			static async Task ReceiveAndSendWithDelay(BufferBlock<T> source, BufferBlock<T> target, TimeSpan delay, CancellationToken token)
			{
				if (token.IsCancellationRequested) return;
				T item = await source.ReceiveAsync(token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await Task.Delay(delay, token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await target.SendAsync(item, token).ConfigureAwait();
			}

			static async Task ReceiveAndSendWithDelayAndTimeout(BufferBlock<T> source, BufferBlock<T> target, TimeSpan delay, TimeSpan timeout, CancellationToken token)
			{
				if (token.IsCancellationRequested) return;
				T item = await source.ReceiveAsync(timeout, token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await Task.Delay(delay, token).ConfigureAwait();
				if (token.IsCancellationRequested) return;
				await target.SendAsync(item, token).ConfigureAwait();
			}
		}
	}
}