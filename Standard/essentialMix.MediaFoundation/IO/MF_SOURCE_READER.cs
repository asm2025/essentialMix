using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.IO
{
	[UnmanagedName("Unnamed enum")]
	public enum MF_SOURCE_READER
	{
		InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
		AllStreams = unchecked((int)0xFFFFFFFE),
		AnyStream = unchecked((int)0xFFFFFFFE),
		FirstAudioStream = unchecked((int)0xFFFFFFFD),
		FirstVideoStream = unchecked((int)0xFFFFFFFC),
		FirstSourcePhotoStream = unchecked((int)0xFFFFFFFB),
		PreferredSourceVideoStreamForPreview = unchecked((int)0xFFFFFFFA),
		PreferredSourceVideoStreamForRecord = unchecked((int)0xFFFFFFF9),
		FirstSourceIndependentPhotoStream = unchecked((int)0xFFFFFFF8),
		PreferredSourceStreamForVideoRecord = unchecked((int)0xFFFFFFF9),
		PreferredSourceStreamForPhoto = unchecked((int)0xFFFFFFF8),
		PreferredSourceStreamForAudio = unchecked((int)0xFFFFFFF7),
		MediaSource = unchecked((int)0xFFFFFFFF),
	}
}