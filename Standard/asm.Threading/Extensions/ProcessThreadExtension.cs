using System.Collections.Generic;
using System.Diagnostics;

namespace asm.Threading.Extensions
{
	public static class ProcessThreadExtension
	{
		private static readonly HashSet<ThreadState> AWAITABLE_STATES = new HashSet<ThreadState>
		{
				ThreadState.Initialized,
				ThreadState.Ready,
				ThreadState.Running,
				ThreadState.Standby,
				ThreadState.Wait,
				ThreadState.Transition
		};

		public static bool IsAwaitable(this ProcessThread thisValue) { return thisValue != null && AWAITABLE_STATES.Contains(thisValue.ThreadState); }
	}
}