using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
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