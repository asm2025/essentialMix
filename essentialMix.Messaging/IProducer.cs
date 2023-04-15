using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace essentialMix.Messaging;

public interface IProducer : IHostedService
{
	[NotNull]
	Task<bool> PublishAsync<T>([NotNull] T item, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<int> PublishAsync<T>([NotNull, ItemNotNull] IEnumerable<T> items, CancellationToken token = default(CancellationToken));
}