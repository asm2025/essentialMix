using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace essentialMix.Logging
{
	public class CombinedLogger<T> : ILogger<T>
	{
		private readonly ILogger _writer;
		private ILogger _logger;

		public CombinedLogger([NotNull] ILogger writer)
			: this(writer, null)
		{
		}

		public CombinedLogger([NotNull] ILogger writer, ILogger logger)
		{
			_writer = writer;
			_logger = logger;
		}

		/// <inheritdoc />
		public bool IsEnabled(LogLevel logLevel)
		{
			return _writer.IsEnabled(logLevel) || _logger?.IsEnabled(logLevel) == true;
		}

		/// <inheritdoc />
		public IDisposable BeginScope<TState>(TState state)
		{
			return _logger?.BeginScope(state);
		}

		/// <inheritdoc />
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_writer.Log(logLevel, eventId, state, exception, formatter);
			_logger?.Log(logLevel, eventId, state, exception, formatter);
		}

		public void Combine(ILogger logger)
		{
			_logger = logger;
		}
	}
}
