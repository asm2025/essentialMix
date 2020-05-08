using System;
using System.Runtime.InteropServices;

namespace asm.Events
{
	[Serializable]
	[ComVisible(true)]
	public class EventArgs<T> : ReadOnlyEventArgs<T>
	{
		/// <inheritdoc />
		public EventArgs()
		{
		}

		/// <inheritdoc />
		public EventArgs(T value)
			: base(value)
		{
		}

		public new virtual T Value { get; set; }
	}
}