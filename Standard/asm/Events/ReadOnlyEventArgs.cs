using System;

namespace asm.Events
{
	[Serializable]
	public class ReadOnlyEventArgs<T> : EventArgs
	{
		/// <inheritdoc />
		public ReadOnlyEventArgs()
		{
		}

		/// <inheritdoc />
		public ReadOnlyEventArgs(T value)
		{
			Value = value;
		}

		public virtual T Value { get; }
	}
}