using System;
using System.Diagnostics;
using essentialMix.Threading;
using JetBrains.Annotations;

namespace essentialMix.Windows.Threading;

public class RunAndWaitForSettings : RunSettingsBase, IOnProcessCreated
{
    [NotNull]
    public new static RunAndWaitForSettings Default => new RunAndWaitForSettings();

    [NotNull]
    public new static RunAndWaitForSettings HiddenNoWindow => new RunAndWaitForSettings { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true };

    [NotNull]
    public new static RunAndWaitForSettings AsAdminHiddenNoWindow => new RunAndWaitForSettings { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, RunAsAdministrator = true };

    [NotNull]
    public new static RunAndWaitForSettings AsAdminHidden => new RunAndWaitForSettings { WindowStyle = ProcessWindowStyle.Hidden, RunAsAdministrator = true };

    [NotNull]
    public new static RunAndWaitForSettings AsAdmin => new RunAndWaitForSettings { RunAsAdministrator = true };

    public RunAndWaitForSettings()
    {
    }

    public Action<string> OnOutput { get; set; }
    public Action<string> OnError { get; set; }
}