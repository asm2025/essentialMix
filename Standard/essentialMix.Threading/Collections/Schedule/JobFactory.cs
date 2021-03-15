using System;
using essentialMix.Exceptions;
using essentialMix.Helpers;

namespace essentialMix.Threading.Collections.Schedule
{
	internal class JobFactory : IJobFactory
	{
		/// <inheritdoc />
		public IJob Create<T>(params object[] arguments)
			where T : IJob
		{
			return Create(typeof(T), arguments);
		}

		/// <inheritdoc />
		public IJob Create(Type type, params object[] arguments)
		{
			if (!typeof(IJob).IsAssignableFrom(type)) throw new TypeMismatchException(type);
			return (IJob)TypeHelper.CreateInstance(type, arguments);
		}

		/// <inheritdoc />
		public IAsyncJob CreateAsync<T>(params object[] arguments)
			where T : IAsyncJob
		{
			return CreateAsync(typeof(T), arguments);
		}

		/// <inheritdoc />
		public IAsyncJob CreateAsync(Type type, params object[] arguments)
		{
			if (!typeof(IAsyncJob).IsAssignableFrom(type)) throw new TypeMismatchException(type);
			return (IAsyncJob)TypeHelper.CreateInstance(type, arguments);
		}
	}
}