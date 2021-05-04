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

			Action receiveAndSend;

			if (delay <= TimeSpan.Zero)
			{
				if (timeout <= TimeSpan.Zero)
				{
					receiveAndSend = async () =>
					{
						if (token.IsCancellationRequested) return;
						T item = await thisValue.ReceiveAsync(token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await target.SendAsync(item, token).ConfigureAwait();
					};
				}
				else
				{
					receiveAndSend = async () =>
					{
						if (token.IsCancellationRequested) return;
						T item = await thisValue.ReceiveAsync(timeout, token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await target.SendAsync(item, token).ConfigureAwait();
					};
				}
			}
			else
			{
				if (timeout <= TimeSpan.Zero)
				{
					receiveAndSend = async () =>
					{
						if (token.IsCancellationRequested) return;
						T item = await thisValue.ReceiveAsync(token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await Task.Delay(delay, token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await target.SendAsync(item, token).ConfigureAwait();
					};
				}
				else
				{
					receiveAndSend = async () =>
					{
						if (token.IsCancellationRequested) return;
						T item = await thisValue.ReceiveAsync(timeout, token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await Task.Delay(delay, token).ConfigureAwait();
						if (token.IsCancellationRequested) return;
						await target.SendAsync(item, token).ConfigureAwait();
					};
				}
			}

			while (await thisValue.OutputAvailableAsync(token).ConfigureAwait())
				receiveAndSend();

			target.Complete();
			return DataflowBlock.Encapsulate(thisValue, target);
		}
	}
}