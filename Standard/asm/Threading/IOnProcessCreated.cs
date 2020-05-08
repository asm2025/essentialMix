using System;
using System.Diagnostics;

namespace asm.Threading
{
	public interface IOnProcessCreated
	{
		Action<Process> OnCreate { get; set; }
	}
}