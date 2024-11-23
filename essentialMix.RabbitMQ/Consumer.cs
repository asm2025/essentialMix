using essentialMix.Exceptions.Network;
using essentialMix.Extensions;
using essentialMix.Messaging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace essentialMix.RabbitMQ;

/// <summary>
/// Read entries from RabbitMQ queue and process them.
/// It waits for entries until <see cref="Connector.StopAsync(CancellationToken)" /> is called or the object is disposed.
/// </summary>
public abstract class Consumer<TSettings> : Connector<TSettings>, IConsumer
	where TSettings : ConsumeSettings, new()
{
	// each consumer will use its own queue to read the exchange. It will be deleted on Object's disposal.
	private string _queueName;
	private AsyncEventingBasicConsumer _consumer;

	/// <inheritdoc />
	protected Consumer([CanBeNull] IConnectionFactory factory, [NotNull] TSettings settings, ILogger logger)
		: base(factory, settings, logger)
	{
	}

	/// <inheritdoc />
	protected override async Task OnConnectedAsync()
	{
		_queueName = (await Channel.QueueDeclareAsync(string.Empty, Settings.Durable, Settings.Exclusive, Settings.AutoDelete)).QueueName;
		await Channel.QueueBindAsync(_queueName, Settings.ExchangeQualifiedName, Settings.Route);
		_consumer = new AsyncEventingBasicConsumer(Channel);
		_consumer.ReceivedAsync += OnReceivedAsync;
		await Channel.BasicConsumeAsync(_queueName, false, GetType().Name, false, false, null, _consumer);
		await base.OnConnectedAsync();
	}

	/// <inheritdoc />
	protected override async Task OnDisconnectedAsync()
	{
		_consumer.ReceivedAsync -= OnReceivedAsync;
		await Channel.QueueDeleteAsync(_queueName, false, true);
		_queueName = null;
		_consumer = null;
		await base.OnDisconnectedAsync();
	}

	/// <summary>
	/// Read the RabbitMQ queued messages and process them
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	protected override Task OnExecutingAsync(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		if (!IsConnected) return Task.FromException(new NotConnectedException());
		return token.IsCancellationRequested
					? Task.FromCanceled(token)
					: Task.CompletedTask;
	}

	/// <summary>
	/// This will be the main place to handle received messages by this class or any derived class
	/// </summary>
	protected abstract Task<bool> ProcessReceivedAsync([NotNull] BasicDeliverEventArgs args);

	private async Task OnReceivedAsync(object _, [NotNull] BasicDeliverEventArgs args)
	{
		Logger.LogInformation($"Received from {args.Exchange}, key: {args.RoutingKey}, tag: {args.ConsumerTag}, length: {args.Body.Length}{(args.Redelivered ? " [Redelivered]" : string.Empty)}.");

		try
		{
			if (await ProcessReceivedAsync(args)) await Channel.BasicAckAsync(args.DeliveryTag, false);
			else await Channel.BasicRejectAsync(args.DeliveryTag, !args.Redelivered);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex.CollectMessages());
			await Channel.BasicNackAsync(args.DeliveryTag, false, !args.Redelivered);
		}
	}
}

public abstract class Consumer : Consumer<ConsumeSettings>
{
	/// <inheritdoc />
	protected Consumer([NotNull] IConnectionFactory factory, [NotNull] ConsumeSettings settings, ILogger logger)
		: base(factory, settings, logger)
	{
	}
}