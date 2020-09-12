using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace asm.Threading
{
	public class RunSettingsBase : RunSettingsCore, IOnProcessStartAndExit
	{
		[NotNull]
		public new static RunSettingsBase Default => new RunSettingsBase();

		[NotNull]
		public new static RunSettingsBase HiddenNoWindow => new RunSettingsBase {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true};

		[NotNull]
		public new static RunSettingsBase AsAdminHiddenNoWindow => new RunSettingsBase {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, RunAsAdministrator = true};

		[NotNull]
		public new static RunSettingsBase AsAdminHidden => new RunSettingsBase {WindowStyle = ProcessWindowStyle.Hidden, RunAsAdministrator = true};

		[NotNull]
		public new static RunSettingsBase AsAdmin => new RunSettingsBase {RunAsAdministrator = true};

		public RunSettingsBase()
		{
		}

		public bool RedirectInput { get; set; }
		public bool RedirectOutput { get; set; }
		public bool RedirectError { get; set; }

		public Action<string, DateTime?> OnStart { get; set; }
		public Action<string, DateTime?, int?> OnExit { get; set; }
	}
}