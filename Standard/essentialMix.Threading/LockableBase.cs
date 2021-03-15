using System;

namespace essentialMix.Threading
{
	public abstract class LockableBase
	{
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