using JetBrains.Annotations;
using System.Threading;
using System.Threading.Tasks;

namespace essentialMix.Messaging;

public interface IConnector
{
	public bool IsConnected { get; }
	[NotNull]
	public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default(CancellationToken));
	[NotNull]
	public Task<bool> ConnectAsync(CancellationToken cancellationToken = default(CancellationToken));
	[NotNull]
	public Task DisconnectAsync(CancellationToken token = default(CancellationToken));
}