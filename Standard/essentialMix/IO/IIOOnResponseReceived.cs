using System;
using System.Net;

namespace essentialMix.IO
{
	public interface IIOOnResponseReceived
	{
		Func<WebResponse, bool> OnResponseReceived { get; set; }
	}
}