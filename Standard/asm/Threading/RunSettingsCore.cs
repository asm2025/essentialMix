using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using JetBrains.Annotations;

namespace asm.Threading
{
	public class RunSettingsCore : ExecutableSettingsBase, IOnProcessCreated
	{
		[NotNull]
		public static RunSettingsCore Default => new RunSettingsCore();

		[NotNull]
		public static RunSettingsCore HiddenNoWindow => new RunSettingsCore {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true};

		[NotNull]
		public static RunSettingsCore AsAdminHiddenNoWindow => new RunSettingsCore {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, RunAsAdministrator = true};

		[NotNull]
		public static RunSettingsCore AsAdminHidden => new RunSettingsCore {WindowStyle = ProcessWindowStyle.Hidden, RunAsAdministrator = true};

		[NotNull]
		public static RunSettingsCore AsAdmin => new RunSettingsCore {RunAsAdministrator = true};

		private IDictionary<string, string> _environmentVariables;

		public RunSettingsCore()
		{
		}

		public ProcessWindowStyle? WindowStyle { get; set; }
		public bool RunAsAdministrator { get; set; }
		public IntPtr ErrorDialogParentHandle { get; set; }
		public bool LoadUserProfile { get; set; }
		public bool CreateNoWindow { get; set; }
		public bool UseDefaultCredentials { get; set; } = true;
		public NetworkCredential Credentials { get; set; }

		[NotNull]
		public IDictionary<string, string> EnvironmentVariables => _environmentVariables ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public Action<Process> OnCreate { get; set; }
	}
}