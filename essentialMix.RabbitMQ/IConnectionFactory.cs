using RabbitMQ.Client;
using RabbitMQConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace essentialMix.RabbitMQ;

public interface IConnectionFactory : RabbitMQConnectionFactory
{
	public AmqpTcpEndpoint Endpoint { get; }
	bool IsAvailable();
}