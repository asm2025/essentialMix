using System;
using JetBrains.Annotations;

namespace asm.IO
{
	public interface IIOOnRead
	{
		[NotNull]
		Func<char[], int, bool> OnRead { get; set; }
	}
}