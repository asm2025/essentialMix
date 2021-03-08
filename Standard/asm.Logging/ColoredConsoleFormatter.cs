using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace asm.Logging
{
	public sealed class ColoredConsoleFormatter : ConsoleFormatter, IDisposable
	{
		private const int DISPOSAL_NOT_STARTED = 0;
		private const int DISPOSAL_STARTED = 1;
		private const int DISPOSAL_COMPLETE = 2;

		private IDisposable _optionsReloadToken;
		
		// see the constants defined above for valid values
		private int _disposeStage;
		private ColoredConsoleOptions _formatterOptions;

		public ColoredConsoleFormatter()
			: this(null)
		{
		}

		/// <inheritdoc />
		public ColoredConsoleFormatter(IOptionsMonitor<ColoredConsoleOptions> options)
			: base(nameof(ColoredConsoleFormatter))
		{
			ReloadLoggerOptions(options.CurrentValue);
			_optionsReloadToken = options.OnChange(ReloadLoggerOptions);
		}

		/// <summary>
		/// Finalizes an instance of the Disposable class.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1063", Justification = "The enforced behavior of CA1063 is not thread-safe or full-featured enough for our purposes here.")]
		~ColoredConsoleFormatter()
		{
		}

		public bool FormattingEnabled
		{
			get;
			private set;
		}

		/// <inheritdoc />
		/// <summary>
		/// Disposes of this object, if it hasn't already been disposed.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1063", Justification = "The enforced behavior of CA1063 is not thread-safe or full-featured enough for our purposes here.")]
		[SuppressMessage("Microsoft.Usage", "CA1816", Justification = "GC.SuppressFinalize is called indirectly.")]
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_STARTED, DISPOSAL_NOT_STARTED) != DISPOSAL_NOT_STARTED) return;

			if (_optionsReloadToken != null)
			{
				_optionsReloadToken.Dispose();
				_optionsReloadToken = null;
			}

			GC.SuppressFinalize(this);
			MarkAsDisposed();
		}

		/// <inheritdoc />
		public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
		{
			if (logEntry.LogLevel == LogLevel.None) return;

			string text = logEntry.Formatter(logEntry.State, logEntry.Exception);
			if (text == null && logEntry.Exception == null) return;

			if (!FormattingEnabled)
			{
				textWriter.Write(text);
				return;
			}
			
			LogLevel logLevel = logEntry.Exception != null && logEntry.LogLevel < LogLevel.Error
									? LogLevel.Error
									: logEntry.LogLevel;
			ConsoleColor? foreground
		}

		/// <summary>
		/// Verifies that this object has not been disposed, throwing an exception if it is.
		/// </summary>
		private void ThrowIfDisposed()
		{
			if (Interlocked.CompareExchange(ref _disposeStage, DISPOSAL_COMPLETE, DISPOSAL_COMPLETE) == DISPOSAL_COMPLETE) return;
			throw new ObjectDisposedException(GetType().FullName);
		}

		/// <summary>
		/// Marks this object as disposed without running any other dispose logic.
		/// </summary>
		/// <remarks>
		/// Use this method with caution. It is helpful when you have an object that can be disposed in multiple fashions, such as through a <c>CloseAsync</c> method.
		/// </remarks>
		[SuppressMessage("Microsoft.Usage", "CA1816", Justification = "This is a helper method for IDisposable.Dispose.")]
		private void MarkAsDisposed()
		{
			GC.SuppressFinalize(this);
			Interlocked.Exchange(ref _disposeStage, DISPOSAL_COMPLETE);
		}

		private void ReloadLoggerOptions(ColoredConsoleOptions options)
		{
			_formatterOptions = options;
			FormattingEnabled = !Console.IsOutputRedirected &&
								(_formatterOptions.ColorBehavior == LoggerColorBehavior.Enabled ||
								_formatterOptions.ColorBehavior == LoggerColorBehavior.Default);
		}
	}
}