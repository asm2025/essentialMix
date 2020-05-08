using System;

namespace asm.Threading
{
	public abstract class LockableBase
	{
		/// <inheritdoc />
		protected LockableBase()
		{
		}

		public bool IsLocked { get; protected set; }

		protected void ThrowIfLocked()
		{
			if (!IsLocked) return;
			throw new InvalidOperationException(GetType().Name + " is locked.");
		}
	}
}