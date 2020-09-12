using System;
using System.ComponentModel;
using asm.Collections;
using asm.Extensions;
using asm.Media.Commands;

namespace asm.Media.ffmpeg.Commands
{
	/*
	 * http://ffmpeg.org/ffmpeg.html
	 * https://trac.ffmpeg.org/wiki/Creating%20multiple%20outputs
	 * https://trac.ffmpeg.org/wiki/Encode/H.264
	 * https://trac.ffmpeg.org/wiki/Create%20a%20thumbnail%20image%20every%20X%20seconds%20of%20the%20video
	 * https://www.radiantmediaplayer.com/guides/working-with-ffmpeg.html
	 * https://www.virag.si/2012/01/web-video-encoding-tutorial-with-ffmpeg-0-9/
	 * https://www.virag.si/2012/01/webm-web-video-encoding-tutorial-with-ffmpeg-0-9/
	 * https://www.tecmint.com/ffmpeg-commands-for-video-audio-and-image-conversion-in-linux/
	 * https://www.labnol.org/internet/useful-ffmpeg-commands/28490/
	 * https://www.catswhocode.com/blog/19-ffmpeg-commands-for-all-needs
	 * https://superuser.com/questions/543589/information-about-ffmpeg-command-line-options
	 * https://stackoverflow.com/questions/25122740/different-between-s-and-vf-scale-in-ffmpeg-especially-in-two-pass-transc
	 * https://stackoverflow.com/questions/29350089/what-combinations-of-video-formats-and-video-and-audio-codecs-can-be-played-in-m
	 * http://diveintohtml5.info/video.html
	 * http://edoceo.com/cli/ffmpeg
	 *
	 * ffmpeg command line arguments are position sensitive - make sure you don�t mix up the order. Good rule of thumb to prevent mistakes is to keep the order of
	 * ffmpeg [input options] -i [input filename] -codec:v [video options] -codec:a [audio options] [output file options] [output filename]
	 * 
	 * ffmpeg -i STREAM.MP4 -acodec libvorbis -ac 2 -ab 96k -ar 44100 -b 345k -s 640�360 output.ogv
	 * ffmpeg -i STREAM.MP4 -acodec libvorbis -ac 2 -ab 96k -ar 44100 -b 345k -s 640�360 output.webm
	 * ffmpeg -i STREAM.MP4 -acodec libfaac -ab 96k -vcodec libx264 -vpre slower -level 21 -refs 2 -b 345k -bt 345k -threads 0 -s 640�360 output.mp4
	 * 
	 * Encoder info
	 * ffmpeg -h encoder=libvorbis
	 * ffmpeg -h encoder=libvpx
	 * ffmpeg -h encoder=libx264
	 */
	public sealed class ConvertCommand : InputOutputCommand
	{
		public ConvertCommand()
			: this(null)
		{
		}

		public ConvertCommand(int? frames)
			: base(Properties.Settings.Default.FFMPEG_NAME)
		{
			Monitor = frames.HasValue && frames.Value > 0
				? new FastProgressMonitor(frames.Value, 
				() => OnProgressStart(EventArgs.Empty),
				percent => OnProgress(new ProgressChangedEventArgs(percent, null)),
				() => OnProgressCompleted(EventArgs.Empty))
				: null;
			Arguments.Insert(0, new Property("default", "-nostdin -loglevel info -hide_banner", true, true));
			Arguments.Insert(1, new Property("startMark", "-ss {0:c}", true, true));
			Arguments.Add(new Property("endMark", "-t {0:c}", true, true));
			Arguments.Add(new Property("target", "-target {0}", true, true));
			Arguments.Add(new Property("vcodec", "-codec:v {0}", true, true));
			Arguments.Add(new Property("level", "-level {0}", true, true));
			Arguments.Add(new Property("profile", "-profile:v {0}", true, true));
			Arguments.Add(new Property("preset", "-preset {0}", true, true));
			Arguments.Add(new Property("pass", "-pass {0}", true, true));
			Arguments.Add(new Property("videoBitRate", "-b:v {0}k", true, true));
			Arguments.Add(new Property("minBitRate", "-minrate {0}k", true, true));
			Arguments.Add(new Property("maxBitRate", "-maxrate {0}k", true, true));
			Arguments.Add(new Property("quality", "-quality {0}", true, true));
			Arguments.Add(new Property("fastStart", "-movflags +faststart", true, true));
			Arguments.Add(new Property("speed", "-speed {0}", true, true));
			Arguments.Add(new Property("qmin", "-qmin {0}", true, true));
			Arguments.Add(new Property("qmax", "-qmax {0}", true, true));
			Arguments.Add(new Property("buffer", "-bufsize {0}k", true, true));
			Arguments.Add(new Property("fbs", "-r {0}", true, true));
			Arguments.Add(new Property("forceKeyFrames", "-force_key_frames \"{0}\"", true, true));
			Arguments.Add(new Property("size", null, true, true));
			Arguments.Add(new Property("ratio", "-aspect {0}", true, true));
			Arguments.Add(new Property("crop", "-filter:v \"crop={0}:{1}:{2}:{3}\"", true, true));
			Arguments.Add(new Property("threads", "-threads {0}", true, true));
			Arguments.Add(new Property("disableAudio", "-an", true, true));
			Arguments.Add(new Property("acodec", "-codec:a {0}", true, true));
			Arguments.Add(new Property("audioBitRate", "-b:a {0}k", true, true));
			Arguments.Add(new Property("sampleRate", "-ar {0}", true, true));
			Arguments.Add(new Property("forceFormat", "-f {0}", true, true));
		}

		public ConversionOptions Options { get; set; }

		protected override IProgressMonitor Monitor { get; }

