using System;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	public class ExceptionEventArgs : EventArgs
	{
		/// <inheritdoc />
		public ExceptionEventArgs([NotNull] Exception exception)
		{
			Exception = exception;
		}

		[NotNull]
		public Exception Exception { get; }
	}
}