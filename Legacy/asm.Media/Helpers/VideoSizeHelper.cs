using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using asm.Extensions;
using asm.Helpers;
using asm.Media.ffmpeg;
using JetBrains.Annotations;

namespace asm.Media.Helpers
{
	public static class VideoSizeHelper
	{
		private const string KEY_ALLOWED_VIDEO_CONVERSIONS = "MediaToolkit:AllowedVideoConversions";
		private const string KEY_CONVERSION_RESTRICTION = "MediaToolkit:ConversionRestriction";
		private const string KEY_CONVERSION_PRESET = "MediaToolkit:ConversionPreset";
		private const string KEY_MAX_THREADS = "MediaToolkit:MaxThreads";

		private static readonly ConversionSearchRestriction CONVERSION_SEARCH_RESTRICTION;
		private static readonly ConversionPreset CONVERSION_PRESET;
		private static readonly int MAX_THREADS;
		private static readonly IDictionary<string, VideoSize> SIZES;

		static VideoSizeHelper()
		{
			SIZES = EnumHelper<VideoSizeEnum>.GetValues()
				.Where(e => e > VideoSizeEnum.Automatic)
				.Reverse()
				.Select(v => new VideoSize(v))
				.ToDictionary(v => v.DimensionsString);
			AllSizes = new ReadOnlyDictionary<string, VideoSize>(SIZES);

			IList<VideoSizeEnum> sizes = new List<VideoSizeEnum>();
			string tmpConfig = ConfigurationManager.AppSettings[KEY_ALLOWED_VIDEO_CONVERSIONS]?.Trim();

			if (!string.IsNullOrEmpty(tmpConfig))
			{
				string[] parts = tmpConfig.Split(',');

				foreach (string t in parts)
				{
					string part = t.Trim();
					if (string.IsNullOrEmpty(part)) continue;

					if (Enum.TryParse(part, true, out VideoSizeEnum sizeName))
						sizes.Add(sizeName);
					else if (AllSizes.TryGetValue(part, out VideoSize videoSize))
						sizes.Add(videoSize.Value);
				}
			}

			if (sizes.Count == 0)
			{
				sizes.Add(VideoSizeEnum.FWQVGA);//240
				sizes.Add(VideoSizeEnum.nHD);//360p
			}

			SetEnabledSizes(sizes.Distinct().ToArray());

			tmpConfig = ConfigurationManager.AppSettings[KEY_CONVERSION_RESTRICTION]?.Trim();
			if (string.IsNullOrEmpty(tmpConfig) || !Enum.TryParse(tmpConfig, true, out CONVERSION_SEARCH_RESTRICTION)) CONVERSION_SEARCH_RESTRICTION = ConversionSearchRestriction.None;

			tmpConfig = ConfigurationManager.AppSettings[KEY_CONVERSION_PRESET]?.Trim();
			if (string.IsNullOrEmpty(tmpConfig) || !Enum.TryParse(tmpConfig, true, out CONVERSION_PRESET)) CONVERSION_PRESET = ConversionPreset.slow;

			tmpConfig = ConfigurationManager.AppSettings[KEY_MAX_THREADS]?.Trim();
			MAX_THREADS = tmpConfig.To(0).Within(-1, byte.MaxValue);
		}

		public static IReadOnlyDictionary<string, VideoSize> AllSizes { get; }

		public static int MinimumVideoHeight()
		{
			return GetEnabledSizes()
				.Where(e => e.Height > 0)
				.OrderBy(e => e.Value)
				.FirstOrDefault()?.Height ?? 0;
		}

		public static int MaximumVideoHeight()
		{
			return GetEnabledSizes()
				.Where(e => e.Height > 0)
				.OrderByDescending(e => e.Value)
				.FirstOrDefault()?.Height ?? 0;
		}

		public static VideoSizeEnum MinimumVideoSize()
		{
			return GetEnabledSizes()
				.OrderBy(e => e.Value)
				.FirstOrDefault()?.Value ?? VideoSizeEnum.Empty;
		}

		public static VideoSizeEnum MaximumVideoSize()
		{
			return GetEnabledSizes()
				.OrderByDescending(e => e.Value)
				.FirstOrDefault()?.Value ?? VideoSizeEnum.Empty;
		}

		[NotNull]
		public static IEnumerable<VideoSize> GetEnabledSizes()
		{
			return SIZES.Values.Where(e => e.Enabled);
		}

		public static void SetEnabledSizes([NotNull] params VideoSizeEnum[] videoSizes)
		{
			foreach (VideoSize size in SIZES.Values)
				size.Enabled = videoSizes.Contains(size.Value);
		}

		public static int GetBitrate([NotNull] Metadata metadata)
		{
			int bitRate;

			if (metadata.BitRate > 0) bitRate = metadata.BitRate;
			else if (metadata.Video != null) bitRate = metadata.Video.Size.BitRate;
			else bitRate = 0;

			return bitRate;
		}

		public static double EstimatedFileSize([NotNull] Metadata metadata)
		{
			int bitRate = GetBitrate(metadata);
			if (bitRate <= 0) return 0;

			// Convert bitrate to Mbit/s and divide by 8
			bitRate /= 1000 * 1000 / 8;
			return bitRate * metadata.Duration.TotalSeconds;
		}

