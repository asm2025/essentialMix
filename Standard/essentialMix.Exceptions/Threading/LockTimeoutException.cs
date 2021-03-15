using System;
using JetBrains.Annotations;

namespace essentialMix.Exceptions.Threading
{
	/// <inheritdoc />
	/// <summary>
	/// Exception thrown when a Lock method on the SyncLock class times out.
	/// </summary>
	[Serializable]
	public class LockTimeoutException : Exception
	{
		/// <inheritdoc />
		/// <summary>
		/// Constructs an instance with the specified message.
		/// </summary>
		/// <param name="message">The message for the exception</param>
		internal LockTimeoutException (string message) : base (message)
		{
		}

		/// <inheritdoc />
		/// <summary>
		/// Constructs an instance by formatting the specified message with
		/// the given parameters.
		/// </summary>
		/// <param name="format">The message, which will be formatted with the parameters.</param>
		/// <param name="args">The parameters to use for formatting.</param>
		public LockTimeoutException ([NotNull] string format, [NotNull] params object[] args)
			: this (string.Format(format, args))
		{
		}
	}
}
