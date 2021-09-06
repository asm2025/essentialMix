using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_ERR")]
	public enum MF_MEDIA_ENGINE_ERR : short
	{
		NoError = 0,
		Aborted = 1,
		Network = 2,
		Decode = 3,
		SrcNotSupported = 4,
		Encrypted = 5,
	}
}