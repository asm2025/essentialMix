using System;
using asm.Patterns.Object;

namespace asm.Threading.Other.JonSkeet.MiscUtil
{
	/// <summary>
	/// Type of buffer returned by BufferManager.
	/// </summary>
	public class Buffer : Disposable, IBuffer, IDisposable
    {
		private volatile bool _available;
		private readonly bool _clearOnDispose;

        internal Buffer(int size, bool clearOnDispose)
        {
            Bytes = new byte[size];
            _clearOnDispose = clearOnDispose;
        }

        internal bool Available
		{
			get => _available;
			set => _available = value;
		}

		public byte[] Bytes { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_clearOnDispose) Array.Clear(Bytes, 0, Bytes.Length);
				_available = true;
			}
			base.Dispose(disposing);
		}
    }
}
