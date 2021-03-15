using System.Threading;
using System.Threading.Tasks;

namespace essentialMix.Threading.Collections.Schedule
{
	public interface IAsyncJob : IJob
	{
		Task ExecuteAsync(CancellationToken token);
	}
}