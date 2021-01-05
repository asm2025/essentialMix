using System;
using JetBrains.Annotations;

namespace asm.Threading.Collections.Schedule
{
	public interface IJobFactory
	{
		IJob Create<T>(params object[] arguments)
			where T : IJob;

		IJob Create([NotNull] Type type, params object[] arguments);
	
		IAsyncJob CreateAsync<T>(params object[] arguments)
			where T : IAsyncJob;

		IAsyncJob CreateAsync([NotNull] Type type, params object[] arguments);
	}
}