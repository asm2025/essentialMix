using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Windows.Threading;

public class RunOutput(string fileName)
{
    public RunOutput()
        : this(null)
    {
    }

    public string Name { get; internal set; } = fileName;

    [NotNull]
    public StringBuilder Output { get; } = new StringBuilder(Constants.BUFFER_KB);
    [NotNull]
    public StringBuilder Error { get; } = new StringBuilder(Constants.BUFFER_KB);
    [NotNull]
    public StringBuilder OutputBuilder { get; } = new StringBuilder(Constants.BUFFER_KB);
    public DateTime? StartTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? ExitCode { get; set; }
}