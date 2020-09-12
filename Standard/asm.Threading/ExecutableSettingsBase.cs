using System;

namespace asm.Threading
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