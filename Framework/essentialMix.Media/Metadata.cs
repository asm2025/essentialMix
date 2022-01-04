using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace essentialMix.Media;

public class Metadata
{
	public class VideoMetadata
	{
		public VideoMetadata()
		{
		}

		public string Codec { get; internal set; }
		public string ColorModel { get; internal set; }
		[NotNull]
		public VideoSize Size { get; internal set; } = new VideoSize();
		public string SAR { get; internal set; }
		public string DAR { get; internal set; }
		public int BitRate { get; internal set; }
		public double FPS { get; internal set; }
		public dynamic StreamMetadata { get; internal set; }
	}

	public class AudioMetadata
	{
		public AudioMetadata()
		{
		}

		public string Codec { get; internal set; }
		public int SampleRate { get; internal set; }
		public string ChannelOutput { get; internal set; }
		public int BitRate { get; internal set; }
		public ICollection<dynamic> StreamMetadata { get; internal set; }
	}

	public Metadata()
	{
	}

	public string FileName { get; internal set; }
	public TimeSpan Duration { get; internal set; }
	public int FrameCount { get; internal set; }
	public int StreamCount { get; internal set; }
	public string FormatName { get; internal set; }
	public string FormatLongName { get; internal set; }
	public TimeSpan StartTime { get; internal set; }
	public long FileSize { get; internal set; }
	public int BitRate { get; internal set; }
	public int ProbeScore { get; internal set; }
	public IDictionary<string, string> Tags { get; internal set; }

	public VideoMetadata Video { get; internal set; }
	public AudioMetadata Audio { get; internal set; }
}