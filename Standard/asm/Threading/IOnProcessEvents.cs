using System;

namespace asm.Threading
{
	public interface IOnProcessEvents : IOnProcessCreated, IOnProcessStartAndExit
	{
		Action<string> OnOutput { get; set; }
		Action<string> OnError { get; set; }
	}
}