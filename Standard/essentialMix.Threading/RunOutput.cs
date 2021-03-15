using System;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Threading
{
	public class RunOutput
	{
		public RunOutput()
			: this(null)
		{
		}

		public RunOutput(string fileName)
		{
			Name = fileName;
		}

		public string Name { get; internal set; }
		[NotNull]
		public StringBuilder Output { get; } = new StringBuilder(Constants.BUFFER_KB);
		[NotNull]
		public StringBuilder Error { get; } = new StringBuilder(Constants.BUFFER_KB);
		[NotNull]
		public StringBuilder OutputBuilder { get; } = new StringBuilder(Constants.BUFFER_KB);
		public DateTime? StartTime { get; internal set; }
		public DateTime? ExitTime { get; internal set; }
		public int? ExitCode { get; internal set; }
 	}
}