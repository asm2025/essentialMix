using System;

namespace essentialMix.Threading
{
	public class ExecutableSettingsBase
	{
		public ExecutableSettingsBase()
		{
		}

		public string WorkingDirectory { get; set; }
		public IntPtr JobHandle { get; set; }
	}
}