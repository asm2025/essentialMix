using essentialMix.Exceptions.Network;
using essentialMix.Extensions;
using essentialMix.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace essentialMix.RabbitMQ;

/// <summary>
/// Generates entries and send them to RabbitMQ queue.
/// It waits for entries until <see cref="Connector.StopAsync(CancellationToken)" /> is called or the object is disposed.
/// </summary>
public abstract class Producer<TSettings> : Connector<TSettings>, IProducer
	where TSettings : ConnectorSettings, new()
{
	/// <inheritdoc />
	protected Producer([NotNull] IConnectionFactory factory, [NotNull] TSettings settings, ILogger logger)
		: base(factory, settings, logger)
	{
	}

	/// <summary>
	/// Publishes a message to the RabbitMQ exchange managed by the <see cref="Connector{TSettings}"/>
	/// </summary>
	public virtual async Task<bool> PublishAsync<T>(T item, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (!await CheckConnection(token)) return false;

		byte[] buffer = await SerializeAsync(item, token);
		if (buffer is not { Length: > 0 }) return false;

		try
		{
			BasicProperties properties = new BasicProperties
			{
				Persistent = true,
				ContentType = "application/json",
				DeliveryMode = DeliveryModes.Persistent
			};
			await Channel.BasicPublishAsync(Settings.ExchangeQualifiedName, Settings.Route, true, properties, buffer, token);
		}
		catch (AlreadyClosedException acx)
		{
			Logger.LogWarning(acx.CollectMessages());
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
		}

		return true;
	}

	/// <summary>
	/// Publishes a message to the RabbitMQ exchange managed by the <see cref="Connector{TSettings}"/>
	/// </summary>
	public virtual async Task<int> PublishAsync<T>(IEnumerable<T> items, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		if (!await CheckConnection(token)) return 0;

		int count = 0;

		foreach (T item in items)
		{
			byte[] buffer = await SerializeAsync(item, token);
			if (buffer is not { Length: > 0 }) continue;

			try
			{
				BasicProperties properties = new BasicProperties
				{
					Persistent = true,
					ContentType = "application/json",
					DeliveryMode = DeliveryModes.Persistent
				};
				await Channel.BasicPublishAsync(Settings.ExchangeQualifiedName, Settings.Route, true, properties, buffer, token);
				count++;
			}
			catch (AlreadyClosedException acx)
			{
				Logger.LogWarning(acx.CollectMessages());
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.CollectMessages());
			}
		}

		return count;
	}

	/// <summary>
	/// Wait for entries and push them into RabbitMQ exchange
	/// </summary>
	protected override Task OnExecutingAsync(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!IsConnected) return Task.FromException(new NotConnectedException());
		return token.IsCancellationRequested
					? Task.FromCanceled(token)
					: Task.CompletedTask;
	}

	private async Task<bool> CheckConnection(CancellationToken token)
	{
		if (Channel is not null && IsConnected) return true;
		await ConnectAsync(token);
		token.ThrowIfCancellationRequested();
		return Channel is not null && IsConnected;
	}

	[NotNull]
	protected abstract Task<byte[]> SerializeAsync<T>(T item, CancellationToken token);
}

public abstract class Producer : Producer<ConnectorSettings>
{
	/// <inheritdoc />
	protected Producer([NotNull] IConnectionFactory factory, [NotNull] ConnectorSettings settings, ILogger logger)
		: base(factory, settings, logger)
	{
	}
}
