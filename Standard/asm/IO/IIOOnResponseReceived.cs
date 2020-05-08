using System;
using System.Net;

namespace asm.IO
{
	public interface IIOOnResponseReceived
	{
		Func<WebResponse, bool> OnResponseReceived { get; set; }
	}
}