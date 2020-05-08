using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using asm.Extensions;

namespace asm.Media.Youtube
{
	public abstract class MediaStreamInfo
	{
		public static readonly IReadOnlyDictionary<int, iTagDescriptor> TAG_MAP = new ReadOnlyDictionary<int, iTagDescriptor>(new Dictionary<int, iTagDescriptor>
		{
			// Mixed
			{5, new iTagDescriptor{Container = Container.Flv, AudioEncoding = AudioEncoding.Mp3, VideoEncoding = VideoEncoding.H263, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.Mixed}},
			{6, new iTagDescriptor{Container = Container.Flv, AudioEncoding = AudioEncoding.Mp3, VideoEncoding = VideoEncoding.H263, VideoQuality = VideoSizeEnum.CIF, TagType = TagTypeEnum.Mixed}},
			{13, new iTagDescriptor{Container = Container.Tgpp, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.Mp4V, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.Mixed}},
			{17, new iTagDescriptor{Container = Container.Tgpp, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.Mp4V, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.Mixed}},
			{18, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.Mixed}},
			{22, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.Mixed}},
			{34, new iTagDescriptor{Container = Container.Flv, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.Mixed}},
			{35, new iTagDescriptor{Container = Container.Flv, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.Mixed}},
			{36, new iTagDescriptor{Container = Container.Tgpp, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.Mp4V, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.Mixed}},
			{37, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.Mixed}},
			{38, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.HXGA, TagType = TagTypeEnum.Mixed}},
			{43, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.Mixed}},
			{44, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.Mixed}},
			{45, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.Mixed}},
			{46, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.Mixed}},
			{59, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.Mixed}},
			{78, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.Mixed}},
			{82, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.nHD, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{83, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{84, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{85, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{91, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.Mixed}},
			{92, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.Mixed}},
			{93, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.Mixed}},
			{94, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.Mixed}},
			{95, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.Mixed}},
			{96, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.Mixed}},
			{100, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.nHD, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{101, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WVGA480_720, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{102, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WXGAH, Is3D = true, TagType = TagTypeEnum.Mixed}},
			{132, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.Mixed}},
			{151, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = AudioEncoding.Aac, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.Mixed}},

			// Video-only (mp4)
			{133, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.VideoOnly}},
			{134, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.VideoOnly}},
			{135, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{136, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{137, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{138, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.UHD8K, TagType = TagTypeEnum.VideoOnly}},
			{160, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.VideoOnly}},
			{212, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{213, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{214, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{215, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{216, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{217, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{264, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHDp1440, TagType = TagTypeEnum.VideoOnly}},
			{266, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.DCI4K, TagType = TagTypeEnum.VideoOnly}},
			{298, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{299, new iTagDescriptor{Container = Container.Mp4, AudioEncoding = null, VideoEncoding = VideoEncoding.H264, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},

			// Video-only (webm)
			{167, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.VideoOnly}},
			{168, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{169, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{170, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{218, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{219, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp8, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{242, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.VideoOnly}},
			{243, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.VideoOnly}},
			{244, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{245, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{246, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{247, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{248, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{271, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHDp1440, TagType = TagTypeEnum.VideoOnly}},
			{272, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.DCI4K, TagType = TagTypeEnum.VideoOnly}},
			{278, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.VideoOnly}},
			{302, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{303, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{308, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHDp1440, TagType = TagTypeEnum.VideoOnly}},
			{313, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.DCI4K, TagType = TagTypeEnum.VideoOnly}},
			{315, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.DCI4K, TagType = TagTypeEnum.VideoOnly}},
			{330, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.QCIF, TagType = TagTypeEnum.VideoOnly}},
			{331, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FWQVGA, TagType = TagTypeEnum.VideoOnly}},
			{332, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.nHD, TagType = TagTypeEnum.VideoOnly}},
			{333, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WVGA480_720, TagType = TagTypeEnum.VideoOnly}},
			{334, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.WXGAH, TagType = TagTypeEnum.VideoOnly}},
			{335, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHD, TagType = TagTypeEnum.VideoOnly}},
			{336, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.FHDp1440, TagType = TagTypeEnum.VideoOnly}},
			{337, new iTagDescriptor{Container = Container.WebM, AudioEncoding = null, VideoEncoding = VideoEncoding.Vp9, VideoQuality = VideoSizeEnum.DCI4K, TagType = TagTypeEnum.VideoOnly}},

			// Audio-only (mp4)
			{139, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{140, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{141, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{256, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{258, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{325, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{328, new iTagDescriptor{Container = Container.M4A, AudioEncoding = AudioEncoding.Aac, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},

			// Audio-only (webm)
			{171, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{172, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Vorbis, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{249, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Opus, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{250, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Opus, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}},
			{251, new iTagDescriptor{Container = Container.WebM, AudioEncoding = AudioEncoding.Opus, VideoEncoding = null, VideoQuality = null, TagType = TagTypeEnum.AudioOnly}}
		});

		private int _iTag;
		private Container? _container;

		/// <inheritdoc />
		protected MediaStreamInfo()
		{
		}

		/// <summary>
		///     Unique tag that identifies the properties of the stream
		/// </summary>
		public virtual int iTag
		{
			get => _iTag;
			internal set
			{
				_iTag = value;
				_container = null;
				Is3D = GetVideo3D(_iTag);
				TagType = GetTagType(_iTag);
			}
		}

		public bool Is3D { get; internal set; }
		public TagTypeEnum TagType { get; internal set; }

		/// <summary>
		///     Media stream URL
		/// </summary>
		public string Url { get; internal set; }

		/// <summary>
		///     Container type
		/// </summary>
		public Container Container
		{
			get
			{
				Container? container = _container ??= GetContainer(_iTag);
				return container.Value;
			}
		}

		/// <summary>
		///     Stream content size (bytes)
		/// </summary>
		public long ContentLength { get; internal set; }

		public bool CanExtractAudio => Container == Container.Flv;

		/// <summary>
		///     Check if the given iTag is known
		/// </summary>
		public static bool IsKnown(int iTag) { return TAG_MAP.ContainsKey(iTag); }

		/// <summary>
		///     Get container type for the given iTag
		/// </summary>
		protected static Container GetContainer(int iTag)
		{
			Container? result = TAG_MAP.GetOrDefault(iTag)?.Container;
			if (!result.HasValue) throw new NotSupportedException();
			return result.Value;
		}

		/// <summary>
		///     Get encoding for the given iTag
		/// </summary>
		protected static AudioEncoding GetAudioEncoding(int iTag)
		{
			AudioEncoding? result = TAG_MAP.GetOrDefault(iTag)?.AudioEncoding;
			if (!result.HasValue) throw new NotSupportedException();
			return result.Value;
		}

		/// <summary>
		///     Get encoding for the given iTag
		/// </summary>
		protected static VideoEncoding GetVideoEncoding(int iTag)
		{
			VideoEncoding? result = TAG_MAP.GetOrDefault(iTag)?.VideoEncoding;
			if (!result.HasValue) throw new NotSupportedException();
			return result.Value;
		}

		/// <summary>
		///     Get video quality for the given iTag
		/// </summary>
		protected static VideoSizeEnum GetVideoQuality(int iTag)
		{
			VideoSizeEnum? result = TAG_MAP.GetOrDefault(iTag)?.VideoQuality;
			if (!result.HasValue) throw new NotSupportedException();
			return result.Value;
		}

		/// <summary>
		///     Returns whether video is 3D or not for the given iTag
		/// </summary>
		protected static bool GetVideo3D(int iTag)
		{
			bool? result = TAG_MAP.GetOrDefault(iTag)?.Is3D;
			if (!result.HasValue) throw new NotSupportedException();
			return result.Value;
		}

		/// <summary>
		///     Get tag type for the given iTag
		/// </summary>
		protected static TagTypeEnum GetTagType(int iTag)
		{
			TagTypeEnum? result = TAG_MAP.GetOrDefault(iTag)?.TagType;
			return result ?? TagTypeEnum.Unknown;
		}
	}
}