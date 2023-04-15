using System;
using System.Collections.Generic;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
	public bool UseBackgroundThreadsForIO
	{
		get => _factory.UseBackgroundThreadsForIO;
		set => _factory.UseBackgroundThreadsForIO = value;
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
	public AmqpTcpEndpoint Endpoint => _factory.Endpoint;

	/// <inheritdoc />
	public IAuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames) { return _factory.AuthMechanismFactory(mechanismNames); }

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection()
	{
		return Setup(_factory.CreateConnection());
	}

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection(string clientProvidedName) { return Setup(_factory.CreateConnection(clientProvidedName)); }

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints) { return Setup(_factory.CreateConnection(endpoints)); }

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints, string clientProvidedName) { return Setup(_factory.CreateConnection(endpoints, clientProvidedName)); }

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection(IList<string> hostnames) { return Setup(_factory.CreateConnection(hostnames)); }

	/// <inheritdoc />
	[NotNull]
	public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName) { return Setup(_factory.CreateConnection(hostnames, clientProvidedName)); }

	/// <inheritdoc />
	public bool IsAvailable()
	{
		_logger.LogInformation("Checking if server is available.");

		IConnection connection = null;

		try
		{
			connection = CreateConnection();
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
		connection.ConnectionBlocked += OnConnectionBlocked;
		connection.ConnectionUnblocked += OnConnectionUnblocked;
		connection.CallbackException += OnCallbackException;
		connection.ConnectionShutdown += OnConnectionShutdown;
		_logger.LogInformation($"[START] {connection.Endpoint}");
		return connection;
	}

	private void Release([NotNull] IConnection connection)
	{
		connection.ConnectionBlocked -= OnConnectionBlocked;
		connection.ConnectionUnblocked -= OnConnectionUnblocked;
		connection.CallbackException -= OnCallbackException;
		connection.ConnectionShutdown -= OnConnectionShutdown;
	}

	private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs args)
	{
		if (sender is not IConnection cn) return;
		_logger.LogWarning($"[BLOCKED] {cn.Endpoint} {args.Reason}");
	}

	private void OnConnectionUnblocked(object sender, EventArgs _)
	{
		if (sender is not IConnection cn) return;
		_logger.LogInformation($"[UNBLOCKED] {cn.Endpoint}");
	}

	private void OnCallbackException(object sender, CallbackExceptionEventArgs args)
	{
		if (sender is not IConnection cn) return;
		_logger.LogError($"{cn.Endpoint} {args.Exception.CollectMessages()}");
	}

	private void OnConnectionShutdown(object sender, ShutdownEventArgs args)
	{
		if (sender is not IConnection cn) return;
		Release(cn);
		_logger.LogInformation($"[SHUTDOWN] {cn.Endpoint}, {args.Initiator} [{args.Cause}] {args.ReplyCode} {args.ReplyText}");
	}
}