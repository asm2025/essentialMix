using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MSE_ERROR")]
	public enum MF_MSE_ERROR
	{
		NoError = 0,
		Network = 1,
		Decode = 2,
		UnknownError = 3,
	}
}