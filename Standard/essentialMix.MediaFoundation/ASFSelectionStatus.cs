using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[UnmanagedName("ASF_SELECTION_STATUS")]
	public enum ASFSelectionStatus
	{
		NotSelected = 0,
		CleanPointsOnly = 1,
		AllDataUnits = 2
	}
}