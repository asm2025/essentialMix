using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace asm.Events
{
	[Serializable]
	[ComVisible(true)]
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

		public static implicit operator T([NotNull] ReadOnlyEventArgs<T> args) { return args.Value; }
	}
}