using System.Threading;
using System.Threading.Tasks;

namespace asm.Threading.Collections.Schedule
{
	public interface IAsyncJob : IJob
	{
		Task ExecuteAsync(CancellationToken token);
	}
}