using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Exceptions.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace essentialMix.RabbitMQ;

/// <summary>
/// Base class to handle common <see cref="Producer"/> / <see cref="Consumer"/>
/// such as connecting and creating a channel.
/// </summary>
public abstract class Connector<TSettings> : BackgroundService, IConnector
	where TSettings : ConnectorSettings, new()
{
	private object _syncRoot;
	private IConnection _connection;
	private IModel _channel;

	protected Connector([CanBeNull] IConnectionFactory factory, [NotNull] TSettings settings, ILogger logger)
	{
		Factory = factory;
		Settings = settings;
		Logger = logger;
	}

	/// <inheritdoc />
	public override void Dispose()
	{
		Cleanup();
		base.Dispose();
	}

	public virtual bool IsConnected => _connection?.IsOpen == true;

	protected IConnectionFactory Factory { get; set; }

	[NotNull]
	protected TSettings Settings { get; set; }

	protected IModel Channel => _channel;

	protected object SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	protected ILogger Logger { get; }

	/// <inheritdoc />
	public bool IsAvailable()
	{
		Logger.LogInformation("Checking if server is available.");

		if (IsConnected)
		{
			Logger.LogInformation("Already connected.");
			return true;
		}

		IConnection connection = null;

		try
		{
			connection = Factory?.CreateConnection();
			bool isConnected = connection is { IsOpen: true };
			Logger.LogInformation($"Is connected: {isConnected}");
			return isConnected;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			return false;
		}
		finally
		{
			ObjectHelper.Dispose(ref connection);
		}
	}

	/// <summary>
	/// Connect to RabbitMQ using the <see cref="IConnectionFactory"/>.
	/// </summary>
	public virtual Task<bool> ConnectAsync(CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		Logger.LogInformation("Connecting.");

		if (IsConnected)
		{
			Logger.LogInformation("Already connected.");
			return Task.FromResult(true);
		}

		if (token.CanBeCanceled)
		{
			bool lockTaken;

			if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM);
					return lockTaken || token.IsCancellationRequested;
				}, Settings.Timeout))
			{
				throw new LockTimeoutException();
			}
		}
		else if (!Monitor.TryEnter(SyncRoot, Settings.Timeout))
		{
			throw new LockTimeoutException();
		}

		bool result;

		try
		{
			token.ThrowIfCancellationRequested();
			OnConnecting();
			Cleanup();
			_connection = Factory?.CreateConnection();
			if (_connection is not { IsOpen: true }) return Task.FromResult(false);
			_channel = _connection.CreateModel();

			/*
			* We need to control the in-flight messages (unacked messages being delivered)
			* otherwise the cpu usage will be so intense. It's said in the documentations
			* "values in the 100 through 300 range usually offer optimal throughput".
			* These values seems to be magical numbers though! And it's not clear what is
			* the difference between prefetchSize and prefetchCount.
			* https://www.rabbitmq.com/confirms.html
			*
			* Parameters:
			* long prefetch-size (uint C#)
			*		messages be sent in advance so that when the client finishes processing a message,
			*		the following message is already held locally.
			*		May be set to zero, meaning "no specific limit", although other prefetch limits may
			*		still apply. The prefetch-size is ignored if the no-ack option is set.
			* short prefetch-count (ushort C#)
			*		Specifies a prefetch window in terms of whole messages. This field may be used in
			*		combination with the prefetch-size field; a message will only be sent in advance if
			*		both prefetch windows (and those at the channel and connection level) allow it.
			*		The prefetch-count is ignored if the no-ack option is set.
			* bit global
			*		RabbitMQ has reinterpreted this field. The original specification said: "By default
			*		the QoS settings apply to the current channel only. If this field is set, they are
			*		applied to the entire connection." Instead, RabbitMQ takes global=false to mean that
			*		the QoS settings should apply per-consumer (for new consumers on the channel; existing
			*		ones being unaffected) and global=true to mean that the QoS settings should apply per-channel.
			*
			* Using the documentation as a reference:
			* prefetch-size
			*		is obviously about memory because it's meant to act as something like a buffer.
			*		Setting the prefetch-size to a value will apply more restrictions that might not be needed to
			*		maximize the performance. If the CPU usage gets high, will try a number. This number will have
			*		to unfortunately magical by trial and error because we cannot use one global channel.
			*		It is said also in the documentation under the section Channels and Concurrency Considerations
			*		"As a rule of thumb, sharing Channel instances between threads is something to be avoided.
			*		Applications should prefer using a Channel per thread instead of sharing the same Channel across
			*		multiple threads.
			*
			*		While some operations on channels are safe to invoke concurrently, some are not and will result in
			*		incorrect frame interleaving on the wire, double acknowledgments and so on."
			*		https://www.rabbitmq.com/api-guide.html
			*
			*		Each connector will have its own channel. It's not shared among producers and consumers.
			*
			* global
			*		should be set to true which means applied per channel. Since we have one channel per each connector
			*		(producer/consumer), this means the number should be calculated in conjunction with other channels on this
			*		process. This should be CPU relevant instead of magical. It's still not accurate but relatively accurate
			*		than a random number between 100 through 300.
			*
			* prefetch-count
			*		the total number will set to either the number of Cors or the Cors * 2 if hyper threading is enabled.
			*		This is available in the property of TaskHelper.QueueDefault or TaskHelper.ProcessDefault. Will skip the [factor] for now.
			*
			* It seems like a total number for all the channels need to be maintained.
			* Now I think global should have been implemented as what it's meant  in AMQP 0-9-1
			* https://www.rabbitmq.com/consumer-prefetch.html
			*
			* false means shared across all consumers on the channel
			* true means shared across all consumers on the connection
			* Which could have enabled this to be maintained globally per connection to control the number of threads.
			* Unfortunately it is not, so we have to use magical numbers (grrr) which is difficult to control.
			*/
			Channel.BasicQos(0, 150, true);
			Channel.ExchangeDeclare(Settings.ExchangeQualifiedName, Settings.ExchangeType.ToString().ToLowerInvariant(), Settings.Durable, Settings.AutoDelete);
			OnConnected();
			result = true;
			Logger.LogInformation("Connected.");
		}
		catch (Exception ex)
		{
			Cleanup();
			Logger.LogError(ex.CollectMessages());
			result = false;
		}
		finally
		{
			Monitor.Exit(SyncRoot);
		}

		return Task.FromResult(result);
	}

	public virtual async Task DisconnectAsync(CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		Logger.LogInformation("Disconnecting.");

		if (!IsConnected)
		{
			Logger.LogInformation("Already disconnected.");
			return;
		}

		if (token.CanBeCanceled)
		{
			bool lockTaken;

			if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM);
					return lockTaken || token.IsCancellationRequested;
				}, Settings.Timeout))
			{
				throw new LockTimeoutException();
			}
		}
		else if (!Monitor.TryEnter(SyncRoot, Settings.Timeout))
		{
			throw new LockTimeoutException();
		}

		try
		{
			OnDisconnecting();
			await base.StopAsync(token);
			OnDisconnected();
			Logger.LogInformation("Disconnected.");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
		}
		finally
		{
			Cleanup();
			Monitor.Exit(SyncRoot);
		}
	}

	/// <inheritdoc />
	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		Logger.LogInformation($"{GetType().Name} is starting...");

		if (cancellationToken.CanBeCanceled)
		{
			bool lockTaken;

			if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM);
					return lockTaken || cancellationToken.IsCancellationRequested;
				}, Settings.Timeout))
			{
				throw new LockTimeoutException();
			}
		}
		else if (!Monitor.TryEnter(SyncRoot, Settings.Timeout))
		{
			throw new LockTimeoutException();
		}

		try
		{
			await base.StartAsync(cancellationToken);
			if (!await ConnectAsync(cancellationToken)) return;
			await OnStartedAsync(cancellationToken);
			Logger.LogInformation("Finished.");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
		}
		finally
		{
			Monitor.Exit(SyncRoot);
		}
	}

	/// <inheritdoc />
	[NotNull]
	public override Task StopAsync(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Stopping.");
		return DisconnectAsync(cancellationToken)
			.ContinueWith(_ =>
			{
				Logger.LogInformation("Stopped.");
			}, cancellationToken);
	}

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		stoppingToken.ThrowIfCancellationRequested();
		Logger.LogInformation("Executing.");

		if (stoppingToken.CanBeCanceled)
		{
			bool lockTaken;

			if (!SpinWait.SpinUntil(() =>
				{
					lockTaken = Monitor.TryEnter(SyncRoot, TimeSpanHelper.MINIMUM);
					return lockTaken || stoppingToken.IsCancellationRequested;
				}, Settings.Timeout))
			{
				throw new LockTimeoutException();
			}
		}
		else if (!Monitor.TryEnter(SyncRoot, Settings.Timeout))
		{
			throw new LockTimeoutException();
		}

		try
		{
			if (!await ConnectAsync(stoppingToken)) return;
			await OnExecutingAsync(stoppingToken);
			Logger.LogInformation("Finished.");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
		}
		finally
		{
			Monitor.Exit(SyncRoot);
		}
	}

	protected virtual void OnConnecting() { }
	protected virtual void OnConnected() { }
	protected virtual void OnDisconnecting() { }
	protected virtual void OnDisconnected() { }

	protected void Cleanup()
	{
		ObjectHelper.Dispose(ref _channel);
		ObjectHelper.Dispose(ref _connection);
	}

	[NotNull]
	protected virtual Task OnStartedAsync(CancellationToken token)
	{
		return Task.CompletedTask;
	}

	[NotNull]
	protected virtual Task OnExecutingAsync(CancellationToken token)
	{
		return Task.CompletedTask;
	}
}

public abstract class Connector : Connector<ConnectorSettings>
{
	protected Connector([NotNull] IConnectionFactory factory, [NotNull] ConnectorSettings settings, ILogger logger)
		: base(factory, settings, logger)
	{
	}
}
