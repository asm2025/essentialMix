using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ProcessThreadExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsAwaitable(this ProcessThread thisValue)
		{
			if (thisValue == null) return false;

			switch (thisValue.ThreadState)
			{
				case ThreadState.Initialized:
				case ThreadState.Ready:
				case ThreadState.Running:
				case ThreadState.Standby:
				case ThreadState.Wait:
				case ThreadState.Transition:
					return true;
				default:
					return false;
			}
		}
	}
}