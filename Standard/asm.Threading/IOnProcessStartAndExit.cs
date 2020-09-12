using System;

namespace asm.Threading
{
	public interface IOnProcessStartAndExit : IOnProcessCreated
	{
		Action<string, DateTime?> OnStart { get; set; }
		Action<string, DateTime?, int?> OnExit { get; set; }
	}
}