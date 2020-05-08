using System;

namespace asm.Delegation
{
	/// <summary>
	/// Delegate for handling exceptions.
	/// </summary>
	public delegate void ExceptionHandler(object sender, Exception e);
}