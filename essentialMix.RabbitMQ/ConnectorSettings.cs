using System;
using System.Text;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using RabbitMQFactory = RabbitMQ.Client.ConnectionFactory;

namespace essentialMix.RabbitMQ;

public class ConnectorSettings
{
	private string _prefix = string.Empty;
	private string _exchangeName = string.Empty;
	private string _route = string.Empty;
	private TimeSpan _timeout = RabbitMQFactory.DefaultConnectionTimeout;

	private string _exchangeQualifiedName;

	[NotNull]
	public virtual string Prefix
	{
		get => _prefix;
		set
		{
			_prefix = value.ToNullIfEmpty()?.Suffix('_') ?? string.Empty;
			_exchangeQualifiedName = null;
		}
	}

	[NotNull]
	public virtual string ExchangeName
	{
		get => _exchangeName;
		set
		{
			_exchangeName = value.ToNullIfEmpty() ?? string.Empty;
			_exchangeQualifiedName = null;
		}
	}

	[NotNull]
	public virtual string ExchangeQualifiedName => _exchangeQualifiedName ??= string.Concat(Prefix, ExchangeName);

	public virtual ExchangeType ExchangeType { get; set; }

	[NotNull]
	public virtual string Route
	{
		get => _route;
		set => _route = value.ToNullIfEmpty() ?? string.Empty;
	}

	public virtual bool Durable { get; set; }
	public virtual bool Exclusive { get; set; }
	public virtual bool AutoDelete { get; set; }

	public TimeSpan Timeout
	{
		get => _timeout;
		set => _timeout = value.IfLessThanOrEqual(TimeSpan.Zero, RabbitMQFactory.DefaultConnectionTimeout)
								.Within(TimeSpanHelper.FiveSeconds, TimeSpanHelper.FiveMinutes);
	}

	[NotNull]
	public virtual Encoding Encoding { get; set; } = Encoding.UTF8;

	public virtual void Apply([NotNull] ConnectorSettings settings)
	{
		Prefix = settings.Prefix;
		ExchangeName = settings.ExchangeName;
		ExchangeType = settings.ExchangeType;
		Route = settings.Route;
		Durable = settings.Durable;
		Exclusive = settings.Exclusive;
		AutoDelete = settings.AutoDelete;
		Encoding = settings.Encoding;
	}
}