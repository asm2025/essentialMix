using System;
using JetBrains.Annotations;

namespace essentialMix.IO;

public interface IIOOnRead
{
	[NotNull]
	Func<char[], int, bool> OnRead { get; set; }
}