using System;
using System.Diagnostics;

namespace essentialMix.Threading
{
	public interface IOnProcessCreated
	{
		Action<Process> OnCreate { get; set; }
	}
}