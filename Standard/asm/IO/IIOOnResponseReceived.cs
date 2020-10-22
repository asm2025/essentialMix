using System;
using System.Net;

namespace asm.IO
{
	public interface IIoOnResponseReceived
	{
		Func<WebResponse, bool> OnResponseReceived { get; set; }
	}
}