		protected override string CollectArgument(IProperty property)
		{
			if (Options == null)
			{
				switch (property.Name)
				{
					case "default":
					case "overwrite":
					case "input":
					case "output":
						return base.CollectArgument(property);
					default:
						return null;
				}
			}

			if (Options.Target != Target.Default)
			{
				switch (property.Name)
				{
					case "default":
					case "overwrite":
					case "input":
					case "output":
						return base.CollectArgument(property);
					case "target":
						if (Options.TargetStandard != TargetStandard.Default)
							return string.Format((string)property.Value, string.Concat(Options.TargetStandard, '-', Options.Target));
						return string.Format((string)property.Value, Options.Target);
					default:
						return null;
				}
			}

			switch (property.Name)
			{
				case "default":
				case "overwrite":
				case "input":
				case "output":
					return base.CollectArgument(property);
				case "startMark":
					return Options.StartMark.IsValid() ? string.Format((string)property.Value, Options.StartMark) : null;
				case "endMark":
					return Options.EndMark.IsValid() ? string.Format((string)property.Value, Options.EndMark) : null;
				case "vcodec":
					return string.IsNullOrEmpty(Options.VideoCodec) ? null : string.Format((string)property.Value, Options.VideoCodec);
				case "level":
					return Options.Level.HasValue ? string.Format((string)property.Value, Options.Level) : null;
				case "profile":
					return Options.Profile.HasValue ? string.Format((string)property.Value, Options.Profile) : null;
				case "preset":
					return Options.Preset.HasValue ? string.Format((string)property.Value, Options.Preset) : null;
				case "pass":
					return Options.Pass.HasValue ? string.Format((string)property.Value, Options.Pass) : null;
				case "videoBitRate":
					if (Options.VideoBitRate.HasValue) return string.Format((string)property.Value, (int)Math.Ceiling(Options.VideoBitRate.Value / 1000.0));
					return Options.VideoSize.BitRate > 0 ? string.Format((string)property.Value, (int)Math.Ceiling(Options.VideoSize.BitRate / 1000.0)) : null;
				case "minBitRate":
					return Options.MinBitRate.HasValue ? string.Format((string)property.Value, (int)Math.Ceiling(Options.MinBitRate.Value / 1000.0)) : null;
				case "maxBitRate":
					return Options.MaxBitRate.HasValue ? string.Format((string)property.Value, (int)Math.Ceiling(Options.MaxBitRate.Value / 1000.0)) : null;
				case "quality":
					return Options.Quality != Quality.Default ? string.Format((string)property.Value, Options.Quality) : null;
				case "fastStart":
					return Options.FastStart ? (string)property.Value : null;
				case "speed":
					return Options.Speed.HasValue ? string.Format((string)property.Value, Options.Speed) : null;
				case "qmin":
					return Options.QMin.HasValue ? string.Format((string)property.Value, Options.QMin) : null;
				case "qmax":
					return Options.QMax.HasValue ? string.Format((string)property.Value, Options.QMax) : null;
				case "buffer":
					return Options.BufferSize.HasValue ? string.Format((string)property.Value, (int)Math.Ceiling(Options.BufferSize.Value / 1000.0)) : null;
				case "fbs":
					return Options.VideoFixedFPS.HasValue ? string.Format((string)property.Value, Options.VideoFixedFPS) : null;
				case "forceKeyFrames":
					return string.IsNullOrEmpty(Options.ForceKeyFramesExpression) ? null : string.Format((string)property.Value, Options.ForceKeyFramesExpression);
				case "size":
					string sizeOpt = string.Empty;
					if (Options.VideoSize.Value != VideoSizeEnum.Custom && !string.IsNullOrEmpty(Options.VideoSize.ffmpeg)) sizeOpt += string.Concat("-s ", Options.VideoSize.ffmpeg);
					sizeOpt += $" -vf scale={Options.VideoSize.Width}:{Options.VideoSize.Height}:force_original_aspect_ratio=decrease,pad={Options.VideoSize.Width}:{Options.VideoSize.Height}:(ow-iw)/2:(oh-ih)/2";
					if (Options.AspectRatio != VideoAspectRatio.Default) sizeOpt += $",setdar={Options.AspectRatio.GetDisplayName()}";
					return sizeOpt.Trim();
				case "ratio":
					return Options.AspectRatio != VideoAspectRatio.Default ? string.Format((string)property.Value, Options.AspectRatio.GetDisplayName()) : null;
				case "crop":
					return Options.Crop.IsEmpty ? null : string.Format((string)property.Value, Options.Crop.Width, Options.Crop.Height, Options.Crop.X, Options.Crop.Y);
				case "threads":
					int threads = Options.Threads ?? 0;
					if (threads < 0) threads = (int)Math.Ceiling(Environment.ProcessorCount * 0.5).NotBelow(1.0);
					return string.Format((string)property.Value, threads);
				case "disableAudio":
					return Options.DisableAudio ? (string)property.Value : null;
				case "acodec":
					return Options.DisableAudio || string.IsNullOrWhiteSpace(Options.AudioCodec) ? null : string.Format((string)property.Value, Options.AudioCodec);
				case "audioBitRate":
					return !Options.DisableAudio && Options.AudioBitRate.HasValue ? string.Format((string)property.Value, (int)Math.Ceiling(Options.AudioBitRate.Value / 1000.0)) : null;
				case "sampleRate":
					return !Options.DisableAudio && Options.AudioSampleRate != AudioSampleRateEnum.Default ? string.Format((string)property.Value, (int)Options.AudioSampleRate) : null;
				case "forceFormat":
					return string.IsNullOrEmpty(Options.ForceFormat) ? null : string.Format((string)property.Value, Options.ForceFormat);
				default:
					return null;
			}
		}
	}
}