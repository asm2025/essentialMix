using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Messaging;

public interface IConnector
{
	public bool IsConnected { get; }
	public bool IsAvailable();
	[NotNull]
	public Task<bool> ConnectAsync(CancellationToken token = default(CancellationToken));
	[NotNull]
	public Task DisconnectAsync(CancellationToken token = default(CancellationToken));
}