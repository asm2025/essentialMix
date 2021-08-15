using System;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading.Other.JonSkeet.MiscUtil
{
	/// <summary>
	/// Type of buffer returned by BufferManager.
	/// </summary>
	public class Buffer : Disposable, IBuffer
    {
		private readonly bool _clearOnDispose;
		private bool _available;

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
