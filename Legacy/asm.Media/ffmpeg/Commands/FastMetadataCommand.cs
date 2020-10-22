using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using asm.Collections;
using asm.Extensions;
using asm.Helpers;

namespace asm.Media.ffmpeg.Commands
{
	public class FastMetadataCommand : InputCommand
	{
		private static readonly Regex __durationStartBitrate = new Regex(@"duration:\s*(?<duration>[^,]*),\s*start:\s*(?<start>[^,]*),\s*bitrate:\s*(?<bitrate>\d+(?:\.\d+)?)", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __frames = new Regex(@"frame=\s*(?<frames>\d+)\s+fps=\s*\d+", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __streams = new Regex(@"(?s)(?<streams>(?:stream\s+#\d+:\d+\s+->\s+#\d+:\d+)+)", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __streamVideo = new Regex(@"stream\s*#\d+:\d+(?:\([^)]+\))?:\s*Video:\s*(?<vcodec>\w+)(?:\s\(\w+\))?[^,]+,\s*(?<colors>\w+).*?,\s*(?<vsize>\d+x\d+)(?:,\s+(?<bitrate>\d+(?:\.\d+)?)\s+kb/s)?(?:,?\s+\[?SAR\s+(?<sar>\d+:\d+)\s+DAR\s+(?<dar>\d+:\d+)\]?)?,.*?\s+(?:(?<fps>\d+(?:\.\d+)?[km]?)\s+fps,)?", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __streamAudio = new Regex(@"stream\s*#\d+:\d+(?:\([^)]+\))?:\s*audio:\s*(?<acodec>\w+)(?:\s\(\w+\))?[^,]+,\s*(?<rate>\d+)\s*Hz,\s*(?<channels>\w+(?:\.\w+)?),\s*[^,]+,\s*(?<abitrate>\d+)\s*kb/s", RegexHelper.OPTIONS_I | RegexOptions.Multiline);
		private static readonly Regex __formatAndFileName = new Regex(@"input\s+#\d,\s+(?<format>\w+(?:(?:,\w+)+)?),?\s+from\s+'(?<filename>[^']+)'", RegexHelper.OPTIONS_I | RegexOptions.Multiline);

		public FastMetadataCommand()
			: base(Properties.Settings.Default.FFMPEG_NAME)
		{
			Arguments.Insert(0, new Property("default", "-nostdin -loglevel info -hide_banner", true, true));
			Arguments.Add(new Property("null", "-f null out.null", true, true));
		}

		public Metadata Metadata { get; private set; }

		protected override void OnCreate(Process process)
		{
			base.OnCreate(process);
			Metadata = null;
		}

		protected override void OnCompleted()
		{
			base.OnCompleted();

			string data = RunOutput?.OutputBuilder.ToString();

			if (!string.IsNullOrEmpty(data))
			{
				Match match = __durationStartBitrate.Match(data);

				if (match.Success)
				{
					Metadata ??= new Metadata();

					if (!TimeSpan.TryParse(match.Groups["duration"].Value, out TimeSpan timeSpan)) timeSpan = TimeSpan.Zero;
					Metadata.Duration = timeSpan;

					if (!TimeSpan.TryParse(match.Groups["start"].Value, out timeSpan)) timeSpan = TimeSpan.Zero;
					Metadata.StartTime = timeSpan;
					Metadata.BitRate = match.Groups["bitrate"].Value.To(0) * 1000;
				}

				MatchCollection frames = __frames.Matches(data);

				if (frames.Count > 0)
				{
					Metadata ??= new Metadata();

					int f = 0;

					foreach (Match frame in frames)
					{
						int n = frame.Groups["frames"].Value.To(0);
						if (n > f) f = n;
					}

					Metadata.FrameCount = f;
				}

				MatchCollection streams = __streams.Matches(data);

				if (streams.Count > 0)
				{
					Metadata ??= new Metadata();
					Metadata.StreamCount = streams.Count;
				}

				match = __streamVideo.Match(data);

				if (match.Success)
				{
					Metadata.Video ??= new Metadata.VideoMetadata();
					Metadata.Video.Codec = match.Groups["vcodec"].Value;
					Metadata.Video.ColorModel = match.Groups["colors"].Value;
					Metadata.Video.SAR = match.Groups["sar"].Value;
					Metadata.Video.DAR = match.Groups["dar"].Value;
					Metadata.Video.BitRate = match.Groups["bitrate"].Value.To(0) * 1000;
					Metadata.Video.FPS = match.Groups["fps"].Value.To(0.0d);
					string vsize = match.Groups["vsize"].Value;

					if (!string.IsNullOrEmpty(vsize))
					{
						string[] parts = vsize.Split(2, 'x');
						if (parts.Length == 2) Metadata.Video.Size.SetWidthAndHeight(parts[0].To(0), parts[1].To(0));
					}
				}

				match = __streamAudio.Match(data);

				if (match.Success)
				{
					Metadata.Audio ??= new Metadata.AudioMetadata();
					Metadata.Audio.Codec = match.Groups["acodec"].Value;
					Metadata.Audio.SampleRate = match.Groups["rate"].Value.To(0);
					Metadata.Audio.ChannelOutput = match.Groups["channels"].Value;
					Metadata.Audio.BitRate = match.Groups["abitrate"].Value.To(0) * 1000;
				}

				if (Metadata == null) return;

				match = __formatAndFileName.Match(data);

				if (match.Success)
				{
					Metadata.FormatName = match.Groups["format"].Value;
					Metadata.FileName = match.Groups["filename"].Value;
				}
			}

			Metadata.FileSize = FileHelper.GetLength(Input);
		}
	}
}