		public static void AdjustBitRate([NotNull] ConversionOptions conversion)
		{
			int defaultBitRate = conversion.VideoSize.Width * conversion.VideoSize.Height;
			int bitRate = Math.Max(defaultBitRate, conversion.VideoSize.BitRate);
			conversion.VideoBitRate = (int)(bitRate * 2.8);
			conversion.MinBitRate = (int)(bitRate * 2.0);
			conversion.MaxBitRate = (int)(bitRate * 3.5);
			conversion.BufferSize = conversion.MaxBitRate;
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<ConversionOptions> FindVideoSizeConversions(VideoSizeEnum videoSize, VideoSizeEnum? maxVideoSize = null, ConversionSearchRestriction? restriction = null, ConversionPreset? preset = null)
		{
			if (videoSize < VideoSizeEnum.SQCIF) yield break;
			maxVideoSize ??= MaximumVideoSize();
			restriction ??= CONVERSION_SEARCH_RESTRICTION;
			preset ??= CONVERSION_PRESET;

			// for more info on video size, check out: https://en.wikipedia.org/wiki/Graphics_display_resolution
			foreach (VideoSize size in SIZES.Values.Where(e => e.Enabled && e.Value <= maxVideoSize))
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2)
				if (restriction == ConversionSearchRestriction.None || restriction == ConversionSearchRestriction.MP4)
				{
					ConversionOptions mp4 = ConversionOptions.Mp4;
					mp4.AspectRatio = VideoAspectRatio.R16T9;
					mp4.Preset = preset;
					mp4.Threads = MAX_THREADS;
					mp4.VideoSize = size;
					AdjustBitRate(mp4);
					yield return mp4;
				}

				if (restriction == ConversionSearchRestriction.None || restriction == ConversionSearchRestriction.WebMedia)
				{
					ConversionOptions webM = ConversionOptions.WebM;
					webM.AspectRatio = VideoAspectRatio.R16T9;
					webM.Preset = preset;
					webM.Threads = MAX_THREADS;
					webM.VideoSize = size;
					AdjustBitRate(webM);
					yield return webM;
				}
			}
		}

		[NotNull]
		public static IEnumerable<ConversionOptions> FindVideoSizeConversions([NotNull] Metadata metadata, VideoSizeEnum? maxVideoSize = null, ConversionSearchRestriction? restriction = null, ConversionPreset? preset = null)
		{
			if (metadata.Video == null) return Array.Empty<ConversionOptions>();

			int bitRate = GetBitrate(metadata);
			return bitRate <= 0 ? Array.Empty<ConversionOptions>() : FindVideoSizeConversions(metadata.Video.Size.Value, maxVideoSize, restriction, preset);
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<ConversionOptions[]> FindVideoSize2PassConversions(VideoSizeEnum videoSize, VideoSizeEnum? maxVideoSize = null, ConversionSearchRestriction? restriction = null, ConversionPreset? preset = null)
		{
			if (videoSize < VideoSizeEnum.SQCIF) yield break;
			maxVideoSize ??= MaximumVideoSize();
			restriction ??= CONVERSION_SEARCH_RESTRICTION;
			preset ??= CONVERSION_PRESET;

			// for more info on video size, check out: https://en.wikipedia.org/wiki/Graphics_display_resolution
			foreach (VideoSize size in SIZES.Values.Where(e => e.Enabled && e.Value <= maxVideoSize))
			{
				//Fill VideoSize, MaxBitRate (=VideoSize.BitRate * 1.5~2)
				if (restriction == ConversionSearchRestriction.None || restriction == ConversionSearchRestriction.MP4)
				{
					ConversionOptions[] mp4Opt = new ConversionOptions[2];
					ConversionOptions mp4 = ConversionOptions.Mp4Pass1;
					mp4.AspectRatio = VideoAspectRatio.R16T9;
					mp4.Preset = preset;
					mp4.Threads = MAX_THREADS;
					mp4.VideoSize = size;
					AdjustBitRate(mp4);
					mp4Opt[0] = mp4;

					mp4 = ConversionOptions.Mp4Pass2;
					mp4.AspectRatio = VideoAspectRatio.R16T9;
					mp4.Preset = preset;
					mp4.Threads = MAX_THREADS;
					mp4.VideoSize = size;
					AdjustBitRate(mp4);
					mp4Opt[1] = mp4;
					yield return mp4Opt;
				}

				if (restriction == ConversionSearchRestriction.None || restriction == ConversionSearchRestriction.WebMedia)
				{
					ConversionOptions[] webmOpt = new ConversionOptions[2];
					ConversionOptions webM = ConversionOptions.WebMPass1;
					webM.AspectRatio = VideoAspectRatio.R16T9;
					webM.Preset = preset;
					webM.Threads = MAX_THREADS;
					webM.VideoSize = size;
					AdjustBitRate(webM);
					webmOpt[0] = webM;

					webM = ConversionOptions.WebMPass2;
					webM.AspectRatio = VideoAspectRatio.R16T9;
					webM.Preset = preset;
					webM.Threads = MAX_THREADS;
					webM.VideoSize = size;
					AdjustBitRate(webM);
					webmOpt[1] = webM;
					yield return webmOpt;
				}
			}
		}

		[NotNull]
		public static IEnumerable<ConversionOptions[]> FindVideoSize2PassConversions([NotNull] Metadata metadata, VideoSizeEnum? maxVideoSize = null, ConversionSearchRestriction? restriction = null, ConversionPreset? preset = null)
		{
			if (metadata.Video == null) return Array.Empty<ConversionOptions[]>();

			int bitRate = GetBitrate(metadata);
			return bitRate <= 0 ? Array.Empty<ConversionOptions[]>() : FindVideoSize2PassConversions(metadata.Video.Size.Value, maxVideoSize, restriction, preset);
		}

		[NotNull]
		public static string GetName(VideoSizeEnum videoSize, string prefix = null, string suffix = null)
		{
			string videoSizeName = videoSize == VideoSizeEnum.Automatic
				? ((int)VideoSizeEnum.Automatic).ToString()
				: videoSize.GetDisplayName();
			return string.Concat(prefix ?? string.Empty, videoSizeName, suffix ?? string.Empty);
		}
	}
}