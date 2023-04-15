using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQFactory = RabbitMQ.Client.ConnectionFactory;

namespace essentialMix.RabbitMQ;

public class FactorySettings
{
	private const string HOST_DEF = "localhost";

	private string _host = HOST_DEF;
	private int _port = AmqpTcpEndpoint.UseDefaultPort;
	private string _virtualHost = RabbitMQFactory.DefaultVHost;

	[NotNull]
	public virtual string Host
	{
		get => _host;
		set => _host = UriHelper.Trim(value);
	}

	public virtual int Port
	{
		get => _port;
		set => _port = value.Within(0, ushort.MaxValue)
							.IfEqual(0, AmqpTcpEndpoint.UseDefaultPort);
	}

	public virtual string VirtualHost
	{
		get => _virtualHost;
		set => _virtualHost = UriHelper.Trim(value);
	}

	public string UserName { get; set; } = RabbitMQFactory.DefaultUser;

	public string Password { get; set; } = RabbitMQFactory.DefaultPass;

	public virtual void Apply([NotNull] FactorySettings settings)
	{
		Host = settings.Host;
		Port = settings.Port;
		VirtualHost = settings.VirtualHost;
		UserName = settings.UserName;
		Password = settings.Password;
	}
}