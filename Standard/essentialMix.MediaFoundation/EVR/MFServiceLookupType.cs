using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[UnmanagedName("MF_SERVICE_LOOKUP_TYPE")]
	public enum MFServiceLookupType
	{
		Upstream = 0,
		UpstreamDirect,
		Downstream,
		DownstreamDirect,
		All,
		Global
	}
}