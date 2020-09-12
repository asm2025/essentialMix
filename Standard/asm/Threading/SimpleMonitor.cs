using System;
using asm.Patterns.Object;

namespace asm.Threading
{
	[Serializable]
	public class SimpleMonitor : Disposable
	{
		private int _busyCount;

		public bool Busy => _busyCount > 0;

		public void Enter()
		{
			if (_busyCount == int.MaxValue) return;
			_busyCount++;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_busyCount == 0) return;
				_busyCount--;
			}
			base.Dispose(disposing);
		}
	}
}