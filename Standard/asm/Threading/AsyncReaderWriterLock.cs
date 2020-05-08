using System.Collections.Generic;
using System.Threading.Tasks;
using asm.Patterns.Object;

namespace asm.Threading
{
	public class AsyncReaderWriterLock
	{
		public class Releaser : Disposable
		{
			private readonly AsyncReaderWriterLock _toRelease;
			private readonly bool _writer;

			internal Releaser(AsyncReaderWriterLock toRelease, bool writer)
			{
				_toRelease = toRelease;
				_writer = writer;
			}

			/// <inheritdoc />
			protected override void Dispose(bool disposing)
			{
				if (disposing && _toRelease != null)
				{
					if (_writer)
						_toRelease.ReleaseWriter();
					else
						_toRelease.ReleaseReader();
				}

				base.Dispose(disposing);
			}
		}

		private readonly Task<Releaser> _readerReleaser;
		private readonly Task<Releaser> _writerReleaser;
		private readonly Queue<TaskCompletionSource<Releaser>> _waitingWriters = new Queue<TaskCompletionSource<Releaser>>();

		private TaskCompletionSource<Releaser> _waitingReader = new TaskCompletionSource<Releaser>();
		private int _readersWaiting;
		private int _status;

		/// <inheritdoc />
		public AsyncReaderWriterLock()
		{
			_readerReleaser = Task.FromResult(new Releaser(this, false));
			_writerReleaser = Task.FromResult(new Releaser(this, true));
		}

		public Task<Releaser> LockReaderAsync()
		{
			lock (_waitingWriters)
			{
				if (_status >= 0 && _waitingWriters.Count == 0)
				{
					++_status;
					return _readerReleaser;
				}

				++_readersWaiting;
				return _waitingReader.Task.ContinueWith(t => t.Result);
			}
		}

		public Task<Releaser> LockWriterAsync()
		{
			lock (_waitingWriters)
			{
				if (_status == 0)
				{
					_status = -1;
					return _writerReleaser;
				}

				TaskCompletionSource<Releaser> waiter = new TaskCompletionSource<Releaser>();
				_waitingWriters.Enqueue(waiter);
				return waiter.Task;
			}
		}

		private void ReleaseReader()
		{
			TaskCompletionSource<Releaser> toWake = null;

			lock (_waitingWriters)
			{ 
				--_status;

				if (_status == 0 && _waitingWriters.Count > 0)
				{
					_status = -1;
					toWake = _waitingWriters.Dequeue();
				}
			}

			toWake?.SetResult(new Releaser(this, true));
		}

		private void ReleaseWriter()
		{
			TaskCompletionSource<Releaser> toWake = null;
			bool toWakeIsWriter = false;

			lock (_waitingWriters)
			{
				if (_waitingWriters.Count > 0)
				{
					toWake = _waitingWriters.Dequeue();
					toWakeIsWriter = true;
				}
				else if (_readersWaiting > 0)
				{
					toWake = _waitingReader;
					_status = _readersWaiting;
					_readersWaiting = 0;
					_waitingReader = new TaskCompletionSource<Releaser>();
				}
				else _status = 0;
			}

			toWake?.SetResult(new Releaser(this, toWakeIsWriter));
		}
	}
}