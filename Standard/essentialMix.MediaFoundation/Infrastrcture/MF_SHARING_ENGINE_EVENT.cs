using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_SHARING_ENGINE_EVENT")]
	public enum MF_SHARING_ENGINE_EVENT
	{
		Disconnect = 2000,
		LocalRenderingStarted = 2001,
		LocalRenderingEnded = 2002,
		Stopped = 2003,
		Error = 2501,
	}
}