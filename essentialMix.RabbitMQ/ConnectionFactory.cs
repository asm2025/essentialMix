using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQFactory = RabbitMQ.Client.ConnectionFactory;

namespace essentialMix.RabbitMQ;

public class ConnectionFactory : Disposable, IConnectionFactory
{
	private static IConnectionFactory __instance;

	private readonly RabbitMQFactory _factory;
	private readonly ILogger _logger;

	private ConnectionFactory(FactorySettings settings, ILogger logger)
	{
		settings ??= new FactorySettings();
		_factory = new RabbitMQFactory
		{
			HostName = settings.Host,
			Port = settings.Port,
		};

		if (!string.IsNullOrEmpty(settings.VirtualHost)) _factory.VirtualHost = settings.VirtualHost;

		if (!string.IsNullOrEmpty(settings.UserName))
		{
			_factory.UserName = settings.UserName;
			if (!string.IsNullOrEmpty(settings.Password)) _factory.Password = settings.Password;
		}

		_logger = logger;
	}

	[NotNull]
	public static IConnectionFactory Instance
	{
		get
		{
			if (__instance == null) Interlocked.CompareExchange(ref __instance, Create(), null);
			return __instance!;
		}
	}

	/// <inheritdoc />
	public Uri Uri
	{
		get => _factory.Uri;
		set => _factory.Uri = value;
	}

	/// <inheritdoc />
	public string VirtualHost
	{
		get => _factory.VirtualHost;
		set => _factory.VirtualHost = UriHelper.Trim(value) ?? "/";
	}

	/// <inheritdoc />
	public string UserName
	{
		get => _factory.UserName;
		set => _factory.UserName = value;
	}

	/// <inheritdoc />
	public string Password
	{
		get => _factory.Password;
		set => _factory.Password = value;
	}

	/// <inheritdoc />
	public ICredentialsProvider CredentialsProvider
	{
		get => _factory.CredentialsProvider;
		set => _factory.CredentialsProvider = value;
	}

	/// <inheritdoc />
	public string ClientProvidedName
	{
		get => _factory.ClientProvidedName;
		set => _factory.ClientProvidedName = value;
	}

	/// <inheritdoc />
	public IDictionary<string, object> ClientProperties
	{
		get => _factory.ClientProperties;
		set => _factory.ClientProperties = value;
	}

	/// <inheritdoc />
	public ushort RequestedChannelMax
	{
		get => _factory.RequestedChannelMax;
		set => _factory.RequestedChannelMax = value;
	}

	/// <inheritdoc />
	public uint RequestedFrameMax
	{
		get => _factory.RequestedFrameMax;
		set => _factory.RequestedFrameMax = value;
	}

	/// <inheritdoc />
	public TimeSpan RequestedHeartbeat
	{
		get => _factory.RequestedHeartbeat;
		set => _factory.RequestedHeartbeat = value;
	}

	/// <inheritdoc />
	public TimeSpan HandshakeContinuationTimeout
	{
		get => _factory.HandshakeContinuationTimeout;
		set => _factory.HandshakeContinuationTimeout = value;
	}

	/// <inheritdoc />
	public TimeSpan ContinuationTimeout
	{
		get => _factory.ContinuationTimeout;
		set => _factory.ContinuationTimeout = value;
	}

	/// <inheritdoc />
	public ushort ConsumerDispatchConcurrency { get; set; }

	/// <inheritdoc />
	[NotNull]
	public AmqpTcpEndpoint Endpoint => _factory.Endpoint;

	/// <inheritdoc />
	public IAuthMechanismFactory AuthMechanismFactory(IEnumerable<string> mechanismNames) { return _factory.AuthMechanismFactory(mechanismNames); }

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
	{
		return _factory.CreateConnectionAsync(cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(string clientProvidedName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _factory.CreateConnectionAsync(clientProvidedName, cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(IEnumerable<string> hostnames, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _factory.CreateConnectionAsync(hostnames, cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(IEnumerable<string> hostnames, string clientProvidedName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _factory.CreateConnectionAsync(hostnames, clientProvidedName, cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint> endpoints, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _factory.CreateConnectionAsync(endpoints, cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public Task<IConnection> CreateConnectionAsync(IEnumerable<AmqpTcpEndpoint> endpoints, string clientProvidedName, CancellationToken cancellationToken = new CancellationToken())
	{
		return _factory.CreateConnectionAsync(endpoints, clientProvidedName, cancellationToken).ContinueWith(e => Setup(e.Result), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
	}

	/// <inheritdoc />
	public bool IsAvailable()
	{
		_logger.LogInformation("Checking if server is available.");

		IConnection connection = null;

		try
		{
			connection = CreateConnectionAsync(CancellationToken.None).Execute();
			bool isConnected = connection is { IsOpen: true };
			_logger.LogInformation($"Is connected: {isConnected}");
			return isConnected;
		}
		catch
		{
			return false;
		}
		finally
		{
			ObjectHelper.Dispose(ref connection);
		}
	}

	[NotNull]
	public static IConnectionFactory Create(FactorySettings settings = null, ILogger logger = null) { return new ConnectionFactory(settings, logger); }

	[NotNull]
	private IConnection Setup([NotNull] IConnection connection)
	{
		if (_logger == null) return connection;
		connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;
		connection.ConnectionUnblockedAsync += OnConnectionUnblockedAsync;
		connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
		connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
		_logger.LogInformation($"[START] {connection.Endpoint}");
		return connection;
	}

	private void Release([NotNull] IConnection connection)
	{
		connection.ConnectionBlockedAsync -= OnConnectionBlockedAsync;
		connection.ConnectionUnblockedAsync -= OnConnectionUnblockedAsync;
		connection.CallbackExceptionAsync -= OnCallbackExceptionAsync;
		connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
	}

	[NotNull]
	private Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs args)
	{
		if (sender is IConnection cn) _logger.LogWarning($"[BLOCKED] {cn.Endpoint} {args.Reason}");
		return Task.CompletedTask;
	}

	[NotNull]
	private Task OnConnectionUnblockedAsync(object sender, AsyncEventArgs asyncEventArgs)
	{
		if (sender is IConnection cn) _logger.LogInformation($"[UNBLOCKED] {cn.Endpoint}");
		return Task.CompletedTask;
	}

	[NotNull]
	private Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs args)
	{
		if (sender is IConnection cn) _logger.LogError($"{cn.Endpoint} {args.Exception.CollectMessages()}");
		return Task.CompletedTask;
	}

	[NotNull]
	private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs args)
	{
		if (sender is not IConnection cn) return Task.CompletedTask;
		Release(cn);
		_logger.LogInformation($"[SHUTDOWN] {cn.Endpoint}, {args.Initiator} [{args.Cause}] {args.ReplyCode} {args.ReplyText}");
		return Task.CompletedTask;
	}
}