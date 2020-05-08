namespace asm.Media.Youtube
{
	public class iTagDescriptor
	{
		internal iTagDescriptor()
		{
		}

		public Container Container { get; internal set; }

		public AudioEncoding? AudioEncoding { get; internal set; }

		public VideoEncoding? VideoEncoding { get; internal set; }

		public VideoSizeEnum? VideoQuality { get; internal set; }
		public bool Is3D { get; internal set; }
		public TagTypeEnum TagType { get; internal set; }
	}
}