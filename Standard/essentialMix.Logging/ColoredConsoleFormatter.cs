using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Logging.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace essentialMix.Logging
{
	/// <summary>
	/// This class is based on Microsoft's internal class: <see cref="SimpleConsoleFormatter" /> 
	/// </summary>
	public sealed class ColoredConsoleFormatter : ConsoleFormatter, IDisposable
	{
		private const int DISPOSAL_NOT_STARTED = 0;
		private const int DISPOSAL_STARTED = 1;
		private const int DISPOSAL_COMPLETE = 2;

		private const string LOG_LEVEL_PADDING = ": ";

		private static readonly ConsoleColors __noColors = new();
		private static readonly string __logEntryPadding = new(' ', LogLevel.Information.String().Length + LOG_LEVEL_PADDING.Length);
		private static readonly string __newLineWithLogEntryPadding = Environment.NewLine + __logEntryPadding;

		private readonly IDictionary<LogLevel, ConsoleColors> _colorsMap = new Dictionary<LogLevel, ConsoleColors>();

		private IDisposable _optionsReloadToken;

		// see the constants defined above for valid values
		private int _disposeStage;
		private ColoredConsoleFormatterOptions _formatterOptions;

		public ColoredConsoleFormatter()
			: this(null)
		{
		}

		/// <inheritdoc />
		public ColoredConsoleFormatter(IOptionsMonitor<ColoredConsoleFormatterOptions> options)
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
			ThrowIfDisposed();
			if (logEntry.LogLevel == LogLevel.None) return;
			
			string text = logEntry.Formatter(logEntry.State, logEntry.Exception);
			if (text == null && logEntry.Exception == null) return;
			
			LogLevel logLevel = logEntry.Exception != null && logEntry.LogLevel < LogLevel.Error
									? LogLevel.Error
									: logEntry.LogLevel;
			bool singleLine = _formatterOptions.SingleLine;
			bool includeScopes = _formatterOptions.IncludeScopes;
			int eventId = logEntry.EventId.Id;
			Exception exception = logEntry.Exception;
			string timestamp = GetTimeStamp();
			if (timestamp != null) textWriter.Write(timestamp);
			textWriter.ColorfulWrite(logLevel.String(), GetColors(logLevel));

			// Example:
			// info: ConsoleApp.Program[10]
			//       Request received

			// category and event id
			textWriter.Write(LOG_LEVEL_PADDING + logEntry.Category + '[' + eventId + "]");
			if (!singleLine) textWriter.Write(Environment.NewLine);
			if (includeScopes && scopeProvider != null) WriteScopeInformation(textWriter, scopeProvider, singleLine);
			WriteMessage(textWriter, text, singleLine);

			// Example:
			// System.InvalidOperationException
			//    at Namespace.Class.Function() in File:line X
			if (exception != null) WriteMessage(textWriter, Unwrap(exception), singleLine);
			if (singleLine) textWriter.Write(Environment.NewLine);

			static void WriteScopeInformation(TextWriter writer, IExternalScopeProvider scopeProvider, bool singleLine)
			{
				bool paddingNeeded = !singleLine;
				scopeProvider.ForEachScope((scope, state) =>
				{
					if (paddingNeeded)
					{
						paddingNeeded = false;
						state.Write(__logEntryPadding + "=> ");
					}
					else
					{
						state.Write(" => ");
					}

					state.Write(scope);
				}, writer);
				if (!paddingNeeded && !singleLine) writer.Write(Environment.NewLine);
			}

			static void WriteMessage(TextWriter writer, string text, bool singleLine)
			{
				if (string.IsNullOrEmpty(text)) return;

				if (singleLine)
				{
					writer.Write(' ');
					WriteReplacing(writer, Environment.NewLine, " ", text);
				}
				else
				{
					writer.Write(__logEntryPadding);
					WriteReplacing(writer, Environment.NewLine, __newLineWithLogEntryPadding, text);
					writer.Write(Environment.NewLine);
				}
			}

			static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
			{
				string newMessage = message.Replace(oldValue, newValue);
				writer.Write(newMessage);
			}
		}

		private void ReloadLoggerOptions(ColoredConsoleFormatterOptions options)
		{
			_formatterOptions = options;
			if (_formatterOptions.ColorBehavior == LoggerColorBehavior.Default) _formatterOptions.ColorBehavior = LoggerColorBehavior.Enabled;
			if (Console.IsOutputRedirected || _formatterOptions.ColorBehavior != LoggerColorBehavior.Enabled) _formatterOptions.ColorBehavior = LoggerColorBehavior.Disabled;
			_colorsMap.Clear();
			if (_formatterOptions.ColorBehavior == LoggerColorBehavior.Disabled) return;
			_colorsMap.Add(LogLevel.Trace, _formatterOptions.TraceColors);
			_colorsMap.Add(LogLevel.Debug, _formatterOptions.DebugColors);
			_colorsMap.Add(LogLevel.Information, _formatterOptions.InformationColors);
			_colorsMap.Add(LogLevel.Warning, _formatterOptions.WarningColors);
			_colorsMap.Add(LogLevel.Error, _formatterOptions.ErrorColors);
			_colorsMap.Add(LogLevel.Critical, _formatterOptions.CriticalColors);
		}

		private string GetTimeStamp()
		{
			string timestampFormat = _formatterOptions.TimestampFormat;
			if (string.IsNullOrEmpty(timestampFormat)) return null;

			DateTimeOffset dateTimeOffset = _formatterOptions.UseUtcTimestamp
												? DateTimeOffset.UtcNow
												: DateTimeOffset.Now;
			return dateTimeOffset.ToString(timestampFormat);
		}

		private ConsoleColors GetColors(LogLevel level)
		{
			return _formatterOptions.ColorBehavior != LoggerColorBehavior.Enabled || !_colorsMap.TryGetValue(level, out ConsoleColors colors)
						? __noColors
						: colors;
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

		private static string Unwrap(Exception exception)
		{
			switch (exception)
			{
				case null:
					return null;
				case AggregateException ae:
				{
					StringBuilder sb = new StringBuilder();

					foreach (Exception inn in ae.InnerExceptions)
					{
						Exception ex = inn;

						while (ex.InnerException != null) 
							ex = ex.InnerException;

						sb.AppendLine(ex.Message);
					}

					return sb.ToString();
				}
				default:
				{
					while (exception.InnerException != null) 
						exception = exception.InnerException;

					return exception.Message;
				}
			}
		}
	}
}