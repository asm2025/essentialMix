using System.Diagnostics;

namespace essentialMix.Helpers
{
	public static class DebugHelper
	{
#if DEBUG
		private const bool DBG = true;
#else
		private const bool DBG = false;
#endif

		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable 162
		public static bool DebugMode => DBG || Debugger.IsAttached;
#pragma warning restore 162
	}
}