using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace essentialMix.Logging.Helpers
{
	public static class LogHelper
	{
		public static ILogger Empty { get; } = LogFactoryHelper.Empty.CreateLogger(string.Empty);

		public static ILogger<T> Create<TFactory, T>()
			where TFactory : ILoggerFactory
		{
			return Create<T>(typeof(TFactory));
		}

		public static ILogger<T> Create<T>(Type factoryType = null)
		{
			return GetFactory(factoryType).CreateLogger<T>();
		}

		public static ILogger Create(Type type = null, Type factoryType = null)
		{
			string typeName = type?.FullName ?? string.Empty;
			return GetFactory(factoryType).CreateLogger(typeName);
		}

		[NotNull]
		public static ILoggerFactory GetFactory(Type factoryType = null)
		{
			return LogFactoryHelper.GetOrCreate(factoryType);
		}
	}
}