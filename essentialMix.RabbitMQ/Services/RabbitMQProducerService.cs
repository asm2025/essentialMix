using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;

namespace essentialMix.RabbitMQ.Services;

public sealed class RabbitMQProducerService : Producer
{
	private static readonly Lazy<MessagePackSerializerOptions> __messagePackSerializerOptions = new Lazy<MessagePackSerializerOptions>(() => MessagePackSerializerOptions.Standard
		.WithSecurity(MessagePackSecurity.UntrustedData)
		.WithAllowAssemblyVersionMismatch(true)
		.WithOmitAssemblyVersion(true)
		.WithResolver(new TypelessContractlessStandardResolver()), LazyThreadSafetyMode.PublicationOnly);

	public RabbitMQProducerService([NotNull] IConnectionFactory factory, [NotNull] ProduceSettings settings, [NotNull] ILogger<RabbitMQProducerService> logger)
		: base(factory, settings, logger)
	{
	}

	/// <inheritdoc />
	protected override Task<byte[]> SerializeAsync<T>(T item, CancellationToken token)
	{
		return item is null
					? Task.FromResult<byte[]>(null)
					: Task.FromResult(MessagePackSerializer.Typeless.Serialize(item, __messagePackSerializerOptions.Value, token));
	}
}