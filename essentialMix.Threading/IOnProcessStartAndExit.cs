using System;

namespace essentialMix.Threading;

public interface IOnProcessStartAndExit : IOnProcessCreated
{
	Action<string, DateTime?> OnStart { get; set; }
	Action<string, DateTime?, int?> OnExit { get; set; }
}