using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Threading
{
	public class RunSettings : RunSettingsBase, IOnProcessEvents
	{
		[NotNull]
		public new static RunSettings Default => new RunSettings();

		[NotNull]
		public new static RunSettings HiddenNoWindow => new RunSettings {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true};

		[NotNull]
		public new static RunSettings AsAdminHiddenNoWindow => new RunSettings {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, RunAsAdministrator = true};

		[NotNull]
		public new static RunSettings AsAdminHidden => new RunSettings {WindowStyle = ProcessWindowStyle.Hidden, RunAsAdministrator = true};

		[NotNull]
		public new static RunSettings AsAdmin => new RunSettings {RunAsAdministrator = true};

		public RunSettings()
		{
		}

		public Action<string> OnOutput { get; set; }
		public Action<string> OnError { get; set; }
	}
}