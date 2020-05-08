using System;
using System.Drawing;
using JetBrains.Annotations;

namespace asm.Media.ffmpeg
{
	public class ConversionOptions
	{
		public ConversionOptions()
		{
		}

		public string Extension { get; set; }

		public TimeSpan StartMark { get; set; }
		public TimeSpan EndMark { get; set; }

		/// <summary>
		///     Predefined audio and video options for various file formats,
		///     <para>Can be used in conjunction with <see cref="TargetStandard" /> option</para>
		/// </summary>
		public Target Target { get; set; } = Target.Default;
		/// <summary>
		///     Predefined standards to be used with the <see cref="Target" /> option
		/// </summary>
		public TargetStandard TargetStandard { get; set; } = TargetStandard.Default;

		public string VideoCodec { get; set; }
		public float? Level { get; set; }
		public ConversionProfile? Profile { get; set; }
		public ConversionPreset? Preset { get; set; }
		public int? Pass { get; set; }

		[NotNull]
		public VideoSize VideoSize { get; set; } = new VideoSize();
		public int? VideoBitRate { get; set; }
		public int? MinBitRate { get; set; }
		public int? MaxBitRate { get; set; }
		public Quality Quality { get; set; }
		public bool FastStart { get; set; }
		public int? Speed { get; set; }
		public int? QMin { get; set; }
		public int? QMax { get; set; }
		public VideoAspectRatio AspectRatio { get; set; } = VideoAspectRatio.Default;
		public Rectangle Crop { get; set; }
		public int? BufferSize { get; set; }
		public double? VideoFixedFPS { get; set; }
		public string ForceKeyFramesExpression { get; set; }
		public int? Threads { get; set; }
		public bool DisableAudio { get; set; }
		public string AudioCodec { get; set; }
		public int? AudioBitRate { get; set; }
		public AudioSampleRateEnum AudioSampleRate { get; set; } = AudioSampleRateEnum.Default;
		public string ForceFormat { get; set; }

		[NotNull]
		public static ConversionOptions Mp4 =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".mp4",
				VideoCodec = "libx264",
				Preset = ConversionPreset.slow,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				AudioCodec = "aac", //libfdk_aac
				AudioBitRate = 128000
			};

		[NotNull]
		public static ConversionOptions Mp4Pass1 =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".mp4",
				VideoCodec = "libx264",
				Preset = ConversionPreset.slow,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				Pass = 1,
				DisableAudio = true,
				ForceFormat = "mp4"
			};

		[NotNull]
		public static ConversionOptions Mp4Pass2 =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".mp4",
				VideoCodec = "libx264",
				Preset = ConversionPreset.slow,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				Pass = 2,
				AudioCodec = "aac", //libfdk_aac
				AudioBitRate = 128000,
				ForceFormat = "mp4"
			};

		[NotNull]
		public static ConversionOptions WebM =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".webm",
				VideoCodec = "libvpx-vp9",
				Quality = Quality.good,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				Speed = 0,
				QMin = 10,
				QMax = 42,
				AudioCodec = "libopus",
				AudioBitRate = 128000
			};

		[NotNull]
		public static ConversionOptions WebMPass1 =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".webm",
				VideoCodec = "libvpx-vp9",
				Quality = Quality.good,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				Speed = 0,
				QMin = 10,
				QMax = 42,
				DisableAudio = true,
				Pass = 1,
				ForceFormat = "webm"
			};

		[NotNull]
		public static ConversionOptions WebMPass2 =>
			new ConversionOptions
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2), 
				Extension = ".webm",
				VideoCodec = "libvpx-vp9",
				Quality = Quality.good,
				FastStart = true,
				VideoFixedFPS = 24,
				ForceKeyFramesExpression = "expr:gte(t,n_forced*2)",
				Speed = 0,
				QMin = 10,
				QMax = 42,
				AudioCodec = "libopus",
				AudioBitRate = 128000,
				ForceFormat = "webm"
			};
	}
}