using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	[ComVisible(true)]
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