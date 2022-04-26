using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Service;

public interface IServiceBase : IDisposable
{
	[NotNull]
	IPaginated<TDto> List<TDto>(IPagination settings = null, CancellationToken token = default(CancellationToken));
	[NotNull]
	[ItemNotNull]
	Task<IPaginated<TDto>> ListAsync<TDto>(IPagination settings = null, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TKey> : IServiceBase
{
	TDetailsDto Get<TDetailsDto>([NotNull] TKey key);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey key, CancellationToken token = default(CancellationToken));

	TDetailsDto Get<TDetailsDto>([NotNull] TKey key, [NotNull] IGetSettings settings);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TKey1, TKey2> : IServiceBase
{
	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));

	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TKey1, TKey2, TKey3> : IServiceBase
{
	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));

	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TKey1, TKey2, TKey3, TKey4> : IServiceBase
{
	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));

	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TKey1, TKey2, TKey3, TKey4, TKey5> : IServiceBase
{
	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));

	TDetailsDto Get<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
	Task<TDetailsDto> GetAsync<TDetailsDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}
