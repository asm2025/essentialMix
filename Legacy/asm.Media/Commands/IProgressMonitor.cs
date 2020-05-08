using System;

namespace asm.Media.Commands
{
	public interface IProgressMonitor
	{
		Action OnProgressStart { get; }
		Action<int> OnProgress { get; }
		Action OnProgressCompleted { get; }
		void ProcessOutput(string data);
		void Reset();
	}
}