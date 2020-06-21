using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Threading
{
	/// <summary>
	/// A lock token returned by a Lock method call on a SyncLock.
	/// This effectively holds the lock until it is disposed - a
	/// slight violation of the IDisposable contract, but it makes
	/// for easy use of the SyncLock global::System. This type itself
	/// is not thread-safe - LockTokens should not be shared between
	/// threads.
	/// </summary>
	public class LockToken : Disposable
	{
		/// <summary>
		/// The lock this token has been created by.
		/// </summary>
		private SyncLock _parent;

		/// <summary>
		/// Constructs a new lock token for the specified lock.
		/// </summary>
		/// <param name="parent">The internal monitor used for locking.</param>
		internal LockToken(SyncLock parent)
		{
			_parent = parent;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_parent?.Unlock();
				ObjectHelper.Dispose(ref _parent);
			}
			base.Dispose(disposing);
		}
	}
}