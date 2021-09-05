using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using essentialMix.ComponentModel.DataAnnotations;
using essentialMix.MediaFoundation.Internal;
using JetBrains.Annotations;

namespace essentialMix.MediaFoundation
{
	#region enums
	[UnmanagedName("MFAudioConstriction")]
	public enum MFAudioConstriction
	{
		Off = 0,
		C48_16 = Off + 1,
		C44_16 = C48_16 + 1,
		C14_14 = C44_16 + 1,
		Mute = C14_14 + 1
	}

	[UnmanagedName("MFVideoRotationFormat")]
	public enum MFVideoRotationFormat
	{
		R0 = 0,
		R90 = 90,
		R180 = 180,
		R270 = 270,
	}

	[UnmanagedName("MFVideo3DSampleFormat")]
	public enum MFVideo3DSampleFormat
	{
		Packed = 0,
		MultiView = 1
	}

	[UnmanagedName("MFVideo3DFormat")]
	public enum MFVideo3DFormat
	{
		BaseView = 0,
		MultiView = 1,
		PackedLeftRight = 2,
		PackedTopBottom = 3
	}

	[UnmanagedName("MFNETSOURCE_STATISTICS_IDS")]
	public enum MFNETSOURCE_STATISTICS_IDS
	{
		RECVPACKETS_ID = 0,
		LOSTPACKETS_ID,
		RESENDSREQUESTED_ID,
		RESENDSRECEIVED_ID,
		RECOVEREDBYECCPACKETS_ID,
		RECOVEREDBYRTXPACKETS_ID,
		OUTPACKETS_ID,
		RECVRATE_ID,
		AVGBANDWIDTHBPS_ID,
		BYTESRECEIVED_ID,
		PROTOCOL_ID,
		TRANSPORT_ID,
		CACHE_STATE_ID,
		LINKBANDWIDTH_ID,
		CONTENTBITRATE_ID,
		SPEEDFACTOR_ID,
		BUFFERSIZE_ID,
		BUFFERPROGRESS_ID,
		LASTBWSWITCHTS_ID,
		SEEKRANGESTART_ID,
		SEEKRANGEEND_ID,
		BUFFERINGCOUNT_ID,
		INCORRECTLYSIGNEDPACKETS_ID,
		SIGNEDSESSION_ID,
		MAXBITRATE_ID,
		RECEPTION_QUALITY_ID,
		RECOVEREDPACKETS_ID,
		VBR_ID,
		DOWNLOADPROGRESS_ID,
		UNPREDEFINEDPROTOCOLNAME_ID
	}

	[Flags]
	[UnmanagedName("MFOUTPUTATTRIBUTE_ *")]
	public enum MFOutputAttribute
	{
		None = 0,
		Digital = 0x00000001,
		NonstandardImplementation = 0x00000002,
		Video = 0x00000004,
		Compressed = 0x00000008,
		Software = 0x00000010,
		Bus = 0x00000020,
		BusImplementation = 0x0000FF00
	}

	[UnmanagedName("MFStandardVideoFormat")]
	public enum MFStandardVideoFormat
	{
		reserved = 0,
		NTSC = reserved + 1,
		PAL = NTSC + 1,
		DVD_NTSC = PAL + 1,
		DVD_PAL = DVD_NTSC + 1,
		DV_PAL = DVD_PAL + 1,
		DV_NTSC = DV_PAL + 1,
		ATSC_SD480i = DV_NTSC + 1,
		ATSC_HD1080i = ATSC_SD480i + 1,
		ATSC_HD720p = ATSC_HD1080i + 1
	}

	[UnmanagedName("MFPolicyManagerAction")]
	public enum MFPolicyManagerAction
	{
		Copy = 2,
		Export = 3,
		Extract = 4,
		Last = 7,
		No = 0,
		Play = 1,
		Reserved1 = 5,
		Reserved2 = 6,
		Reserved3 = 7
	}

	[Flags]
	[UnmanagedName("MF_CONNECT_METHOD")]
	public enum MF_CONNECT_METHOD
	{
		Direct = 0x00000000,
		AllowConverter = 0x00000001,
		AllowDecoder = 0x00000003,
		ResolveIndependentOutputTypes = 0x00000004,
		AsOptional = 0x00010000,
		AsOptionalBranch = 0x00020000,
	}

	[Flags]
	[UnmanagedName("MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS_* defines")]
	public enum MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS
	{
		None = 0x00000000,
		CrossProcess = 0x00000001,
		NoPersist = 0x00000002,
		DontAllowFormatChanges = 0x00000004
	}

	[Flags]
	[UnmanagedName("MF_TRANSCODE_ADJUST_PROFILE_FLAGS")]
	public enum MF_TRANSCODE_ADJUST_PROFILE_FLAGS
	{
		Default = 0,
		UseSourceAttributes = 1
	}

	[Flags]
	[UnmanagedName("MF_TRANSCODE_TOPOLOGYMODE_FLAGS")]
	public enum MF_TRANSCODE_TOPOLOGYMODE_FLAGS
	{
		SoftwareOnly = 0,
		HardwareAllowed = 1
	}

	[UnmanagedName("MFTOPOLOGY_HARDWARE_MODE")]
	public enum MFTOPOLOGY_HARDWARE_MODE
	{
		SoftwareOnly = 0,
		UseHardware = 1,
		UseOnlyHardware = 2,
	}

	[UnmanagedName("MF_OPM_CGMSA_PROTECTION_LEVEL")]
	public enum MF_OPM_CGMSA_PROTECTION_LEVEL
	{
		Off = 0x00,
		CopyFreely = 0x01,
		CopyNoMore = 0x02,
		CopyOneGeneration = 0x03,
		CopyNever = 0x04,
		RedistributionControlRequired = 0x08,
	}

	[UnmanagedName("MF_OPM_ACP_PROTECTION_LEVEL")]
	public enum MF_OPM_ACP_PROTECTION_LEVEL
	{
		Off = 0,
		LevelOne = 1,
		LevelTwo = 2,
		LevelThree = 3,
		ForceULong = 0x7fffffff
	}

	[UnmanagedName("MF_VIDEO_PROCESSOR_ALGORITHM_TYPE")]
	public enum MF_VIDEO_PROCESSOR_ALGORITHM_TYPE
	{
		Default = 0,
		MrfCrf444 = 1
	}

	[UnmanagedName("MF_TRANSFER_VIDEO_FRAME_FLAGS")]
	public enum MF_TRANSFER_VIDEO_FRAME_FLAGS
	{
		Default = 0,
		Stretch = 1,
		IgnorePar = 2,
	}

	public enum SAMPLE_PROTECTION_VERSION
	{
		No = 0,
		BasicLoki = 1,
		Scatter = 2,
		RC4 = 3,
		Aes128Ctr = 4,
	}

	[Flags]
	public enum MFChannelMasks
	{
		None = 0,
		Y = 0x00000001,
		R = 0x00000002,
		G = 0x00000004,
		B = 0x00000008,
		Cb = 0x00000010,
		Cr = 0x00000020,
	}

	[UnmanagedName("MF2DBuffer_LockFlags")]
	public enum MF2DBuffer_LockFlags
	{
		None,
		LockTypeMask = 0x1 | 0x2 | 0x3,
		Read = 0x1,
		Write = 0x2,
		ReadWrite = 0x3,

		ForceDWORD = 0x7FFFFFFF
	}

	[UnmanagedName("MFRATE_DIRECTION")]
	public enum MFRateDirection
	{
		Forward = 0,
		Reverse
	}

	[UnmanagedName("MFSTREAMSINK_MARKER_TYPE")]
	public enum MFStreamSinkMarkerType
	{
		Default,
		EndOfSegment,
		Tick,
		Event
	}

	[Flags]
	[UnmanagedName("MFSequencerTopologyFlags")]
	public enum MFSequencerTopologyFlags
	{
		None = 0,
		Last = 0x00000001
	}

	[Flags]
	[UnmanagedName("MFSESSION_GETFULLTOPOLOGY_FLAGS")]
	public enum MFSessionGetFullTopologyFlags
	{
		None = 0x0,
		Current = 0x1
	}

	[Flags]
	[UnmanagedName("MFSESSIONCAP_* defines")]
	public enum MFSessionCapabilities
	{
		None = 0x00000000,
		Start = 0x00000001,
		Seek = 0x00000002,
		Pause = 0x00000004,
		RateForward = 0x00000010,
		RateReverse = 0x00000020,
		DoesNotUseNetwork = 0x00000040
	}

	[Flags]
	[UnmanagedName("MFSESSION_SETTOPOLOGY_FLAGS")]
	public enum MFSessionSetTopologyFlags
	{
		None = 0x0,
		Immediate = 0x1,
		NoResolution = 0x2,
		ClearCurrent = 0x4
	}

	[UnmanagedName("MFWaveFormatExConvertFlags")]
	public enum MFWaveFormatExConvertFlags
	{
		Normal = 0,
		ForceExtensible = 1
	}

	[UnmanagedName("MF_OBJECT_TYPE")]
	public enum MFObjectType
	{
		MediaSource,
		ByteStream,
		Invalid
	}

	[Flags]
	[UnmanagedName("MF_RESOLUTION_* defines")]
	public enum MFResolution
	{
		None = 0x0,
		MediaSource = 0x00000001,
		ByteStream = 0x00000002,
		ContentDoesNotHaveToMatchExtensionOrMimeType = 0x00000010,
		KeepByteStreamAliveOnFail = 0x00000020,
		DisableLocalPlugins = 0x40,
		PluginControlPolicyApprovedOnly = 0x80,
		PluginControlPolicyWebOnly = 0x100,
		PluginControlPolicyWebOnlyEdgemode = 0x00000200,
		Read = 0x00010000,
		Write = 0x00020000,
	}

	[UnmanagedName("MF_TOPOSTATUS")]
	public enum MFTopoStatus
	{
		// MF_TOPOSTATUS_INVALID: Invalid value; will not be sent
		Invalid = 0,

		// READY: The topology has been put in place and is
		// ready to start.  All GetService calls to the Media Session will use
		// this topology.
		Ready = 100,

		// STARTED_SOURCE: The Media Session has started to read
		// and process data from the Media Source(s) in this topology.
		StartedSource = 200,

		// MF_TOPOSTATUS_DYNAMIC_CHANGED: The topology has been dynamic changed
		// due to the format change.
		DynamicChanged = 210,

		// SINK_SWITCHED: The Media Sinks in the pipeline have
		// switched from a previous topology to this topology.
		// Note that this status does not get sent for the first topology;
		// applications can assume that the sinks are playing the first
		// topology when they receive MESessionStarted.
		SinkSwitched = 300,

		// ENDED: Playback of this topology is complete.
		// Before deleting this topology, however, the application should wait
		// for either MESessionEnded or the STARTED_SOURCE status
		// on the next topology to ensure that the Media Session is no longer
		// using this topology.
		Ended = 400,
	}

	[UnmanagedName("MFSTARTUP_* defines")]
	public enum MFStartup
	{
		NoSocket = 0x1,
		Lite = 0x1,
		Full = 0
	}

	[UnmanagedName("MFCLOCK_STATE")]
	public enum MFClockState
	{
		Invalid,
		Running,
		Stopped,
		Paused
	}

	[Flags]
	[UnmanagedName("MFCLOCK_CHARACTERISTICS_FLAGS")]
	public enum MFClockCharacteristicsFlags
	{
		None = 0,
		Frequency10Mhz = 0x2,
		AlwaysRunning = 0x4,
		IsSystemClock = 0x8
	}

	[UnmanagedName("MF_TOPOLOGY_TYPE")]
	public enum MFTopologyType
	{
		Max = -1,
		OutputNode = 0,
		SourcestreamNode = 1,
		TeeNode = 3,
		TransformNode = 2
	}

	[Flags]
	[UnmanagedName("MFMEDIASOURCE_CHARACTERISTICS")]
	public enum MFMediaSourceCharacteristics
	{
		None = 0,
		IsLive = 0x1,
		CanSeek = 0x2,
		CanPause = 0x4,
		HasSlowSeek = 0x8,
		HasMultiplePresentations = 0x10,
		CanSkipForward = 0x20,
		CanSkipBackward = 0x40,
		DoesNotUseNetwork = 0x80
	}

	[Flags]
	[UnmanagedName("MEDIASINK_ defines")]
	public enum MFMediaSinkCharacteristics
	{
		None = 0,
		FixedStreams = 0x00000001,
		CannotMatchClock = 0x00000002,
		Rateless = 0x00000004,
		ClockRequired = 0x00000008,
		CanPreroll = 0x00000010,
		RequireReferenceMediaType = 0x00000020
	}

	[UnmanagedName("MF_ATTRIBUTE_SERIALIZE_OPTIONS")]
	[Flags]
	public enum MFAttributeSerializeOptions
	{
		None = 0,
		UnknownByRef = 0x00000001
	}

	[UnmanagedName("MF_FILE_ACCESSMODE")]
	public enum MFFileAccessMode
	{
		None = 0,
		Read = 1,
		Write = 2,
		ReadWrite = 3
	}

	[UnmanagedName("MF_FILE_OPENMODE")]
	public enum MFFileOpenMode
	{
		FailIfNotExist = 0,
		FailIfExist = 1,
		ResetIfExist = 2,
		AppendIfExist = 3,
		DeleteIfExist = 4
	}

	[Flags]
	[UnmanagedName("MF_FILE_FLAGS")]
	public enum MFFileFlags
	{
		None = 0,
		NoBuffering = 0x1,
		AllowWriteSharing = 0x2
	}

	[UnmanagedName("MF_URL_TRUST_STATUS")]
	public enum MFURLTrustStatus
	{
		Untrusted,
		Trusted,
		Tampered
	}

	[Flags]
	[UnmanagedName("MFPMPSESSION_CREATION_FLAGS")]
	public enum MFPMPSessionCreationFlags
	{
		None = 0,
		UnprotectedProcess = 0x1,
		InProcess = 0x2,
	}

	[Flags]
	[UnmanagedName("MFCLOCK_RELATIONAL_FLAGS")]
	public enum MFClockRelationalFlags
	{
		None = 0,
		JitterNeverAhead = 0x1
	}

	[UnmanagedName("MFSHUTDOWN_STATUS")]
	public enum MFShutdownStatus
	{
		Initiated,
		Completed
	}

	[Flags]
	[UnmanagedName("MFTIMER_FLAGS")]
	public enum MFTimeFlags
	{
		None = 0,
		Relative = 0x00000001
	}

	[UnmanagedName("MF_QUALITY_DROP_MODE")]
	public enum MFQualityDropMode
	{
		None,
		Mode1,
		Mode2,
		Mode3,
		Mode4,
		Mode5,
		NumDropModes
	}

	[UnmanagedName("MF_QUALITY_LEVEL")]
	public enum MFQualityLevel
	{
		Normal,
		NormalMinus1,
		NormalMinus2,
		NormalMinus3,
		NormalMinus4,
		NormalMinus5,
		NumQualityLevels
	}

	[Flags]
	[UnmanagedName("MF_QUALITY_ADVISE_FLAGS")]
	public enum MFQualityAdviseFlags
	{
		None = 0,
		CannotKeepUp = 0x1
	}

	[Flags]
	[UnmanagedName("MFTOPOLOGY_DXVA_MODE")]
	public enum MFTOPOLOGY_DXVA_MODE
	{
		Default = 0,
		None = 1,
		Full = 2
	}

	[UnmanagedName("MFASYNC_WORKQUEUE_TYPE")]
	public enum MFASYNC_WORKQUEUE_TYPE
	{
		StandardWorkqueue = 0,
		WindowWorkqueue = 1,
		MultiThreadedWorkqueue = 2
	}

	[UnmanagedName("AM_MPEG2Level")]
	public enum AM_MPEG2Level
	{
		None = 0,
		Low = 1,
		Main = 2,
		High1440 = 3,
		High = 4
	};

	[UnmanagedName("AM_MPEG2Profile")]
	public enum AM_MPEG2Profile
	{
		None = 0,
		Simple = 1,
		Main = 2,
		SNRScalable = 3,
		SpatiallyScalable = 4,
		High = 5
	};

	[Flags]
	[UnmanagedName("AMMPEG2_* defines")]
	public enum AMMPEG2_Flags
	{
		DoPanScan = 0x00000001,
		DVDLine21Field1 = 0x00000002,
		DVDLine21Field2 = 0x00000004,
		SourceIsLetterboxed = 0x00000008,
		FilmCameraMode = 0x00000010,
		LetterboxAnalogOut = 0x00000020,
		DSS_UserData = 0x00000040,
		DVB_UserData = 0x00000080,
		x27MhzTimebase = 0x00000100,
		WidescreenAnalogOut = 0x00000200
	}

	[Flags]
	[UnmanagedName("MFNetAuthenticationFlags")]
	public enum MFNetAuthenticationFlags
	{
		None = 0,
		Proxy = 0x00000001,
		ClearText = 0x00000002,
		LoggedOnUser = 0x00000004
	}

	[UnmanagedName("MFNetCredentialRequirements")]
	public enum MFNetCredentialRequirements
	{
		None = 0,
		RequirePrompt = 0x00000001,
		RequireSaveSelected = 0x00000002
	}

	[Flags]
	[UnmanagedName("MFNetCredentialOptions")]
	public enum MFNetCredentialOptions
	{
		None = 0,
		Save = 0x00000001,
		DontCache = 0x00000002,
		AllowClearText = 0x00000004,
	}

	[UnmanagedName("MFNETSOURCE_PROTOCOL_TYPE")]
	public enum MFNetSourceProtocolType
	{
		Undefined,
		Http,
		Rtsp,
		File,
		MultiCast
	}

	[UnmanagedName("MF_VIDEO_PROCESSOR_MIRROR")]
	public enum MF_VIDEO_PROCESSOR_MIRROR
	{
		None = 0,
		Horizontal = 1,
		Vertical = 2
	}

	[UnmanagedName("MF_VIDEO_PROCESSOR_ROTATION")]
	public enum MF_VIDEO_PROCESSOR_ROTATION
	{
		None = 0,
		Normal = 1
	}

	[Flags]
	[UnmanagedName("MFVideoFlags")]
	public enum MFVideoFlags : long
	{
		None = 0,
		PAD_TO_Mask = 0x0001 | 0x0002,
		PAD_TO_None = 0 * 0x0001,
		PAD_TO_4x3 = 1 * 0x0001,
		PAD_TO_16x9 = 2 * 0x0001,
		SrcContentHintMask = 0x0004 | 0x0008 | 0x0010,
		SrcContentHintNone = 0 * 0x0004,
		SrcContentHint16x9 = 1 * 0x0004,
		SrcContentHint235_1 = 2 * 0x0004,
		AnalogProtected = 0x0020,
		DigitallyProtected = 0x0040,
		ProgressiveContent = 0x0080,
		FieldRepeatCountMask = 0x0100 | 0x0200 | 0x0400,
		FieldRepeatCountShift = 8,
		ProgressiveSeqReset = 0x0800,
		PanScanEnabled = 0x20000,
		LowerFieldFirst = 0x40000,
		BottomUpLinearRep = 0x80000,
		DXVASurface = 0x100000,
		RenderTargetSurface = 0x400000,
		ForceQWORD = 0x7FFFFFFF
	}

	[UnmanagedName("MFVideoChromaSubsampling")]
	public enum MFVideoChromaSubsampling
	{
		Cosited = 7,
		DV_PAL = 6,
		ForceDWORD = 0x7fffffff,
		Horizontally_Cosited = 4,
		Last = 8,
		MPEG1 = 1,
		MPEG2 = 5,
		ProgressiveChroma = 8,
		Unknown = 0,
		Vertically_AlignedChromaPlanes = 1,
		Vertically_Cosited = 2
	}

	[UnmanagedName("MFVideoInterlaceMode")]
	public enum MFVideoInterlaceMode
	{
		FieldInterleavedLowerFirst = 4,
		FieldInterleavedUpperFirst = 3,
		FieldSingleLower = 6,
		FieldSingleUpper = 5,
		ForceDWORD = 0x7fffffff,
		Last = 8,
		MixedInterlaceOrProgressive = 7,
		Progressive = 2,
		Unknown = 0
	}

	[UnmanagedName("MFVideoTransferFunction")]
	public enum MFVideoTransferFunction
	{
		Unknown = 0,
		Func10 = 1,
		Func18 = 2,
		Func20 = 3,
		Func22 = 4,
		Func240M = 6,
		Func28 = 8,
		Func709 = 5,
		Func2020Const = 12,
		Func2020 = 13,
		Func26 = 14,
		ForceDWORD = 0x7fffffff,
		Last = 9,
		sRGB = 7,
		Log_100 = 9,
		Log_316 = 10,
		x709_sym = 11 // symmetric 709
	}

	[UnmanagedName("MFVideoPrimaries")]
	public enum MFVideoPrimaries
	{
		BT470_2_SysBG = 4,
		BT470_2_SysM = 3,
		BT709 = 2,
		EBU3213 = 7,
		ForceDWORD = 0x7fffffff,
		Last = 9,
		reserved = 1,
		SMPTE_C = 8,
		SMPTE170M = 5,
		SMPTE240M = 6,
		Unknown = 0,
		BT2020 = 9,
		XYZ = 10,
	}

	[UnmanagedName("MFVideoTransferMatrix")]
	public enum MFVideoTransferMatrix
	{
		BT601 = 2,
		BT709 = 1,
		ForceDWORD = 0x7fffffff,
		SMPTE240M = 3,
		Unknown = 0,
		BT2020_10 = 4,
		BT2020_12 = 5,
		Last = 6,
	}

	[UnmanagedName("MFVideoLighting")]
	public enum MFVideoLighting
	{
		Bright = 1,
		Dark = 4,
		Dim = 3,
		ForceDWORD = 0x7fffffff,
		Last = 5,
		Office = 2,
		Unknown = 0
	}

	[UnmanagedName("MFNominalRange")]
	public enum MFNominalRange
	{
		MFNominalRange_Unknown = 0,
		MFNominalRange_Normal = 1,
		MFNominalRange_Wide = 2,

		MFNominalRange_0_255 = 1,
		MFNominalRange_16_235 = 2,
		MFNominalRange_48_208 = 3,
		MFNominalRange_64_127 = 4,

		MFNominalRange_Last,
		MFNominalRange_ForceDWORD = 0x7fffffff,
	}

	[Flags]
	[UnmanagedName("MFBYTESTREAM_SEEK_FLAG_ defines")]
	public enum MFByteStreamSeekingFlags
	{
		None = 0,
		CancelPendingIO = 1
	}

	[UnmanagedName("MFBYTESTREAM_SEEK_ORIGIN")]
	public enum MFByteStreamSeekOrigin
	{
		Begin,
		Current
	}

	[Flags]
	[UnmanagedName("MFBYTESTREAM_* defines")]
	public enum MFByteStreamCapabilities
	{
		None = 0x00000000,
		IsReadable = 0x00000001,
		IsWritable = 0x00000002,
		IsSeekable = 0x00000004,
		IsRemote = 0x00000008,
		IsDirectory = 0x00000080,
		HasSlowSeek = 0x00000100,
		IsPartiallyDownloaded = 0x00000200,
		ShareWrite = 0x00000400,
		DoesNotUseNetwork = 0x00000800,
	}

	[Flags]
	[UnmanagedName("MF_MEDIATYPE_EQUAL_* defines")]
	public enum MFMediaEqual
	{
		None = 0,
		MajorTypes = 0x00000001,
		FormatTypes = 0x00000002,
		FormatData = 0x00000004,
		FormatUserData = 0x00000008
	}

	[UnmanagedName("MFASYNC_CALLBACK_QUEUE_ defines")]
	public enum MFAsyncCallbackQueue
	{
		Undefined = 0x00000000,
		Standard = 0x00000001,
		RT = 0x00000002,
		IO = 0x00000003,
		Timer = 0x00000004,
		MultiThreaded = 0x00000005,
		LongFunction = 0x00000007,
		PrivateMask = unchecked((int)0xFFFF0000),
		All = unchecked((int)0xFFFFFFFF)
	}

	[Flags]
	[UnmanagedName("MFASYNC_* defines")]
	public enum MFASync
	{
		None = 0,
		FastIOProcessingCallback = 0x00000001,
		SignalCallback = 0x00000002,
		BlockingCallback = 0x00000004,
		ReplyCallback = 0x00000008,
		LocalizeRemoteCallback = 0x00000010,
	}

	[UnmanagedName("MF_ATTRIBUTES_MATCH_TYPE")]
	public enum MFAttributesMatchType
	{
		OurItems,
		TheirItems,
		AllItems,
		InterSection,
		Smaller
	}

	[Flags]
	[UnmanagedName("MF_EVENT_FLAG_* defines")]
	public enum MFEventFlag
	{
		None = 0,
		NoWait = 0x00000001
	}

	[UnmanagedName("MF_ATTRIBUTE_TYPE")]
	public enum MFAttributeType
	{
		None = 0x0,
		Blob = 0x1011,
		Double = 0x5,
		Guid = 0x48,
		IUnknown = 13,
		String = 0x1f,
		Uint32 = 0x13,
		Uint64 = 0x15
	}

	[UnmanagedName("unnamed enum")]
	public enum MediaEventType
	{
		MEUnknown = 0,
		MEError = 1,
		MEExtendedType = 2,
		MENonFatalError = 3,
		MEGenericV1Anchor = MENonFatalError,
		MESessionUnknown = 100,
		MESessionTopologySet = 101,
		MESessionTopologiesCleared = 102,
		MESessionStarted = 103,
		MESessionPaused = 104,
		MESessionStopped = 105,
		MESessionClosed = 106,
		MESessionEnded = 107,
		MESessionRateChanged = 108,
		MESessionScrubSampleComplete = 109,
		MESessionCapabilitiesChanged = 110,
		MESessionTopologyStatus = 111,
		MESessionNotifyPresentationTime = 112,
		MENewPresentation = 113,
		MELicenseAcquisitionStart = 114,
		MELicenseAcquisitionCompleted = 115,
		MEIndividualizationStart = 116,
		MEIndividualizationCompleted = 117,
		MEEnablerProgress = 118,
		MEEnablerCompleted = 119,
		MEPolicyError = 120,
		MEPolicyReport = 121,
		MEBufferingStarted = 122,
		MEBufferingStopped = 123,
		MEConnectStart = 124,
		MEConnectEnd = 125,
		MEReconnectStart = 126,
		MEReconnectEnd = 127,
		MERendererEvent = 128,
		MESessionStreamSinkFormatChanged = 129,
		MESessionV1Anchor = MESessionStreamSinkFormatChanged,
		MESourceUnknown = 200,
		MESourceStarted = 201,
		MEStreamStarted = 202,
		MESourceSeeked = 203,
		MEStreamSeeked = 204,
		MENewStream = 205,
		MEUpdatedStream = 206,
		MESourceStopped = 207,
		MEStreamStopped = 208,
		MESourcePaused = 209,
		MEStreamPaused = 210,
		MEEndOfPresentation = 211,
		MEEndOfStream = 212,
		MEMediaSample = 213,
		MEStreamTick = 214,
		MEStreamThinMode = 215,
		MEStreamFormatChanged = 216,
		MESourceRateChanged = 217,
		MEEndOfPresentationSegment = 218,
		MESourceCharacteristicsChanged = 219,
		MESourceRateChangeRequested = 220,
		MESourceMetadataChanged = 221,
		MESequencerSourceTopologyUpdated = 222,
		MESourceV1Anchor = MESequencerSourceTopologyUpdated,

		MESinkUnknown = 300,
		MEStreamSinkStarted = 301,
		MEStreamSinkStopped = 302,
		MEStreamSinkPaused = 303,
		MEStreamSinkRateChanged = 304,
		MEStreamSinkRequestSample = 305,
		MEStreamSinkMarker = 306,
		MEStreamSinkPrerolled = 307,
		MEStreamSinkScrubSampleComplete = 308,
		MEStreamSinkFormatChanged = 309,
		MEStreamSinkDeviceChanged = 310,
		MEQualityNotify = 311,
		MESinkInvalidated = 312,
		MEAudioSessionNameChanged = 313,
		MEAudioSessionVolumeChanged = 314,
		MEAudioSessionDeviceRemoved = 315,
		MEAudioSessionServerShutdown = 316,
		MEAudioSessionGroupingParamChanged = 317,
		MEAudioSessionIconChanged = 318,
		MEAudioSessionFormatChanged = 319,
		MEAudioSessionDisconnected = 320,
		MEAudioSessionExclusiveModeOverride = 321,
		MESinkV1Anchor = MEAudioSessionExclusiveModeOverride,

		MECaptureAudioSessionVolumeChanged = 322,
		MECaptureAudioSessionDeviceRemoved = 323,
		MECaptureAudioSessionFormatChanged = 324,
		MECaptureAudioSessionDisconnected = 325,
		MECaptureAudioSessionExclusiveModeOverride = 326,
		MECaptureAudioSessionServerShutdown = 327,
		MESinkV2Anchor = MECaptureAudioSessionServerShutdown,

		METrustUnknown = 400,
		MEPolicyChanged = 401,
		MEContentProtectionMessage = 402,
		MEPolicySet = 403,
		METrustV1Anchor = MEPolicySet,

		MEWMDRMLicenseBackupCompleted = 500,
		MEWMDRMLicenseBackupProgress = 501,
		MEWMDRMLicenseRestoreCompleted = 502,
		MEWMDRMLicenseRestoreProgress = 503,
		MEWMDRMLicenseAcquisitionCompleted = 506,
		MEWMDRMIndividualizationCompleted = 508,
		MEWMDRMIndividualizationProgress = 513,
		MEWMDRMProximityCompleted = 514,
		MEWMDRMLicenseStoreCleaned = 515,
		MEWMDRMRevocationDownloadCompleted = 516,
		MEWMDRMV1Anchor = MEWMDRMRevocationDownloadCompleted,

		METransformUnknown = 600,
		METransformNeedInput,
		METransformHaveOutput,
		METransformDrainComplete,
		METransformMarker,
		METransformInputStreamStateChanged,
		MEByteStreamCharacteristicsChanged = 700,
		MEVideoCaptureDeviceRemoved = 800,
		MEVideoCaptureDevicePreempted = 801,
		MEStreamSinkFormatInvalidated = 802,
		MEEncodingParameters = 803,
		MEContentProtectionMetadata = 900,
		MEDeviceThermalStateChanged = 950,
		MEReservedMax = 10000
	}

	[UnmanagedName("MF_Plugin_Type")]
	public enum MFPluginType
	{
		MFT = 0,
		MediaSource = 1,
		MFT_MatchOutputType = 2,
		Other = unchecked((int)0xffffffff),
	}

	[UnmanagedName("MF_PLUGIN_CONTROL_POLICY")]
	public enum MF_PLUGIN_CONTROL_POLICY
	{
		UseAllPlugins = 0,
		UseApprovedPlugins = 1,
		UseWebPlugins = 2,
		UseWebPluginsEdgemode = 3,
	}

	[UnmanagedName("AMINTERLACE_*"), Flags]
	public enum AMInterlace
	{
		None = 0,
		IsInterlaced = 0x00000001,
		OneFieldPerSample = 0x00000002,
		Field1First = 0x00000004,
		Unused = 0x00000008,
		FieldPatternMask = 0x00000030,
		FieldPatField1Only = 0x00000000,
		FieldPatField2Only = 0x00000010,
		FieldPatBothRegular = 0x00000020,
		FieldPatBothIrregular = 0x00000030,
		DisplayModeMask = 0x000000c0,
		DisplayModeBobOnly = 0x00000000,
		DisplayModeWeaveOnly = 0x00000040,
		DisplayModeBobOrWeave = 0x00000080,
	}

	[UnmanagedName("AMCOPYPROTECT_*")]
	public enum AMCopyProtect
	{
		None = 0,
		RestrictDuplication = 0x00000001
	}

	[UnmanagedName("From AMCONTROL_*"), Flags]
	public enum AMControl
	{
		None = 0,
		Used = 0x00000001,
		PadTo4x3 = 0x00000002,
		PadTo16x9 = 0x00000004,
	}

	[Flags, UnmanagedName("SPEAKER_* defines")]
	public enum WaveMask
	{
		None = 0x0,
		FrontLeft = 0x1,
		FrontRight = 0x2,
		FrontCenter = 0x4,
		LowFrequency = 0x8,
		BackLeft = 0x10,
		BackRight = 0x20,
		FrontLeftOfCenter = 0x40,
		FrontRightOfCenter = 0x80,
		BackCenter = 0x100,
		SideLeft = 0x200,
		SideRight = 0x400,
		TopCenter = 0x800,
		TopFrontLeft = 0x1000,
		TopFrontCenter = 0x2000,
		TopFrontRight = 0x4000,
		TopBackLeft = 0x8000,
		TopBackCenter = 0x10000,
		TopBackRight = 0x20000
	}

	[UnmanagedName("MFVideoPadFlags")]
	public enum MFVideoPadFlags
	{
		PAD_TO_None = 0,
		PAD_TO_4x3 = 1,
		PAD_TO_16x9 = 2
	}

	[UnmanagedName("MFVideoSrcContentHintFlags")]
	public enum MFVideoSrcContentHintFlags
	{
		None = 0,
		F16x9 = 1,
		F235_1 = 2
	}

	[Flags, UnmanagedName("STGC")]
	public enum STGC
	{
		Default = 0,
		Overwrite = 1,
		OnlyIfCurrent = 2,
		DangerouslyCommitMerelyToDiskCache = 4,
		Consolidate = 8
	}

	[UnmanagedName("STATFLAG")]
	public enum STATFLAG
	{
		Default = 0,
		NoName = 1,
		NoOpen = 2
	}

	[UnmanagedName("STGTY")]
	public enum STGTY
	{
		None = 0,
		Storage = 1,
		Stream = 2,
		LockBytes = 3,
		Property = 4
	}

	public enum eAVEncCommonRateControlMode
	{
		eAVEncCommonRateControlMode_CBR = 0,
		eAVEncCommonRateControlMode_PeakConstrainedVBR = 1,
		eAVEncCommonRateControlMode_UnconstrainedVBR = 2,
		eAVEncCommonRateControlMode_Quality = 3,
		eAVEncCommonRateControlMode_LowDelayVBR = 4,
		eAVEncCommonRateControlMode_GlobalVBR = 5,
		eAVEncCommonRateControlMode_GlobalLowDelayVBR = 6
	}

	public enum eAVEncMPVLevel
	{
		eAVEncMPVLevel_Low = 1,
		eAVEncMPVLevel_Main = 2,
		eAVEncMPVLevel_High1440 = 3,
		eAVEncMPVLevel_High = 4,
	}

	public enum eAVEncH263VProfile
	{
		eAVEncH263VProfile_Base = 0,
		eAVEncH263VProfile_CompatibilityV2 = 1,
		eAVEncH263VProfile_CompatibilityV1 = 2,
		eAVEncH263VProfile_WirelessV2 = 3,
		eAVEncH263VProfile_WirelessV3 = 4,
		eAVEncH263VProfile_HighCompression = 5,
		eAVEncH263VProfile_Internet = 6,
		eAVEncH263VProfile_Interlace = 7,
		eAVEncH263VProfile_HighLatency = 8
	};

	public enum eAVEncH264VProfile
	{
		eAVEncH264VProfile_unknown = 0,
		eAVEncH264VProfile_Simple = 66,
		eAVEncH264VProfile_Base = 66,
		eAVEncH264VProfile_Main = 77,
		eAVEncH264VProfile_High = 100,
		eAVEncH264VProfile_422 = 122,
		eAVEncH264VProfile_High10 = 110,
		eAVEncH264VProfile_444 = 244,
		eAVEncH264VProfile_Extended = 88,

		// UVC 1.2 H.264 extension
		eAVEncH264VProfile_ScalableBase = 83,
		eAVEncH264VProfile_ScalableHigh = 86,
		eAVEncH264VProfile_MultiviewHigh = 118,
		eAVEncH264VProfile_StereoHigh = 128,
		eAVEncH264VProfile_ConstrainedBase = 256,
		eAVEncH264VProfile_UCConstrainedHigh = 257,
		eAVEncH264VProfile_UCScalableConstrainedBase = 258,
		eAVEncH264VProfile_UCScalableConstrainedHigh = 259
	};

	public enum eAVEncH265VProfile
	{
		eAVEncH265VProfile_unknown = 0,
		eAVEncH265VProfile_Main_420_8 = 1,
		eAVEncH265VProfile_Main_420_10 = 2,
		eAVEncH265VProfile_Main_420_12 = 3,
		eAVEncH265VProfile_Main_422_10 = 4,
		eAVEncH265VProfile_Main_422_12 = 5,
		eAVEncH265VProfile_Main_444_8 = 6,
		eAVEncH265VProfile_Main_444_10 = 7,
		eAVEncH265VProfile_Main_444_12 = 8,
		eAVEncH265VProfile_Monochrome_12 = 9,
		eAVEncH265VProfile_Monochrome_16 = 10,
		eAVEncH265VProfile_MainIntra_420_8 = 11,
		eAVEncH265VProfile_MainIntra_420_10 = 12,
		eAVEncH265VProfile_MainIntra_420_12 = 13,
		eAVEncH265VProfile_MainIntra_422_10 = 14,
		eAVEncH265VProfile_MainIntra_422_12 = 15,
		eAVEncH265VProfile_MainIntra_444_8 = 16,
		eAVEncH265VProfile_MainIntra_444_10 = 17,
		eAVEncH265VProfile_MainIntra_444_12 = 18,
		eAVEncH265VProfile_MainIntra_444_16 = 19,
		eAVEncH265VProfile_MainStill_420_8 = 20,
		eAVEncH265VProfile_MainStill_444_8 = 21,
		eAVEncH265VProfile_MainStill_444_16 = 22
	};
	#endregion

	#region Interfaces
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3C9B2EB9-86D5-4514-A394-F56664F9F0D8")]
	public interface IMFMediaSourceEx : IMFMediaSource
	{
		#region IMFMediaEventGenerator methods

		[PreserveSig]
		new int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
		);

		[PreserveSig]
		new int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
		);

		[PreserveSig]
		new int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
		);

		[PreserveSig]
		new int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
		);

		#endregion

		#region IMFMediaSource methods

		[PreserveSig]
		new int GetCharacteristics(
			out MFMediaSourceCharacteristics pdwCharacteristics
		);

		[PreserveSig]
		new int CreatePresentationDescriptor(
			out IMFPresentationDescriptor ppPresentationDescriptor
		);

		[PreserveSig]
		new int Start([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid pguidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarStartPosition
		);

		[PreserveSig]
		new int Stop();

		[PreserveSig]
		new int Pause();

		[PreserveSig]
		new int Shutdown();

		#endregion

		[PreserveSig]
		int GetSourceAttributes(
			out IMFAttributes ppAttributes
		);

		[PreserveSig]
		int GetStreamAttributes(
			int dwStreamIdentifier,
			out IMFAttributes ppAttributes
		);

		[PreserveSig]
		int SetD3DManager(
			[MarshalAs(UnmanagedType.IUnknown)] object pManager
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("20bc074b-7a8d-4609-8c3b-64a0a3b5d7ce")]
	public interface IMFDXGIDeviceManagerSource
	{
		[PreserveSig]
		int GetManager(
			out IMFDXGIDeviceManager ppManager
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("26AFEA53-D9ED-42B5-AB80-E64F9EE34779")]
	public interface IMFSeekInfo
	{
		[PreserveSig]
		int GetNearestKeyFrames([In][MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarStartPosition,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSeekInfo.GetNearestKeyFrames.0", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvarPreviousKeyFrame,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSeekInfo.GetNearestKeyFrames.1", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pvarNextKeyFrame
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("64976BFA-FB61-4041-9069-8C9A5F659BEB")]
	public interface IMFByteStreamTimeSeek
	{
		[PreserveSig]
		int IsTimeSeekSupported([Out][MarshalAs(UnmanagedType.Bool)] out bool pfTimeSeekIsSupported
		);

		[PreserveSig]
		int TimeSeek(
			long qwTimePosition
		);

		[PreserveSig]
		int GetTimeSeekResult(
			out long pqwStartTime,
			out long pqwStopTime,
			out long pqwDuration
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("84d2054a-3aa1-4728-a3b0-440a418cf49c")]
	public interface IMFPMPHostApp
	{
		[PreserveSig]
		int LockProcess();

		[PreserveSig]
		int UnlockProcess();

		[PreserveSig]
		int ActivateClassById([In][MarshalAs(UnmanagedType.LPWStr)] string id,
			IStream pStream,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("c004f646-be2c-48f3-93a2-a0983eba1108")]
	public interface IMFPMPClientApp
	{
		[PreserveSig]
		int SetPMPHost(
			IMFPMPHostApp pPMPHost
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("ef5dc845-f0d9-4ec9-b00c-cb5183d38434")]
	public interface IMFProtectedEnvironmentAccess
	{
		[PreserveSig]
		int Call(
			int inputLength,
			IntPtr input,
			int outputLength,
			IntPtr output
		);

		[PreserveSig]
		int ReadGRL(
			out int outputLength,
			out IntPtr output
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4a724bca-ff6a-4c07-8e0d-7a358421cf06")]
	public interface IMFSignedLibrary
	{
		[PreserveSig]
		int GetProcedureAddress([In][MarshalAs(UnmanagedType.LPWStr)] string name,
			out IntPtr address
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("fff4af3a-1fc1-4ef9-a29b-d26c49e2f31a")]
	public interface IMFSystemId
	{
		[PreserveSig]
		int GetData(
			out int size,
			out IntPtr data
		);

		[PreserveSig]
		int Setup(
			int stage,
			int cbIn,
			IntPtr pbIn,
			out int pcbOut,
			out IntPtr ppbOut
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D19F8E98-B126-4446-890C-5DCB7AD71453")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFInputTrustAuthority
	{
		[PreserveSig]
		int GetDecrypter([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);

		[PreserveSig]
		int RequestAccess(
			[In] MFPolicyManagerAction Action,
			[MarshalAs(UnmanagedType.Interface)] out IMFActivate ppContentEnablerActivate
			);

		[PreserveSig]
		int GetPolicy(
			[In] MFPolicyManagerAction Action,
			[MarshalAs(UnmanagedType.Interface)] out IMFOutputPolicy ppPolicy
			);

		[PreserveSig]
		int BindAccess(
			[In] ref MFInputTrustAuthorityAccessParams pParam
			);

		[PreserveSig]
		int UpdateAccess(
			[In] ref MFInputTrustAuthorityAccessParams pParam
			);

		[PreserveSig]
		int Reset();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("0E1D600A-C9F3-442D-8C51-A42D2D49452F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSourcePresentationProvider
	{
		[PreserveSig]
		int ForceEndOfPresentation([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7F00F10A-DAED-41AF-AB26-5FDFA4DFBA3C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFOutputPolicy : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFOutputPolicy.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFOutputPolicy.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GenerateRequiredSchemas(
			[In] MFOutputAttribute dwAttributes,
			[In][MarshalAs(UnmanagedType.Struct)]
			Guid guidOutputSubType,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid rgGuidProtectionSchemasSupported,
			[In] int cProtectionSchemasSupported,
			[MarshalAs(UnmanagedType.Interface)] out IMFCollection ppRequiredProtectionSchemas
			);

		[PreserveSig]
		int GetOriginatorID(
			out Guid pguidOriginatorID
			);

		[PreserveSig]
		int GetMinimumGRLVersion(
			out int pdwMinimumGRLVersion
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("7BE0FC5B-ABD9-44FB-A5C8-F50136E71599")]
	public interface IMFOutputSchema : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFOutputSchema.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFOutputSchema.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetSchemaType(
			out Guid pguidSchemaType
			);

		[PreserveSig]
		int GetConfigurationData(
			out int pdwVal
			);

		[PreserveSig]
		int GetOriginatorID(
			out Guid pguidOriginatorID
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D19F8E94-B126-4446-890C-5DCB7AD71453")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFOutputTrustAuthority
	{
		[PreserveSig]
		int GetAction(
			out MFPolicyManagerAction pAction
			);

		[PreserveSig]
		int SetPolicy([In][MarshalAs(UnmanagedType.Interface)] ref IMFOutputPolicy ppPolicy,
			[In] int nPolicy,
			[Out] IntPtr ppbTicket,
			out int pcbTicket
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("994E23AD-1CC2-493C-B9FA-46F1CB040FA4")]
	public interface IMFRemoteProxy
	{
		[PreserveSig]
		int GetRemoteObject([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);

		[PreserveSig]
		int GetRemoteHost([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8E36395F-C7B9-43C4-A54D-512B4AF63C95")]
	public interface IMFSampleProtection
	{
		[PreserveSig]
		int GetInputProtectionVersion(
			out int pdwVersion
			);

		[PreserveSig]
		int GetOutputProtectionVersion(
			out int pdwVersion
			);

		[PreserveSig]
		int GetProtectionCertificate(
			[In] int dwVersion,
			[Out] IntPtr ppCert,
			out int pcbCert
			);

		[PreserveSig]
		int InitOutputProtection(
			[In] int dwVersion,
			[In] int dwOutputId,
			[In] ref byte pbCert,
			[In] int cbCert,
			[Out] IntPtr ppbSeed,
			out int pcbSeed
			);

		[PreserveSig]
		int InitInputProtection(
			[In] int dwVersion,
			[In] int dwInputId,
			[In] ref byte pbSeed,
			[In] int cbSeed
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D0AE555D-3B12-4D97-B060-0990BC5AEB67")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSecureChannel
	{
		[PreserveSig]
		int GetCertificate(
			[Out] IntPtr ppCert,
			out int pcbCert
			);

		[PreserveSig]
		int SetupSession(
			[In] ref byte pbEncryptedSessionKey,
			[In] int cbSessionKey
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("542612C4-A1B8-4632-B521-DE11EA64A0B0")]
	public interface IMFTrustedInput
	{
		[PreserveSig]
		int GetInputTrustAuthority(
			[In] int dwStreamID,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppunkObject
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D19F8E95-B126-4446-890C-5DCB7AD71453")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTrustedOutput
	{
		[PreserveSig]
		int GetOutputTrustAuthorityCount(
			out int pcOutputTrustAuthorities
			);

		[PreserveSig]
		int GetOutputTrustAuthorityByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFOutputTrustAuthority ppauthority
			);

		[PreserveSig]
		int IsFinal(
			[MarshalAs(UnmanagedType.Bool)] out bool pfIsFinal
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("61F7D887-1230-4A8B-AEBA-8AD434D1A64D")]
	public interface IMFSSLCertificateManager
	{
		[PreserveSig]
		int GetClientCertificate([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			[Out] out IntPtr ppbData,
			out int pcbData
		);

		[PreserveSig]
		int BeginGetClientCertificate([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
		);

		[PreserveSig]
		int EndGetClientCertificate(
			IMFAsyncResult pResult,
			[Out] IntPtr ppbData,
			out int pcbData
		);

		[PreserveSig]
		int GetCertificatePolicy([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			[MarshalAs(UnmanagedType.Bool)] out bool pfOverrideAutomaticCheck,
			[MarshalAs(UnmanagedType.Bool)] out bool pfClientCertificateAvailable
		);

		[PreserveSig]
		int OnServerCertificate([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			[In] IntPtr pbData,
			[In] int cbData,
			[MarshalAs(UnmanagedType.Bool)] out bool pfIsGood
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("380b9af9-a85b-4e78-a2af-ea5ce645c6b4")]
	public interface IMFMediaStreamSourceSampleRequest
	{
		[PreserveSig]
		int SetSample(
			IMFSample value
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3978AA1A-6D5B-4B7F-A340-90899189AE34")]
	public interface IMFVideoSampleAllocatorNotifyEx : IMFVideoSampleAllocatorNotify
	{
		#region IMFVideoSampleAllocatorNotify

		[PreserveSig]
		new int NotifyRelease();

		#endregion

		[PreserveSig]
		int NotifyPrune(IMFSample pSample);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("BDE633D3-E1DC-4a7f-A693-BBAE399C4A20")]
	public interface IMFVideoProcessorControl2 : IMFVideoProcessorControl
	{
		#region IMFVideoProcessorControl

		[PreserveSig]
		new int SetBorderColor(
			MFARGB pBorderColor
		);

		[PreserveSig]
		new int SetSourceRectangle(
			MFRect pSrcRect
		);

		[PreserveSig]
		new int SetDestinationRectangle(
			MFRect pDstRect
		);

		[PreserveSig]
		new int SetMirror(
			MF_VIDEO_PROCESSOR_MIRROR eMirror
		);

		[PreserveSig]
		new int SetRotation(
			MF_VIDEO_PROCESSOR_ROTATION eRotation
		);

		[PreserveSig]
		new int SetConstrictionSize(
			[In] MFSize pConstrictionSize
		);

		#endregion

		[PreserveSig]
		int SetRotationOverride(
			int uiRotation
			);

		[PreserveSig]
		int EnableHardwareEffects([In][MarshalAs(UnmanagedType.Bool)] bool fEnabled
			);

		[PreserveSig]
		int GetSupportedHardwareEffects(
			out int puiSupport
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("E6257174-A060-4C9A-A088-3B1B471CAD28")]
	public interface IMFContentProtectionDevice
	{
		[PreserveSig]
		int InvokeFunction(
			int FunctionId,
			int InputBufferByteCount,
			byte[] InputBuffer,
			ref int OutputBufferByteCount,
			byte[] OutputBuffer
		);

		[PreserveSig]
		int GetPrivateDataByteCount(
			out int PrivateInputByteCount,
			out int PrivateOutputByteCount
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("7EC4B1BD-43FB-4763-85D2-64FCB5C5F4CB")]
	public interface IMFContentDecryptorContext
	{
		[PreserveSig]
		int InitializeHardwareKey(
			int InputPrivateDataByteCount,
			[In] byte[] InputPrivateData,
			out long OutputPrivateData
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("eb533d5d-2db6-40f8-97a9-494692014f07")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFDXGIDeviceManager
	{
		[PreserveSig]
		int CloseDeviceHandle(
			IntPtr hDevice
		);

		[PreserveSig]
		int GetVideoService(
			IntPtr hDevice,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppService
		);

		[PreserveSig]
		int LockDevice(
			IntPtr hDevice,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnkDevice,
			[MarshalAs(UnmanagedType.Bool)] bool fBlock
		);

		[PreserveSig]
		int OpenDeviceHandle(
			out IntPtr phDevice
		);

		[PreserveSig]
		int ResetDevice(
			[MarshalAs(UnmanagedType.IUnknown)] object pUnkDevice,
			int resetToken
		);

		[PreserveSig]
		int TestDevice(
			IntPtr hDevice
		);

		[PreserveSig]
		int UnlockDevice(
			IntPtr hDevice,
			[MarshalAs(UnmanagedType.Bool)] bool fSaveState
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("8feed468-6f7e-440d-869a-49bdd283ad0d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSampleOutputStream
	{
		[PreserveSig]
		int BeginWriteSample(
			IMFSample pSample,
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object punkState
		);

		[PreserveSig]
		int EndWriteSample(
			IMFAsyncResult pResult
		);

		[PreserveSig]
		int Close();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("33ae5ea6-4316-436f-8ddd-d73d22f829ec")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMF2DBuffer2 : IMF2DBuffer
	{
		#region IMF2DBuffer Methods

		[PreserveSig]
		new int Lock2D(
			[Out] out IntPtr pbScanline0,
			out int plPitch
			);

		[PreserveSig]
		new int Unlock2D();

		[PreserveSig]
		new int GetScanline0AndPitch(
			out IntPtr pbScanline0,
			out int plPitch
			);

		[PreserveSig]
		new int IsContiguousFormat(
			[MarshalAs(UnmanagedType.Bool)] out bool pfIsContiguous
			);

		[PreserveSig]
		new int GetContiguousLength(
			out int pcbLength
			);

		[PreserveSig]
		new int ContiguousCopyTo(
			IntPtr pbDestBuffer,
			[In] int cbDestBuffer
			);

		[PreserveSig]
		new int ContiguousCopyFrom(
			[In] IntPtr pbSrcBuffer,
			[In] int cbSrcBuffer
			);

		#endregion

		[PreserveSig]
		int Lock2DSize(
			MF2DBuffer_LockFlags lockFlags,
			out IntPtr ppbScanline0,
			out int plPitch,
			out IntPtr ppbBufferStart,
			out int pcbBufferLength
		);

		[PreserveSig]
		int Copy2DTo(
			IMF2DBuffer2 pDestBuffer
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("e7174cfa-1c9e-48b1-8866-626226bfc258")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFDXGIBuffer
	{
		[PreserveSig]
		int GetResource([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		int GetSubresourceIndex(
			out int puSubresource
		);

		[PreserveSig]
		int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guid,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);

		[PreserveSig]
		int SetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guid,
			[MarshalAs(UnmanagedType.IUnknown)] object pUnkData
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("c7a4dca1-f5f0-47b6-b92b-bf0106d25791")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFAsyncCallbackLogging : IMFAsyncCallback
	{
		#region IMFAsyncCallback

		[PreserveSig]
		new int GetParameters(
			out MFASync pdwFlags,
			out MFAsyncCallbackQueue pdwQueue
			);

		[PreserveSig]
		new int Invoke([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pAsyncResult
			);

		#endregion

		[PreserveSig]
		IntPtr GetObjectPointer();

		[PreserveSig]
		int GetObjectTag();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("a27003d0-2354-4f2a-8d6a-ab7cff15437e")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFRemoteAsyncCallback
	{
		[PreserveSig]
		int Invoke(
			int hr,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pRemoteResult
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
	public interface IMFMediaSource : IMFMediaEventGenerator
	{
		#region IMFMediaEventGenerator methods

		[PreserveSig]
		new int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
			);

		[PreserveSig]
		new int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);

		#endregion

		[PreserveSig]
		int GetCharacteristics(
			out MFMediaSourceCharacteristics pdwCharacteristics
			);

		[PreserveSig]
		int CreatePresentationDescriptor(
			out IMFPresentationDescriptor ppPresentationDescriptor
			);

		[PreserveSig]
		int Start([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid pguidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarStartPosition
			);

		[PreserveSig]
		int Stop();

		[PreserveSig]
		int Pause();

		[PreserveSig]
		int Shutdown();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("03CB2711-24D7-4DB6-A17F-F3A7A479A536")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFPresentationDescriptor : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPresentationDescriptor.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFPresentationDescriptor.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetStreamDescriptorCount(
			out int pdwDescriptorCount
			);

		[PreserveSig]
		int GetStreamDescriptorByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Bool)] out bool pfSelected,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppDescriptor
			);

		[PreserveSig]
		int SelectStream(
			[In] int dwDescriptorIndex
			);

		[PreserveSig]
		int DeselectStream(
			[In] int dwDescriptorIndex
			);

		[PreserveSig]
		int Clone(
			[MarshalAs(UnmanagedType.Interface)] out IMFPresentationDescriptor ppPresentationDescriptor
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56C03D9C-9DBB-45F5-AB4B-D80F47C05938")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFStreamDescriptor : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFStreamDescriptor.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFStreamDescriptor.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetStreamIdentifier(
			out int pdwStreamIdentifier
			);

		[PreserveSig]
		int GetMediaTypeHandler(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppMediaTypeHandler
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("E93DCF6C-4B07-4E1E-8123-AA16ED6EADF5")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaTypeHandler
	{
		[PreserveSig]
		int IsMediaTypeSupported([In][MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
			IntPtr ppMediaType  //[MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
			);

		[PreserveSig]
		int GetMediaTypeCount(
			out int pdwTypeCount
			);

		[PreserveSig]
		int GetMediaTypeByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
			);

		[PreserveSig]
		int SetCurrentMediaType([In][MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType
			);

		[PreserveSig]
		int GetCurrentMediaType(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
			);

		[PreserveSig]
		int GetMajorType(
			out Guid pguidMajorType
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("83CF873A-F6DA-4BC8-823F-BACFD55DC433")]
	public interface IMFTopology : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFTopology.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFTopology.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetTopologyID(
			out long pID
			);

		[PreserveSig]
		int AddNode([In][MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
			);

		[PreserveSig]
		int RemoveNode([In][MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
			);

		[PreserveSig]
		int GetNodeCount(
			out short pwNodes
			);

		[PreserveSig]
		int GetNode(
			[In] short wIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppNode
			);

		[PreserveSig]
		int Clear();

		[PreserveSig]
		int CloneFrom([In][MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology
			);

		[PreserveSig]
		int GetNodeByID(
			[In] long qwTopoNodeID,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppNode
			);

		[PreserveSig]
		int GetSourceNodeCollection(
			[MarshalAs(UnmanagedType.Interface)] out IMFCollection ppCollection
			);

		[PreserveSig]
		int GetOutputNodeCollection(
			[MarshalAs(UnmanagedType.Interface)] out IMFCollection ppCollection
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("83CF873A-F6DA-4BC8-823F-BACFD55DC430")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTopologyNode : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFTopologyNode.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFTopologyNode.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int SetObject([In][MarshalAs(UnmanagedType.IUnknown)] object pObject
			);

		[PreserveSig]
		int GetObject(
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int GetNodeType(
			out MFTopologyType pType
			);

		[PreserveSig]
		int GetTopoNodeID(
			out long pID
			);

		[PreserveSig]
		int SetTopoNodeID(
			[In] long ullTopoID
			);

		[PreserveSig]
		int GetInputCount(
			out int pcInputs
			);

		[PreserveSig]
		int GetOutputCount(
			out int pcOutputs
			);

		[PreserveSig]
		int ConnectOutput(
			[In] int dwOutputIndex,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFTopologyNode pDownstreamNode,
			[In] int dwInputIndexOnDownstreamNode
			);

		[PreserveSig]
		int DisconnectOutput(
			[In] int dwOutputIndex
			);

		[PreserveSig]
		int GetInput(
			[In] int dwInputIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppUpstreamNode,
			out int pdwOutputIndexOnUpstreamNode
			);

		[PreserveSig]
		int GetOutput(
			[In] int dwOutputIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopologyNode ppDownstreamNode,
			out int pdwInputIndexOnDownstreamNode
			);

		[PreserveSig]
		int SetOutputPrefType(
			[In] int dwOutputIndex,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pType
			);

		[PreserveSig]
		int GetOutputPrefType(
			[In] int dwOutputIndex,
			out IMFMediaType ppType
			);

		[PreserveSig]
		int SetInputPrefType(
			[In] int dwInputIndex,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pType
			);

		[PreserveSig]
		int GetInputPrefType(
			[In] int dwInputIndex,
			out IMFMediaType ppType
			);

		[PreserveSig]
		int CloneFrom([In][MarshalAs(UnmanagedType.Interface)] IMFTopologyNode pNode
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("90377834-21D0-4DEE-8214-BA2E3E6C1127")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSession : IMFMediaEventGenerator
	{
		#region IMFMediaEventGenerator methods

		[PreserveSig]
		new int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
			);

		[PreserveSig]
		new int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);

		#endregion

		[PreserveSig]
		int SetTopology(
			[In] MFSessionSetTopologyFlags dwSetTopologyFlags,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFTopology pTopology
			);

		[PreserveSig]
		int ClearTopologies();

		[PreserveSig]
		int Start([In][MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarStartPosition
			);

		[PreserveSig]
		int Pause();

		[PreserveSig]
		int Stop();

		[PreserveSig]
		int Close();

		[PreserveSig]
		int Shutdown();

		[PreserveSig]
		int GetClock(
			[MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock
			);

		[PreserveSig]
		int GetSessionCapabilities(
			out MFSessionCapabilities pdwCaps
			);

		[PreserveSig]
		int GetFullTopology(
			[In] MFSessionGetFullTopologyFlags dwGetFullTopologyFlags,
			[In] long TopoId,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopology ppFullTopology
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("D182108F-4EC6-443F-AA42-A71106EC825F")]
	public interface IMFMediaStream : IMFMediaEventGenerator
	{
		#region IMFMediaEventGenerator methods

		[PreserveSig]
		new int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
			);

		[PreserveSig]
		new int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);

		#endregion

		[PreserveSig]
		int GetMediaSource(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaSource ppMediaSource
			);

		[PreserveSig]
		int GetStreamDescriptor(
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppStreamDescriptor
			);

		[PreserveSig]
		int RequestSample([In][MarshalAs(UnmanagedType.IUnknown)] object pToken
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("FBE5A32D-A497-4B61-BB85-97B1A848A6E3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSourceResolver
	{
		[PreserveSig]
		int CreateObjectFromURL([In][MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
			[In] MFResolution dwFlags,
			IPropertyStore pProps,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int CreateObjectFromByteStream([In][MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pwszURL,
			[In] MFResolution dwFlags,
			[In][MarshalAs(UnmanagedType.Interface)]
			IPropertyStore pProps,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int BeginCreateObjectFromURL([In][MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
			MFResolution dwFlags,
			IPropertyStore pProps,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object punkState
			);

		[PreserveSig]
		int EndCreateObjectFromURL(
			IMFAsyncResult pResult,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.Interface)] out object ppObject
			);

		[PreserveSig]
		int BeginCreateObjectFromByteStream([In][MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pwszURL,
			[In] MFResolution dwFlags,
			IPropertyStore pProps,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object punkState
		   );

		[PreserveSig]
		int EndCreateObjectFromByteStream(
			IMFAsyncResult pResult,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int CancelObjectCreation([In][MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFActivate : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFActivate.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFActivate.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int ActivateObject([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);

		[PreserveSig]
		int ShutdownObject();

		[PreserveSig]
		int DetachObject();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("FA993888-4383-415A-A930-DD472A8CF6F7")]
	public interface IMFGetService
	{
		[PreserveSig]
		int GetService([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppvObject
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("BB420AA4-765B-4A1F-91FE-D6A8A143924C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFByteStreamHandler
	{
		[PreserveSig]
		int BeginCreateObject([In][MarshalAs(UnmanagedType.Interface)] IMFByteStream pByteStream,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pwszURL,
			[In] MFResolution dwFlags,
			[In][MarshalAs(UnmanagedType.Interface)]
			IPropertyStore pProps,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndCreateObject([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int CancelObjectCreation([In][MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
			);

		[PreserveSig]
		int GetMaxNumberOfBytesRequiredForResolution(
			out long pqwBytes
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F6696E82-74F7-4F3D-A178-8A5E09C3659F")]
	public interface IMFClockStateSink
	{
		[PreserveSig]
		int OnClockStart(
			[In] long hnsSystemTime,
			[In] long llClockStartOffset
			);

		[PreserveSig]
		int OnClockStop(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		int OnClockPause(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		int OnClockRestart(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		int OnClockSetRate(
			[In] long hnsSystemTime,
			[In] float flRate
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("2EB1E945-18B8-4139-9B1A-D5D584818530")]
	public interface IMFClock
	{
		[PreserveSig]
		int GetClockCharacteristics(
			out MFClockCharacteristicsFlags pdwCharacteristics
			);

		[PreserveSig]
		int GetCorrelatedTime(
			[In] int dwReserved,
			out long pllClockTime,
			out long phnsSystemTime
			);

		[PreserveSig]
		int GetContinuityKey(
			out int pdwContinuityKey
			);

		[PreserveSig]
		int GetState(
			[In] int dwReserved,
			out MFClockState peClockState
			);

		[PreserveSig]
		int GetProperties(
			out MFClockProperties pClockProperties
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("868CE85C-8EA9-4F55-AB82-B009A910A805")]
	public interface IMFPresentationClock : IMFClock
	{
		#region IMFClock methods

		[PreserveSig]
		new int GetClockCharacteristics(
			out MFClockCharacteristicsFlags pdwCharacteristics
			);

		[PreserveSig]
		new int GetCorrelatedTime(
			[In] int dwReserved,
			out long pllClockTime,
			out long phnsSystemTime
			);

		[PreserveSig]
		new int GetContinuityKey(
			out int pdwContinuityKey
			);

		[PreserveSig]
		new int GetState(
			[In] int dwReserved,
			out MFClockState peClockState
			);

		[PreserveSig]
		new int GetProperties(
			out MFClockProperties pClockProperties
			);

		#endregion

		[PreserveSig]
		int SetTimeSource([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationTimeSource pTimeSource
			);

		[PreserveSig]
		int GetTimeSource(
			[MarshalAs(UnmanagedType.Interface)] out IMFPresentationTimeSource ppTimeSource
			);

		[PreserveSig]
		int GetTime(
			out long phnsClockTime
			);

		[PreserveSig]
		int AddClockStateSink([In][MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
			);

		[PreserveSig]
		int RemoveClockStateSink([In][MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
			);

		[PreserveSig]
		int Start(
			[In] long llClockStartOffset
			);

		[PreserveSig]
		int Stop();

		[PreserveSig]
		int Pause();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7FF12CCE-F76F-41C2-863B-1666C8E5E139")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFPresentationTimeSource : IMFClock
	{
		#region IMFClock methods

		[PreserveSig]
		new int GetClockCharacteristics(
			out MFClockCharacteristicsFlags pdwCharacteristics
			);

		[PreserveSig]
		new int GetCorrelatedTime(
			[In] int dwReserved,
			out long pllClockTime,
			out long phnsSystemTime
			);

		[PreserveSig]
		new int GetContinuityKey(
			out int pdwContinuityKey
			);

		[PreserveSig]
		new int GetState(
			[In] int dwReserved,
			out MFClockState peClockState
			);

		[PreserveSig]
		new int GetProperties(
			out MFClockProperties pClockProperties
			);

		#endregion

		[PreserveSig]
		int GetUnderlyingClock(
			[MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0E1D6009-C9F3-442D-8C51-A42D2D49452F")]
	public interface IMFMediaSourceTopologyProvider
	{
		[PreserveSig]
		int GetMediaSourceTopology([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopology ppTopology
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("197CD219-19CB-4DE1-A64C-ACF2EDCBE59E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSequencerSource
	{
		[PreserveSig]
		int AppendTopology([In][MarshalAs(UnmanagedType.Interface)] IMFTopology pTopology,
			[In] MFSequencerTopologyFlags dwFlags,
			out int pdwId
			);

		[PreserveSig]
		int DeleteTopology(
			[In] int dwId
			);

		[PreserveSig]
		int GetPresentationContext([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPD,
			out int pID,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopology ppTopology
			);

		[PreserveSig]
		int UpdateTopology(
			[In] int dwId,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFTopology pTopology
			);

		[PreserveSig]
		int UpdateTopologyFlags(
			[In] int dwId,
			[In] MFSequencerTopologyFlags dwFlags
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("ACF92459-6A61-42BD-B57C-B43E51203CB0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFContentProtectionManager
	{
		[PreserveSig]
		int BeginEnableContent(
			IMFActivate pEnablerActivate,
			IMFTopology pTopo,
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.Interface)] object punkState
			);

		[PreserveSig]
		int EndEnableContent(
			IMFAsyncResult pResult
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("D3C4EF59-49CE-4381-9071-D5BCD044C770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFContentEnabler
	{
		[PreserveSig]
		int GetEnableType(out Guid pType);

		[PreserveSig]
		int GetEnableURL(
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszURL,
			out int pcchURL,
			out MFURLTrustStatus pTrustStatus
			);

		[PreserveSig]
		int GetEnableData(
			[Out] out IntPtr ppbData,
			out int pcbData);

		[PreserveSig]
		int IsAutomaticSupported(
			[MarshalAs(UnmanagedType.Bool)] out bool pfAutomatic
			);

		[PreserveSig]
		int AutomaticEnable();

		[PreserveSig]
		int MonitorEnable();

		[PreserveSig]
		int Cancel();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("6EF2A660-47C0-4666-B13D-CBB717F2FA2C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSink
	{
		[PreserveSig]
		int GetCharacteristics(
			out MFMediaSinkCharacteristics pdwCharacteristics
			);

		[PreserveSig]
		int AddStreamSink(
			[In] int dwStreamSinkIdentifier,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pMediaType,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		int RemoveStreamSink(
			[In] int dwStreamSinkIdentifier
			);

		[PreserveSig]
		int GetStreamSinkCount(
			out int pcStreamSinkCount
			);

		[PreserveSig]
		int GetStreamSinkByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		int GetStreamSinkById(
			[In] int dwStreamSinkIdentifier,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		int SetPresentationClock([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
			);

		[PreserveSig]
		int GetPresentationClock(
			[MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
			);

		[PreserveSig]
		int Shutdown();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("0A97B3CF-8E7C-4A3D-8F8C-0C843DC247FB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFStreamSink : IMFMediaEventGenerator
	{
		#region IMFMediaEventGenerator methods

		[PreserveSig]
		new int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
			);

		[PreserveSig]
		new int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		new int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);

		#endregion

		[PreserveSig]
		int GetMediaSink(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaSink ppMediaSink
			);

		[PreserveSig]
		int GetIdentifier(
			out int pdwIdentifier
			);

		[PreserveSig]
		int GetMediaTypeHandler(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppHandler
			);

		[PreserveSig]
		int ProcessSample([In][MarshalAs(UnmanagedType.Interface)] IMFSample pSample
			);

		[PreserveSig]
		int PlaceMarker(
			[In] MFStreamSinkMarkerType eMarkerType,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarMarkerValue,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarContextValue
			);

		[PreserveSig]
		int Flush();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("EAECB74A-9A50-42CE-9541-6A7F57AA4AD7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFFinalizableMediaSink : IMFMediaSink
	{
		#region IMFMediaSink methods

		[PreserveSig]
		new int GetCharacteristics(
			out MFMediaSinkCharacteristics pdwCharacteristics
			);

		[PreserveSig]
		new int AddStreamSink(
			[In] int dwStreamSinkIdentifier,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pMediaType,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		new int RemoveStreamSink(
			[In] int dwStreamSinkIdentifier
			);

		[PreserveSig]
		new int GetStreamSinkCount(
			out int pcStreamSinkCount
			);

		[PreserveSig]
		new int GetStreamSinkByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		new int GetStreamSinkById(
			[In] int dwStreamSinkIdentifier,
			[MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
			);

		[PreserveSig]
		new int SetPresentationClock([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
			);

		[PreserveSig]
		new int GetPresentationClock(
			[MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
			);

		[PreserveSig]
		new int Shutdown();

		#endregion

		[PreserveSig]
		int BeginFinalize([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndFinalize([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("0A9CCDBC-D797-4563-9667-94EC5D79292D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFRateSupport
	{
		[PreserveSig]
		int GetSlowestRate(
			[In] MFRateDirection eDirection,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fThin,
			out float pflRate
			);

		[PreserveSig]
		int GetFastestRate(
			[In] MFRateDirection eDirection,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fThin,
			out float pflRate
			);

		[PreserveSig]
		int IsRateSupported([In][MarshalAs(UnmanagedType.Bool)] bool fThin,
			[In] float flRate,
			[In][Out] MFFloat pflNearestSupportedRate
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("059054B3-027C-494C-A27D-9113291CF87F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSourceOpenMonitor
	{
		[PreserveSig]
		int OnSourceEvent([In][MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("97EC2EA4-0E42-4937-97AC-9D6D328824E1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFShutdown
	{
		[PreserveSig]
		int Shutdown();

		[PreserveSig]
		int GetShutdownStatus(
			out MFShutdownStatus pStatus
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("F88CFB8C-EF16-4991-B450-CB8C69E51704")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMetadata
	{
		[PreserveSig]
		int SetLanguage([In][MarshalAs(UnmanagedType.LPWStr)] string pwszRFC1766
			);

		[PreserveSig]
		int GetLanguage(
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszRFC1766
			);

		[PreserveSig]
		int GetAllLanguages([In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMetadata.GetAllLanguages", MarshalTypeRef = typeof(PVMarshaler))] Variant ppvLanguages
			);

		[PreserveSig]
		int SetProperty([In][MarshalAs(UnmanagedType.LPWStr)] string pwszName,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant ppvValue
			);

		[PreserveSig]
		int GetProperty([In][MarshalAs(UnmanagedType.LPWStr)] string pwszName,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMetadata.GetProperty", MarshalTypeRef = typeof(PVMarshaler))]
			Variant ppvValue
			);

		[PreserveSig]
		int DeleteProperty([In][MarshalAs(UnmanagedType.LPWStr)] string pwszName
			);

		[PreserveSig]
		int GetAllPropertyNames(
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMetadata.GetAllPropertyNames", MarshalTypeRef = typeof(PVMarshaler))] Variant ppvNames
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("88DDCD21-03C3-4275-91ED-55EE3929328F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFRateControl
	{
		[PreserveSig]
		int SetRate([In][MarshalAs(UnmanagedType.Bool)] bool fThin,
			[In] float flRate
			);

		[PreserveSig]
		int GetRate([In][Out][MarshalAs(UnmanagedType.Bool)] ref bool pfThin,
			out float pflRate
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("E56E4CBD-8F70-49D8-A0F8-EDB3D6AB9BF2")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTimer
	{
		[PreserveSig]
		int SetTimer(
			[In] MFTimeFlags dwFlags,
			[In] long llClockTime,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppunkKey
			);

		[PreserveSig]
		int CancelTimer([In][MarshalAs(UnmanagedType.IUnknown)] object punkKey
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("56181D2D-E221-4ADB-B1C8-3CEE6A53F76F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMetadataProvider
	{
		[PreserveSig]
		int GetMFMetadata([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
			[In] int dwStreamIdentifier,
			[In] int dwFlags, // must be zero
			[MarshalAs(UnmanagedType.Interface)] out IMFMetadata ppMFMetadata
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F5042EA4-7A96-4A75-AA7B-2BE1EF7F88D5")]
	public interface IMFByteStreamCacheControl
	{
		[PreserveSig]
		int StopBackgroundTransfer();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("508E71D3-EC66-4FC3-8775-B4B9ED6BA847")]
	public interface IMFFieldOfUseMFTUnlock
	{
		[PreserveSig]
		int Unlock([In][MarshalAs(UnmanagedType.IUnknown)] object pUnkMFT
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F3706F0D-8EA2-4886-8000-7155E9EC2EAE")]
	public interface IMFQualityAdvise2 : IMFQualityAdvise
	{
		#region IMFQualityAdvise methods

		[PreserveSig]
		new int SetDropMode(
			[In] MFQualityDropMode eDropMode
			);

		[PreserveSig]
		new int SetQualityLevel(
			[In] MFQualityLevel eQualityLevel
			);

		[PreserveSig]
		new int GetDropMode(
			out MFQualityDropMode peDropMode
			);

		[PreserveSig]
		new int GetQualityLevel(
			out MFQualityLevel peQualityLevel
			);

		[PreserveSig]
		new int DropTime(
			[In] long hnsAmountToDrop
			);

		#endregion

		[PreserveSig]
		int NotifyQualityEvent(
			IMFMediaEvent pEvent,
			out MFQualityAdviseFlags pdwFlags
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("DFCD8E4D-30B5-4567-ACAA-8EB5B7853DC9")]
	public interface IMFQualityAdviseLimits
	{
		[PreserveSig]
		int GetMaximumDropMode(
			out MFQualityDropMode peDropMode
		);

		[PreserveSig]
		int GetMinimumQualityLevel(
			out MFQualityLevel peQualityLevel
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("CA86AA50-C46E-429E-AB27-16D6AC6844CB")]
	public interface IMFSampleGrabberSinkCallback2 : IMFSampleGrabberSinkCallback
	{
		#region IMFClockStateSink methods

		[PreserveSig]
		new int OnClockStart(
			[In] long hnsSystemTime,
			[In] long llClockStartOffset
			);

		[PreserveSig]
		new int OnClockStop(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockPause(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockRestart(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockSetRate(
			[In] long hnsSystemTime,
			[In] float flRate
			);

		#endregion

		#region IMFSampleGrabberSinkCallback methods

		[PreserveSig]
		new int OnSetPresentationClock([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
			);

		[PreserveSig]
		new int OnProcessSample([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
			[In] int dwSampleFlags, // must be zero
			[In] long llSampleTime,
			[In] long llSampleDuration,
			[In] IntPtr pSampleBuffer,
			[In] int dwSampleSize
			);

		[PreserveSig]
		new int OnShutdown();

		#endregion

		[PreserveSig]
		int OnProcessSampleEx(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
			[In] int dwSampleFlags, // No flags are defined
			[In] long llSampleTime,
			[In] long llSampleDuration,
			[In] IntPtr pSampleBuffer,
			[In] int dwSampleSize,
			IMFAttributes pAttributes
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9DB7AA41-3CC5-40D4-8509-555804AD34CC")]
	public interface IMFStreamingSinkConfig
	{
		[PreserveSig]
		int StartStreaming(
			[MarshalAs(UnmanagedType.Bool)] bool fSeekOffsetIsByteOffset,
			[In] long qwSeekOffset
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("AB9D8661-F7E8-4EF4-9861-89F334F94E74")]
	public interface IMFTimecodeTranslate
	{
		[PreserveSig]
		int BeginConvertTimecodeToHNS([In][MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pPropVarTimecode,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object punkState
		);

		[PreserveSig]
		int EndConvertTimecodeToHNS(
			IMFAsyncResult pResult,
			out long phnsTime
		);

		[PreserveSig]
		int BeginConvertHNSToTimecode(
			[In] long hnsTime,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object punkState
		);

		[PreserveSig]
		int EndConvertHNSToTimecode(
			IMFAsyncResult pResult,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFTimecodeTranslate.EndConvertHNSToTimecode", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pVarTimecode
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4ADFDBA3-7AB0-4953-A62B-461E7FF3DA1E")]
	public interface IMFTranscodeProfile
	{
		[PreserveSig]
		int SetAudioAttributes(
			IMFAttributes pAttrs
		);

		[PreserveSig]
		int GetAudioAttributes(
			out IMFAttributes ppAttrs
		);

		[PreserveSig]
		int SetVideoAttributes(
			IMFAttributes pAttrs
		);

		[PreserveSig]
		int GetVideoAttributes(
			out IMFAttributes ppAttrs
		);

		[PreserveSig]
		int SetContainerAttributes(
			IMFAttributes pAttrs
		);

		[PreserveSig]
		int GetContainerAttributes(
			out IMFAttributes ppAttrs
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8CFFCD2E-5A03-4A3A-AFF7-EDCD107C620E")]
	public interface IMFTranscodeSinkInfoProvider
	{
		[PreserveSig]
		int SetOutputFile([In][MarshalAs(UnmanagedType.LPWStr)] string pwszFileName
		);

		[PreserveSig]
		int SetOutputByteStream(
			IMFActivate pByteStreamActivate
		);

		[PreserveSig]
		int SetProfile(
			IMFTranscodeProfile pProfile
		);

		[PreserveSig]
		int GetSinkInfo(
			out MFTranscodeSinkInfo pSinkInfo
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("992388B4-3372-4F67-8B6F-C84C071F4751")]
	public interface IMFVideoSampleAllocatorCallback
	{
		[PreserveSig]
		int SetCallback(
			IMFVideoSampleAllocatorNotify pNotify
		);

		[PreserveSig]
		int GetFreeSampleCount(
			out int plSamples
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A792CDBE-C374-4E89-8335-278E7B9956A4")]
	public interface IMFVideoSampleAllocatorNotify
	{
		[PreserveSig]
		int NotifyRelease();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("EC15E2E9-E36B-4F7C-8758-77D452EF4CE7")]
	public interface IMFQualityAdvise
	{
		[PreserveSig]
		int SetDropMode(
			[In] MFQualityDropMode eDropMode
			);

		[PreserveSig]
		int SetQualityLevel(
			[In] MFQualityLevel eQualityLevel
			);

		[PreserveSig]
		int GetDropMode(
			out MFQualityDropMode peDropMode
			);

		[PreserveSig]
		int GetQualityLevel(
			out MFQualityLevel peQualityLevel
			);

		[PreserveSig]
		int DropTime(
			[In] long hnsAmountToDrop
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8C7B80BF-EE42-4B59-B1DF-55668E1BDCA8")]
	public interface IMFSampleGrabberSinkCallback : IMFClockStateSink
	{
		#region IMFClockStateSink methods

		[PreserveSig]
		new int OnClockStart(
			[In] long hnsSystemTime,
			[In] long llClockStartOffset
			);

		[PreserveSig]
		new int OnClockStop(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockPause(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockRestart(
			[In] long hnsSystemTime
			);

		[PreserveSig]
		new int OnClockSetRate(
			[In] long hnsSystemTime,
			[In] float flRate
			);

		#endregion

		[PreserveSig]
		int OnSetPresentationClock([In][MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
			);

		[PreserveSig]
		int OnProcessSample([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidMajorMediaType,
			[In] int dwSampleFlags, // must be zero
			[In] long llSampleTime,
			[In] long llSampleDuration,
			[In] IntPtr pSampleBuffer,
			[In] int dwSampleSize
			);

		[PreserveSig]
		int OnShutdown();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("86CBC910-E533-4751-8E3B-F19B5B806A03")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFVideoSampleAllocator
	{
		[PreserveSig]
		int SetDirectXManager([In][MarshalAs(UnmanagedType.IUnknown)] object pManager
			);

		[PreserveSig]
		int UninitializeSampleAllocator();

		[PreserveSig]
		int InitializeSampleAllocator(
			[In] int cRequestedFrames,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pMediaType
			);

		[PreserveSig]
		int AllocateSample(
			[MarshalAs(UnmanagedType.Interface)] out IMFSample ppSample
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("994E23AF-1CC2-493C-B9FA-46F1CB040FA4")]
	public interface IMFPMPServer
	{
		[PreserveSig]
		int LockProcess();

		[PreserveSig]
		int UnlockProcess();

		[PreserveSig]
		int CreateObjectByCLSID([In][MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppObject
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5B87EF6C-7ED8-434F-BA0E-184FAC1628D1")]
	public interface IMFNetCredentialCache
	{
		[PreserveSig]
		int GetCredential([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pszRealm,
			[In] MFNetAuthenticationFlags dwAuthenticationFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFNetCredential ppCred,
			out MFNetCredentialRequirements pdwRequirementsFlags
			);

		[PreserveSig]
		int SetGood([In][MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fGood
			);

		[PreserveSig]
		int SetUserOptions([In][MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
			[In] MFNetCredentialOptions dwOptionsFlags
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("E9CD0383-A268-4BB4-82DE-658D53574D41")]
	public interface IMFNetProxyLocator
	{
		[PreserveSig]
		int FindFirstProxy([In][MarshalAs(UnmanagedType.LPWStr)] string pszHost,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string pszUrl,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fReserved
			);

		[PreserveSig]
		int FindNextProxy();

		[PreserveSig]
		int RegisterProxyResult(
			int hrOp
			);

		[PreserveSig]
		int GetCurrentProxy([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszStr,
			[In][Out] MFInt pcchStr
			);

		[PreserveSig]
		int Clone(
			out IMFNetProxyLocator ppProxyLocator
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("1CDE6309-CAE0-4940-907E-C1EC9C3D1D4A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFRemoteDesktopPlugin
	{
		[PreserveSig]
		int UpdateTopology(
			IMFTopology pTopology
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5B87EF6A-7ED8-434F-BA0E-184FAC1628D1")]
	public interface IMFNetCredential
	{
		[PreserveSig]
		int SetUser([In][MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
			[In] int cbData,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fDataIsEncrypted
			);

		[PreserveSig]
		int SetPassword([In][MarshalAs(UnmanagedType.LPArray)] byte[] pbData,
			[In] int cbData,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fDataIsEncrypted
			);

		[PreserveSig]
		int GetUser([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbData,
			[In][Out] MFInt pcbData,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fEncryptData
			);

		[PreserveSig]
		int GetPassword([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbData,
			[In][Out] MFInt pcbData,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fEncryptData
			);

		[PreserveSig]
		int LoggedOnUser(
			[MarshalAs(UnmanagedType.Bool)] out bool pfLoggedOnUser
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("E9CD0384-A268-4BB4-82DE-658D53574D41")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFNetProxyLocatorFactory
	{
		[PreserveSig]
		int CreateProxyLocator([In][MarshalAs(UnmanagedType.LPWStr)] string pszProtocol,
			out IMFNetProxyLocator ppProxyLocator
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("F70CA1A9-FDC7-4782-B994-ADFFB1C98606")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFPMPHost
	{
		[PreserveSig]
		int LockProcess();

		[PreserveSig]
		int UnlockProcess();

		[PreserveSig]
		int CreateObjectByCLSID(
			[MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			IStream pStream,
			[MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("5B87EF6B-7ED8-434F-BA0E-184FAC1628D1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFNetCredentialManager
	{
		[PreserveSig]
		int BeginGetCredentials(
			[In] MFNetCredentialManagerGetParam pParam,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
			);

		[PreserveSig]
		int EndGetCredentials([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
			[MarshalAs(UnmanagedType.Interface)] out IMFNetCredential ppCred
			);

		[PreserveSig]
		int SetGood([In][MarshalAs(UnmanagedType.Interface)] IMFNetCredential pCred,
			[In][MarshalAs(UnmanagedType.Bool)]
			bool fGood
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("5DFD4B2A-7674-4110-A4E6-8A68FD5F3688")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaSinkPreroll
	{
		[PreserveSig]
		int NotifyPreroll(
			long hnsUpcomingStartTime
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("E9931663-80BF-4C6E-98AF-5DCF58747D1F")]
	public interface IMFSaveJob
	{
		[PreserveSig]
		int BeginSave(
			[In] IMFByteStream pStream,
			[In] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
			);

		[PreserveSig]
		int EndSave(
			[In] IMFAsyncResult pResult
			);

		[PreserveSig]
		int CancelSave();

		[PreserveSig]
		int GetProgress(
			out int pdwPercentComplete
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("76B1BBDB-4EC8-4F36-B106-70A9316DF593")]
	public interface IMFAudioStreamVolume
	{
		[PreserveSig]
		int GetChannelCount(
			out int pdwCount
			);

		[PreserveSig]
		int SetChannelVolume(
			int dwIndex,
			float fLevel
			);

		[PreserveSig]
		int GetChannelVolume(
			int dwIndex,
			out float pfLevel
			);

		[PreserveSig]
		int SetAllVolumes(
			int dwCount,
			[In][MarshalAs(UnmanagedType.LPArray)]
			float[] pfVolumes
			);

		[PreserveSig]
		int GetAllVolumes(
			int dwCount,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			float[] pfVolumes
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A0638C2B-6465-4395-9AE7-A321A9FD2856")]
	public interface IMFAudioPolicy
	{
		[PreserveSig]
		int SetGroupingParam([In][MarshalAs(UnmanagedType.LPStruct)] Guid rguidClass
			);

		[PreserveSig]
		int GetGroupingParam(
			out Guid pguidClass
			);

		[PreserveSig]
		int SetDisplayName([In][MarshalAs(UnmanagedType.LPWStr)] string pszName
			);

		[PreserveSig]
		int GetDisplayName(
			[MarshalAs(UnmanagedType.LPWStr)] out string pszName
			);

		[PreserveSig]
		int SetIconPath([In][MarshalAs(UnmanagedType.LPWStr)] string pszPath
			);

		[PreserveSig]
		int GetIconPath(
			[MarshalAs(UnmanagedType.LPWStr)] out string pszPath
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("6D66D782-1D4F-4DB7-8C63-CB8C77F1EF5E")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFByteStreamBuffering
	{
		[PreserveSig]
		int SetBufferingParams(
			[In] MFByteStreamBufferingParams pParams
			);

		[PreserveSig]
		int EnableBuffering(
			[MarshalAs(UnmanagedType.Bool)] bool fEnable
			);

		[PreserveSig]
		int StopBuffering();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("6D4C7B74-52A0-4BB7-B0DB-55F29F47A668")]
	public interface IMFSchemeHandler
	{
		[PreserveSig]
		int BeginCreateObject([In][MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
			[In] MFResolution dwFlags,
			IPropertyStore pProps,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppIUnknownCancelCookie,
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndCreateObject(
			IMFAsyncResult pResult,
			out MFObjectType pObjectType,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppObject
			);

		[PreserveSig]
		int CancelObjectCreation(
			[MarshalAs(UnmanagedType.IUnknown)] object pIUnknownCancelCookie
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("35FE1BB8-A3A9-40FE-BBEC-EB569C9CCCA3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFWorkQueueServices
	{
		[PreserveSig]
		int BeginRegisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
			);

		[PreserveSig]
		int EndRegisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
			);

		[PreserveSig]
		int BeginUnregisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
			);

		[PreserveSig]
		int EndUnregisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
			);

		[PreserveSig]
		int GetTopologyWorkQueueMMCSSClass(
			[In] MFAsyncCallbackQueue dwTopologyWorkQueueId,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszClass,
			[In][Out] ref int pcchClass
			);

		[PreserveSig]
		int GetTopologyWorkQueueMMCSSTaskId(
			[In] MFAsyncCallbackQueue dwTopologyWorkQueueId,
			out int pdwTaskId
			);

		[PreserveSig]
		int BeginRegisterPlatformWorkQueueWithMMCSS(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueue,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszClass,
			[In] int dwTaskId,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object pState
			);

		[PreserveSig]
		int EndRegisterPlatformWorkQueueWithMMCSS(
			IMFAsyncResult pResult,
			out int pdwTaskId
			);

		[PreserveSig]
		int BeginUnregisterPlatformWorkQueueWithMMCSS(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueue,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object pState
			);

		[PreserveSig]
		int EndUnregisterPlatformWorkQueueWithMMCSS(
			IMFAsyncResult pResult
			);

		[PreserveSig]
		int GetPlaftormWorkQueueMMCSSClass(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueueId,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszClass,
			[In][Out] ref int pcchClass
			);

		[PreserveSig]
		int GetPlatformWorkQueueMMCSSTaskId(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueueId,
			out int pdwTaskId
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("09EF5BE3-C8A7-469E-8B70-73BF25BB193F")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFObjectReferenceStream
	{
		[PreserveSig]
		int SaveReference([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnk
			);

		[PreserveSig]
		int LoadReference([In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("2347D60B-3FB5-480C-8803-8DF3ADCD3EF0")]
	public interface IMFRealTimeClient
	{
		[PreserveSig]
		int RegisterThreads(
			[In] int dwTaskIndex,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszClass
			);

		[PreserveSig]
		int UnregisterThreads();

		[PreserveSig]
		int SetWorkQueue(
			[In] MFAsyncCallbackQueue dwWorkQueueId
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("8D009D86-5B9F-4115-B1FC-9F80D52AB8AB")]
	public interface IMFQualityManager
	{
		[PreserveSig]
		int NotifyTopology(
			IMFTopology pTopology
			);

		[PreserveSig]
		int NotifyPresentationClock(
			IMFPresentationClock pClock
			);

		[PreserveSig]
		int NotifyProcessInput(
			IMFTopologyNode pNode,
			int lInputIndex,
			IMFSample pSample
			);

		[PreserveSig]
		int NotifyProcessOutput(
			IMFTopologyNode pNode,
			int lOutputIndex,
			IMFSample pSample
			);

		[PreserveSig]
		int NotifyQualityEvent([In][MarshalAs(UnmanagedType.IUnknown)] object pObject,
			IMFMediaEvent pEvent
			);

		[PreserveSig]
		int Shutdown();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("7BE19E73-C9BF-468A-AC5A-A5E8653BEC87")]
	public interface IMFNetSchemeHandlerConfig
	{
		[PreserveSig]
		int GetNumberOfSupportedProtocols(
			out int pcProtocols
			);

		[PreserveSig]
		int GetSupportedProtocolType(
			[In] int nProtocolIndex,
			out MFNetSourceProtocolType pnProtocolType
			);

		[PreserveSig]
		int ResetProtocolRolloverSettings();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("A7E025DD-5303-4A62-89D6-E747E1EFAC73")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSAMIStyle
	{
		[PreserveSig]
		int GetStyleCount(
			out int pdwCount
			);

		[PreserveSig]
		int GetStyles([In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSAMIStyle.GetStyles", MarshalTypeRef = typeof(PVMarshaler))] Variant pVarStyleArray
			);

		[PreserveSig]
		int SetSelectedStyle([In][MarshalAs(UnmanagedType.LPWStr)] string pwszStyle
			);

		[PreserveSig]
		int GetSelectedStyle(
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszStyle
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("6C4E655D-EAD8-4421-B6B9-54DCDBBDF820")]
	public interface IMFPMPClient
	{
		[PreserveSig]
		int SetPMPHost(
			IMFPMPHost pPMPHost
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("676AA6DD-238A-410D-BB99-65668D01605A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFTopologyNodeAttributeEditor
	{
		[PreserveSig]
		int UpdateNodeAttributes(
			[In] long TopoId,
			[In] int cUpdates,
			[In][MarshalAs(UnmanagedType.LPArray)]
			MFTopoNodeAttributeUpdate[] pUpdates
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("DE9A6157-F660-4643-B56A-DF9F7998C7CD")]
	public interface IMFTopoLoader
	{
		[PreserveSig]
		int Load([In][MarshalAs(UnmanagedType.Interface)] IMFTopology pInputTopo,
			[MarshalAs(UnmanagedType.Interface)] out IMFTopology ppOutputTopo,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFTopology pCurrentTopo
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("091878a3-bf11-4a5c-bc9f-33995b06ef2d")]
	public interface IMFNetResourceFilter
	{
		[PreserveSig]
		int OnRedirect([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl,
			[MarshalAs(UnmanagedType.VariantBool)] out bool pvbCancel
		);

		[PreserveSig]
		int OnSendingRequest([In][MarshalAs(UnmanagedType.LPWStr)] string pszUrl
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("71CE469C-F34B-49EA-A56B-2D2A10E51149")]
	public interface IMFByteStreamCacheControl2 : IMFByteStreamCacheControl
	{
		#region IMFByteStreamCacheControl methods

		[PreserveSig]
		new int StopBackgroundTransfer();

		#endregion

		[PreserveSig]
		int GetByteRanges(
			out int pcRanges,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out MF_BYTE_STREAM_CACHE_RANGE[] ppRanges
		);

		[PreserveSig]
		int SetCacheLimit(
			long qwBytes
		);

		[PreserveSig]
		int IsBackgroundTransferActive([Out][MarshalAs(UnmanagedType.Bool)] out bool pfActive
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("96bf961b-40fe-42f1-ba9d-320238b49700")]
	public interface IMFWorkQueueServicesEx : IMFWorkQueueServices
	{
		#region IMFWorkQueueServices methods

		[PreserveSig]
		new int BeginRegisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
		);

		[PreserveSig]
		new int EndRegisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
		);

		[PreserveSig]
		new int BeginUnregisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pState
		);

		[PreserveSig]
		new int EndUnregisterTopologyWorkQueuesWithMMCSS([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
		);

		[PreserveSig]
		new int GetTopologyWorkQueueMMCSSClass(
			[In] MFAsyncCallbackQueue dwTopologyWorkQueueId,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszClass,
			[In][Out] ref int pcchClass
		);

		[PreserveSig]
		new int GetTopologyWorkQueueMMCSSTaskId(
			[In] MFAsyncCallbackQueue dwTopologyWorkQueueId,
			out int pdwTaskId
		);

		[PreserveSig]
		new int BeginRegisterPlatformWorkQueueWithMMCSS(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueue,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszClass,
			[In] int dwTaskId,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object pState
		);

		[PreserveSig]
		new int EndRegisterPlatformWorkQueueWithMMCSS(
			IMFAsyncResult pResult,
			out int pdwTaskId
		);

		[PreserveSig]
		new int BeginUnregisterPlatformWorkQueueWithMMCSS(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueue,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object pState
		);

		[PreserveSig]
		new int EndUnregisterPlatformWorkQueueWithMMCSS(
			IMFAsyncResult pResult
		);

		[PreserveSig]
		new int GetPlaftormWorkQueueMMCSSClass(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueueId,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszClass,
			[In][Out] ref int pcchClass
		);

		[PreserveSig]
		new int GetPlatformWorkQueueMMCSSTaskId(
			[In] MFAsyncCallbackQueue dwPlatformWorkQueueId,
			out int pdwTaskId
		);

		#endregion

		[PreserveSig]
		int GetTopologyWorkQueueMMCSSPriority(
			int dwTopologyWorkQueueId,
			out int plPriority
		);

		[PreserveSig]
		int BeginRegisterPlatformWorkQueueWithMMCSSEx(
			int dwPlatformWorkQueue,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			string wszClass,
			int dwTaskId,
			int lPriority,
			IMFAsyncCallback pCallback,
			[MarshalAs(UnmanagedType.IUnknown)] object pState
		);

		[PreserveSig]
		int GetPlatformWorkQueueMMCSSPriority(
			int dwPlatformWorkQueueId,
			out int plPriority
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("03910848-AB16-4611-B100-17B88AE2F248")]
	public interface IMFRealTimeClientEx
	{
		[PreserveSig]
		int RegisterThreadsEx(
			ref int pdwTaskIndex,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszClassName,
			int lBasePriority
		);

		[PreserveSig]
		int UnregisterThreads();

		[PreserveSig]
		int SetWorkQueueEx(
			int dwMultithreadedWorkQueueId,
			int lWorkItemBasePriority
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A3F675D5-6119-4f7f-A100-1D8B280F0EFB")]
	public interface IMFVideoProcessorControl
	{
		[PreserveSig]
		int SetBorderColor(
			MFARGB pBorderColor
		);

		[PreserveSig]
		int SetSourceRectangle(
			MFRect pSrcRect
		);

		[PreserveSig]
		int SetDestinationRectangle(
			MFRect pDstRect
		);

		[PreserveSig]
		int SetMirror(
			MF_VIDEO_PROCESSOR_MIRROR eMirror
		);

		[PreserveSig]
		int SetRotation(
			MF_VIDEO_PROCESSOR_ROTATION eRotation
		);

		[PreserveSig]
		int SetConstrictionSize(
			[In] MFSize pConstrictionSize
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("545b3a48-3283-4f62-866f-a62d8f598f9f")]
	public interface IMFVideoSampleAllocatorEx : IMFVideoSampleAllocator
	{
		#region IMFVideoSampleAllocator methods

		[PreserveSig]
		new int SetDirectXManager([In][MarshalAs(UnmanagedType.IUnknown)] object pManager
		);

		[PreserveSig]
		new int UninitializeSampleAllocator();

		[PreserveSig]
		new int InitializeSampleAllocator(
			[In] int cRequestedFrames,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFMediaType pMediaType
		);

		[PreserveSig]
		new int AllocateSample(
			[MarshalAs(UnmanagedType.Interface)] out IMFSample ppSample
		);

		#endregion

		[PreserveSig]
		int InitializeSampleAllocatorEx(
			int cInitialSamples,
			int cMaximumSamples,
			IMFAttributes pAttributes,
			IMFMediaType pMediaType
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("089EDF13-CF71-4338-8D13-9E569DBDC319")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSimpleAudioVolume
	{
		[PreserveSig]
		int SetMasterVolume(
			[In] float fLevel
			);

		[PreserveSig]
		int GetMasterVolume(
			out float pfLevel
			);

		[PreserveSig]
		int SetMute([In][MarshalAs(UnmanagedType.Bool)] bool bMute
			);

		[PreserveSig]
		int GetMute(
			[MarshalAs(UnmanagedType.Bool)] out bool pbMute
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("36F846FC-2256-48B6-B58E-E2B638316581")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEventQueue
	{
		[PreserveSig]
		int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		int QueueEvent([In][MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
			);

		[PreserveSig]
		int QueueEventParamVar(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In][MarshalAs(UnmanagedType.Error)]
			int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);

		[PreserveSig]
		int QueueEventParamUnk(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In][MarshalAs(UnmanagedType.Error)]
			int hrStatus,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnk
			);

		[PreserveSig]
		int Shutdown();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("DF598932-F10C-4E39-BBA2-C308F101DAA3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEvent : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetType(
			out MediaEventType pmet
			);

		[PreserveSig]
		int GetExtendedType(
			out Guid pguidExtendedType
			);

		[PreserveSig]
		int GetStatus(
			[MarshalAs(UnmanagedType.Error)] out int phrStatus
			);

		[PreserveSig]
		int GetValue([In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetValue", MarshalTypeRef = typeof(PVMarshaler))] Variant pvValue
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFAttributes
	{
		[PreserveSig]
		int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAttributes.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		int DeleteAllItems();

		[PreserveSig]
		int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		int LockStore();

		[PreserveSig]
		int UnlockStore();

		[PreserveSig]
		int GetCount(
			out int pcItems
			);

		[PreserveSig]
		int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAttributes.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFMediaEventGenerator
	{
		[PreserveSig]
		int GetEvent(
			[In] MFEventFlag dwFlags,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		int BeginGetEvent([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object o
			);

		[PreserveSig]
		int EndGetEvent(
			IMFAsyncResult pResult,
			out IMFMediaEvent ppEvent
			);

		[PreserveSig]
		int QueueEvent(
			[In] MediaEventType met,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidExtendedType,
			[In] int hrStatus,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvValue
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("AC6B7889-0740-4D51-8619-905994A55CC6")]
	public interface IMFAsyncResult
	{
		[PreserveSig]
		int GetState(
			[MarshalAs(UnmanagedType.IUnknown)] out object ppunkState
			);

		[PreserveSig]
		int GetStatus();

		[PreserveSig]
		int SetStatus([In][MarshalAs(UnmanagedType.Error)] int hrStatus
			);

		[PreserveSig]
		int GetObject(
			[MarshalAs(UnmanagedType.Interface)] out object ppObject
			);

		[PreserveSig]
		IntPtr GetStateNoAddRef();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A27003CF-2354-4F2A-8D6A-AB7CFF15437E")]
	public interface IMFAsyncCallback
	{
		[PreserveSig]
		int GetParameters(
			out MFASync pdwFlags,
			out MFAsyncCallbackQueue pdwQueue
			);

		[PreserveSig]
		int Invoke([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pAsyncResult
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")]
	public interface IMFMediaType : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaType.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaType.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetMajorType(
			out Guid pguidMajorType
			);

		[PreserveSig]
		int IsCompressedFormat(
			[MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
			);

		[PreserveSig]
		int IsEqual([In][MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
			out MFMediaEqual pdwFlags
			);

		[PreserveSig]
		int GetRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			out IntPtr ppvRepresentation
			);

		[PreserveSig]
		int FreeRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			[In] IntPtr pvRepresentation
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5BC8A76B-869A-46A3-9B03-FA218A66AEBE")]
	public interface IMFCollection
	{
		[PreserveSig]
		int GetElementCount(
			out int pcElements
			);

		[PreserveSig]
		int GetElement(
			[In] int dwElementIndex,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement
			);

		[PreserveSig]
		int AddElement([In][MarshalAs(UnmanagedType.IUnknown)] object pUnkElement
			);

		[PreserveSig]
		int RemoveElement(
			[In] int dwElementIndex,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppUnkElement
			);

		[PreserveSig]
		int InsertElementAt(
			[In] int dwIndex,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		int RemoveAllElements();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("AD4C1B00-4BF7-422F-9175-756693D9130D")]
	public interface IMFByteStream
	{
		[PreserveSig]
		int GetCapabilities(
			out MFByteStreamCapabilities pdwCapabilities
			);

		[PreserveSig]
		int GetLength(
			out long pqwLength
			);

		[PreserveSig]
		int SetLength(
			[In] long qwLength
			);

		[PreserveSig]
		int GetCurrentPosition(
			out long pqwPosition
			);

		[PreserveSig]
		int SetCurrentPosition(
			[In] long qwPosition
			);

		[PreserveSig]
		int IsEndOfStream(
			[MarshalAs(UnmanagedType.Bool)] out bool pfEndOfStream
			);

		[PreserveSig]
		int Read(
			IntPtr pb,
			[In] int cb,
			out int pcbRead
			);

		[PreserveSig]
		int BeginRead(
			IntPtr pb,
			[In] int cb,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndRead([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
			out int pcbRead
			);

		[PreserveSig]
		int Write(
			IntPtr pb,
			[In] int cb,
			out int pcbWritten
			);

		[PreserveSig]
		int BeginWrite(
			IntPtr pb,
			[In] int cb,
			[In][MarshalAs(UnmanagedType.Interface)]
			IMFAsyncCallback pCallback,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnkState
			);

		[PreserveSig]
		int EndWrite([In][MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
			out int pcbWritten
			);

		[PreserveSig]
		int Seek(
			[In] MFByteStreamSeekOrigin SeekOrigin,
			[In] long llSeekOffset,
			[In] MFByteStreamSeekingFlags dwSeekFlags,
			out long pqwCurrentPosition
			);

		[PreserveSig]
		int Flush();

		[PreserveSig]
		int Close();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFSample : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSample.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSample.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		int GetSampleFlags(
			out int pdwSampleFlags // Must be zero
			);

		[PreserveSig]
		int SetSampleFlags(
			[In] int dwSampleFlags // Must be zero
			);

		[PreserveSig]
		int GetSampleTime(
			out long phnsSampleTime
			);

		[PreserveSig]
		int SetSampleTime(
			[In] long hnsSampleTime
			);

		[PreserveSig]
		int GetSampleDuration(
			out long phnsSampleDuration
			);

		[PreserveSig]
		int SetSampleDuration(
			[In] long hnsSampleDuration
			);

		[PreserveSig]
		int GetBufferCount(
			out int pdwBufferCount
			);

		[PreserveSig]
		int GetBufferByIndex(
			[In] int dwIndex,
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
			);

		[PreserveSig]
		int ConvertToContiguousBuffer(
			[MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
			);

		[PreserveSig]
		int AddBuffer([In][MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
			);

		[PreserveSig]
		int RemoveBufferByIndex(
			[In] int dwIndex
			);

		[PreserveSig]
		int RemoveAllBuffers();

		[PreserveSig]
		int GetTotalLength(
			out int pcbTotalLength
			);

		[PreserveSig]
		int CopyToBuffer([In][MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("045FA593-8799-42B8-BC8D-8968C6453507")]
	public interface IMFMediaBuffer
	{
		[PreserveSig]
		int Lock(
			out IntPtr ppbBuffer,
			out int pcbMaxLength,
			out int pcbCurrentLength
			);

		[PreserveSig]
		int Unlock();

		[PreserveSig]
		int GetCurrentLength(
			out int pcbCurrentLength
			);

		[PreserveSig]
		int SetCurrentLength(
			[In] int cbCurrentLength
			);

		[PreserveSig]
		int GetMaxLength(
			out int pcbMaxLength
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("7DC9D5F9-9ED9-44EC-9BBF-0600BB589FBB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMF2DBuffer
	{
		[PreserveSig]
		int Lock2D(
			[Out] out IntPtr pbScanline0,
			out int plPitch
			);

		[PreserveSig]
		int Unlock2D();

		[PreserveSig]
		int GetScanline0AndPitch(
			out IntPtr pbScanline0,
			out int plPitch
			);

		[PreserveSig]
		int IsContiguousFormat(
			[MarshalAs(UnmanagedType.Bool)] out bool pfIsContiguous
			);

		[PreserveSig]
		int GetContiguousLength(
			out int pcbLength
			);

		[PreserveSig]
		int ContiguousCopyTo(
			IntPtr pbDestBuffer,
			[In] int cbDestBuffer
			);

		[PreserveSig]
		int ContiguousCopyFrom(
			[In] IntPtr pbSrcBuffer,
			[In] int cbSrcBuffer
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("B99F381F-A8F9-47A2-A5AF-CA3A225A3890")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFVideoMediaType : IMFMediaType
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFVideoMediaType.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFVideoMediaType.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		#region IMFMediaType methods

		[PreserveSig]
		new int GetMajorType(
			out Guid pguidMajorType
			);

		[PreserveSig]
		new int IsCompressedFormat(
			[MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
			);

		[PreserveSig]
		new int IsEqual([In][MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
			out MFMediaEqual pdwFlags
			);

		[PreserveSig]
		new int GetRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			out IntPtr ppvRepresentation
			);

		[PreserveSig]
		new int FreeRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			[In] IntPtr pvRepresentation
			);

		#endregion

		[PreserveSig]
		[Obsolete("This method is deprecated by MS")]
		MFVideoFormat GetVideoFormat();

		[Obsolete("This method is deprecated by MS")]
		[PreserveSig]
		int GetVideoRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			out IntPtr ppvRepresentation,
			[In] int lStride
			);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5C6C44BF-1DB6-435B-9249-E8CD10FDEC96")]
	public interface IMFPluginControl
	{
		[PreserveSig]
		int GetPreferredClsid(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPWStr)] string selector,
			out Guid clsid
		);

		[PreserveSig]
		int GetPreferredClsidByIndex(
			MFPluginType pluginType,
			int index,
			[MarshalAs(UnmanagedType.LPWStr)] out string selector,
			out Guid clsid
		);

		[PreserveSig]
		int SetPreferredClsid(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPWStr)] string selector,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFGuid clsid
		);

		[PreserveSig]
		int IsDisabled(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPStruct)] Guid clsid
		);

		[PreserveSig]
		int GetDisabledByIndex(
			MFPluginType pluginType,
			int index,
			out Guid clsid
		);

		[PreserveSig]
		int SetDisabled(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			[MarshalAs(UnmanagedType.Bool)] bool disabled
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("26A0ADC3-CE26-4672-9304-69552EDD3FAF")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Obsolete("To get the properties of the audio format, applications should use the media type attributes. If you need to convert the media type into a WAVEFORMATEX structure, call MFCreateWaveFormatExFromMFMediaType")]
	public interface IMFAudioMediaType : IMFMediaType
	{
		#region IMFAttributes methods

		[PreserveSig]
		new int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAudioMediaType.GetItem", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int GetItemType([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new int CompareItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new int GetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new int GetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new int GetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new int GetStringLength([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new int GetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPWStr)]
			StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new int GetAllocatedString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new int GetBlobSize([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new int GetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out][MarshalAs(UnmanagedType.LPArray)]
			byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new int GetAllocatedBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new int GetUnknown([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new int SetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant Value
			);

		[PreserveSig]
		new int DeleteItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new int DeleteAllItems();

		[PreserveSig]
		new int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new int SetUINT64([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new int SetDouble([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid guidValue
			);

		[PreserveSig]
		new int SetString([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPWStr)]
			string wszValue
			);

		[PreserveSig]
		new int SetBlob([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new int SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In][MarshalAs(UnmanagedType.IUnknown)]
			object pUnknown
			);

		[PreserveSig]
		new int LockStore();

		[PreserveSig]
		new int UnlockStore();

		[PreserveSig]
		new int GetCount(
			out int pcItems
			);

		[PreserveSig]
		new int GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In][Out][MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAudioMediaType.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))]
			Variant pValue
			);

		[PreserveSig]
		new int CopyAllItems([In][MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		#region IMFMediaType methods

		[PreserveSig]
		new int GetMajorType(
			out Guid pguidMajorType
			);

		[PreserveSig]
		new int IsCompressedFormat(
			[MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
			);

		[PreserveSig]
		new int IsEqual([In][MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
			out MFMediaEqual pdwFlags
			);

		[PreserveSig]
		new int GetRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			out IntPtr ppvRepresentation
			);

		[PreserveSig]
		new int FreeRepresentation([In][MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
			[In] IntPtr pvRepresentation
			);

		#endregion

		[PreserveSig]
		[Obsolete("To get the properties of the audio format, applications should use the media type attributes. If you need to convert the media type into a WAVEFORMATEX structure, call MFCreateWaveFormatExFromMFMediaType")]
		IntPtr GetAudioFormat();
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("a6b43f84-5c0a-42e8-a44d-b1857a76992f")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFByteStreamProxyClassFactory
	{
		[PreserveSig]
		int CreateByteStreamProxy(
			IMFByteStream pByteStream,
			IMFAttributes pAttributes,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
		);
	}

	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid("C6982083-3DDC-45CB-AF5E-0F7A8CE4DE77")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFPluginControl2 : IMFPluginControl
	{
		#region IMFPluginControl methods

		[PreserveSig]
		new int GetPreferredClsid(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPWStr)] string selector,
			out Guid clsid
		);

		[PreserveSig]
		new int GetPreferredClsidByIndex(
			MFPluginType pluginType,
			int index,
			[MarshalAs(UnmanagedType.LPWStr)] out string selector,
			out Guid clsid
		);

		[PreserveSig]
		new int SetPreferredClsid(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPWStr)] string selector,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			MFGuid clsid
		);

		[PreserveSig]
		new int IsDisabled(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPStruct)] Guid clsid
		);

		[PreserveSig]
		new int GetDisabledByIndex(
			MFPluginType pluginType,
			int index,
			out Guid clsid
		);

		[PreserveSig]
		new int SetDisabled(
			MFPluginType pluginType,
			[MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
			[MarshalAs(UnmanagedType.Bool)] bool disabled
		);

		#endregion

		[PreserveSig]
		int SetPolicy(
			MF_PLUGIN_CONTROL_POLICY policy
		);
	}

	[ComImport, SuppressUnmanagedCodeSecurity,
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("71604b0f-97b0-4764-8577-2f13e98a1422")]
	public interface INamedPropertyStore
	{
		[PreserveSig]
		int GetNamedValue(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "INamedPropertyStore.GetNamedValue", MarshalTypeRef = typeof(PVMarshaler))] Variant pValue
		);

		[PreserveSig]
		int SetNamedValue(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
			[In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant propvar);

		[PreserveSig]
		int GetNameCount(
			out int pdwCount);

		[PreserveSig]
		int GetNameAt(
			int iProp,
			[MarshalAs(UnmanagedType.BStr)] out string pbstrName);
	}

	[ComImport, SuppressUnmanagedCodeSecurity,
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
	public interface IPropertyStore
	{
		[PreserveSig]
		int GetCount(
			out int cProps
			);

		[PreserveSig]
		int GetAt(
			[In] int iProp,
			[Out] PropertyKey pkey
			);

		[PreserveSig]
		int GetValue(
			[In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IPropertyStore.GetValue", MarshalTypeRef = typeof(PVMarshaler))] Variant pv
			);

		[PreserveSig]
		int SetValue(
			[In, MarshalAs(UnmanagedType.LPStruct)] PropertyKey key,
			[In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant propvar
			);

		[PreserveSig]
		int Commit();
	}

	[ComImport, SuppressUnmanagedCodeSecurity,
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("0000000c-0000-0000-C000-000000000046")]
	public interface ISequentialStream
	{
		[PreserveSig]
		int Read(
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer,
			[In] int bytesCount,
			[In] IntPtr bytesRead
			);

		[PreserveSig]
		int Write(
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer,
			[In] int bytesCount,
			[In] IntPtr bytesWritten
			);
	}

	[ComImport, SuppressUnmanagedCodeSecurity,
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
	public interface IStream : ISequentialStream
	{
		#region ISequentialStream Methods

		[PreserveSig]
		new int Read(
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer,
			[In] int bytesCount,
			[In] IntPtr bytesRead
			);

		[PreserveSig]
		new int Write(
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer,
			[In] int bytesCount,
			[In] IntPtr bytesWritten
			);

		#endregion

		[PreserveSig]
		int Seek(
			[In] long offset,
			[In] SeekOrigin origin,
			[In] IntPtr newPosition
			);

		[PreserveSig]
		int SetSize(
			[In] long newSize
			);

		[PreserveSig]
		int CopyTo(
			[In] IStream otherStream,
			[In] long bytesCount,
			[In] IntPtr bytesRead,
			[In] IntPtr bytesWritten
			);

		[PreserveSig]
		int Commit(
			[In] STGC commitFlags
			);

		[PreserveSig]
		int Revert();

		[PreserveSig]
		int LockRegion(
			[In] long offset,
			[In] long bytesCount,
			[In] int lockType
			);

		[PreserveSig]
		int UnlockRegion(
			[In] long offset,
			[In] long bytesCount,
			[In] int lockType
			);

		[PreserveSig]
		int Stat(
			[Out] out STATSTG statstg,
			[In] STATFLAG statFlag
			);

		[PreserveSig]
		int Clone(
			[Out] out IStream clonedStream
			);
	}

	[ComImport, SuppressUnmanagedCodeSecurity,
	Guid("0000010c-0000-0000-C000-000000000046"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersist
	{
		[PreserveSig]
		int GetClassID(out Guid pClassID);
	}
	#endregion

	#region struct
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFINPUTTRUSTAUTHORITY_ACCESS_PARAMS")]
	public struct MFInputTrustAuthorityAccessParams
	{
		public int dwSize;
		public int dwVer;
		public int cbSignatureOffset;
		public int cbSignatureSize;
		public int cbExtensionOffset;
		public int cbExtensionSize;
		public int cActions;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public MFInputTrustAuthorityAction[] rgOutputActions;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFINPUTTRUSTAUTHORITY_ACTION")]
	public struct MFInputTrustAuthorityAction
	{
		public MFPolicyManagerAction Action;
		public IntPtr pbTicket;
		public int cbTicket;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MT_ARBITRARY_HEADER")]
	public struct MT_ARBITRARY_HEADER
	{
		public Guid majortype;
		public Guid subtype;
		public bool bFixedSizeSamples;
		public bool bTemporalCompression;
		public int lSampleSize;
		public Guid formattype;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("ROI_AREA")]
	public struct ROI_AREA
	{
		public MFRect rect;
		public int QPDelta;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SliceHeader
	{
		short dSliceHeaderLen;
		byte[] SliceHeaderBytes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SliceHeaderSet
	{
		short dNumHeaders;
		SliceHeader[] rgstSliceheader;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MOVE_RECT
	{
		//Point SourcePoint;
		MFRect DestRect;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DIRTYRECT_INFO
	{
		int FrameNumber;
		int NumDirtyRects;
		MFRect[] DirtyRects;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MOVEREGION_INFO
	{
		int FrameNumber;
		int NumMoveRegions;
		MOVE_RECT[] MoveRegions;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FaceRectInfoBlobHeader
	{
		int Size;
		int Count;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FaceRectInfo
	{
		MFRect Region;
		int confidenceLevel;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FaceCharacterizationBlobHeader
	{
		int Size;
		int Count;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FaceCharacterization
	{
		int BlinkScoreLeft;
		int BlinkScoreRight;
		int FacialExpression;
		int FacialExpressionScore;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct CapturedMetadataExposureCompensation
	{
		long Flags;
		int Value;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct CapturedMetadataISOGains
	{
		float AnalogGain;
		float DigitalGain;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct CapturedMetadataWhiteBalanceGains
	{
		float R;
		float G;
		float B;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MetadataTimeStamps
	{
		int Flags;
		long Device;
		long Presentation;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HistogramGrid
	{
		int Width;
		int Height;

		MFRect Region;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HistogramBlobHeader
	{
		int Size;
		int Histograms;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HistogramHeader
	{
		int Size;
		int Bins;
		int FourCC;
		MFChannelMasks ChannelMasks;
		HistogramGrid Grid;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HistogramDataHeader
	{
		int Size;
		int ChannelMask;
		int Linear;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MFCONTENTPROTECTIONDEVICE_INPUT_DATA
	{
		int HWProtectionFunctionID;
		int PrivateDataByteCount;
		int HWProtectionDataByteCount;
		int Reserved;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		byte[] InputData;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MFCONTENTPROTECTIONDEVICE_OUTPUT_DATA
	{
		int PrivateDataByteCount;
		int MaxHWProtectionDataByteCount;
		int HWProtectionDataByteCount;
		int Status;
		long TransportTimeInHundredsOfNanoseconds;
		long ExecutionTimeInHundredsOfNanoseconds;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		byte[] OutputData;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct MFCONTENTPROTECTIONDEVICE_REALTIMECLIENT_DATA
	{
		int TaskIndex;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		string ClassName;
		int BasePriority;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	[UnmanagedName("MFPaletteEntry")]
	public struct MFPaletteEntry
	{
		[FieldOffset(0)]
		public MFARGB ARGB;
		[FieldOffset(0)]
		public MFAYUVSample AYCbCr;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[UnmanagedName("MFCLOCK_PROPERTIES")]
	public struct MFClockProperties
	{
		public long qwCorrelationRate;
		public Guid guidClockId;
		public MFClockRelationalFlags dwClockFlags;
		public long qwClockFrequency;
		public int dwClockTolerance;
		public int dwClockJitter;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MF_TRANSCODE_SINK_INFO")]
	public struct MFTranscodeSinkInfo
	{
		public int dwVideoStreamID;
		public IMFMediaType pVideoMediaType;
		public int dwAudioStreamID;
		public IMFMediaType pAudioMediaType;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 8)]
	[UnmanagedName("MFTOPONODE_ATTRIBUTE_UPDATE")]
	public struct MFTopoNodeAttributeUpdate
	{
		[FieldOffset(0)]
		public long NodeId;
		[FieldOffset(8)]
		public Guid guidAttributeKey;
		[FieldOffset(24)]
		public MFAttributeType attrType;
		[FieldOffset(32)]
		public int u32;
		[FieldOffset(32)]
		public long u64;
		[FieldOffset(32)]
		public double d;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MF_BYTE_STREAM_CACHE_RANGE")]
	public struct MF_BYTE_STREAM_CACHE_RANGE
	{
		public long qwStartOffset;
		public long qwEndOffset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[UnmanagedName("MFVideoCompressedInfo")]
	public struct MFVideoCompressedInfo
	{
		public long AvgBitrate;
		public long AvgBitErrorRate;
		public int MaxKeyFrameSpacing;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFVideoSurfaceInfo")]
	public struct MFVideoSurfaceInfo
	{
		public int Format;
		public int PaletteEntries;
		public MFPaletteEntry[] Palette;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFRatio")]
	public struct MFRatio
	{
		public int Numerator;
		public int Denominator;

		public MFRatio(int n, int d)
		{
			Numerator = n;
			Denominator = d;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[UnmanagedName("MFVideoInfo")]
	public struct MFVideoInfo
	{
		public int dwWidth;
		public int dwHeight;
		public MFRatio PixelAspectRatio;
		public MFVideoChromaSubsampling SourceChromaSubsampling;
		public MFVideoInterlaceMode InterlaceMode;
		public MFVideoTransferFunction TransferFunction;
		public MFVideoPrimaries ColorPrimaries;
		public MFVideoTransferMatrix TransferMatrix;
		public MFVideoLighting SourceLighting;
		public MFRatio FramesPerSecond;
		public MFNominalRange NominalRange;
		public MFVideoArea GeometricAperture;
		public MFVideoArea MinimumDisplayAperture;
		public MFVideoArea PanScanAperture;
		public MFVideoFlags VideoFlags;
	}

	[UnmanagedName("MT_CUSTOM_VIDEO_PRIMARIES"), StructLayout(LayoutKind.Sequential)]
	public struct MT_CustomVideoPrimaries
	{
		public float fRx;
		public float fRy;
		public float fGx;
		public float fGy;
		public float fBx;
		public float fBy;
		public float fWx;
		public float fWy;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("STATSTG")]
	public struct STATSTG
	{
		public IntPtr pwcsName;
		public STGTY type;
		public long cbSize;
		public long mtime;
		public long ctime;
		public long atime;
		public int grfMode;
		public int grfLocksSupported;
		public Guid clsid;
		public int grfStateBits;
		public int reserved;
	}
	#endregion

	#region classes
	[StructLayout(LayoutKind.Sequential)]
	public class MFFloat
	{
		private readonly float Value;

		public MFFloat()
			: this(0)
		{
		}

		public MFFloat(float v) { Value = v; }

		[NotNull]
		public override string ToString() { return Value.ToString(CultureInfo.CurrentCulture); }

		public override int GetHashCode() { return Value.GetHashCode(); }

		public static implicit operator float([NotNull] MFFloat l) { return l.Value; }

		[NotNull]
		public static implicit operator MFFloat(float l) { return new MFFloat(l); }
	}

	/// <summary>
	///     ConstPropVariant is used for [In] parameters.  This is important since
	///     for [In] parameters, you must *not* clear the PropVariant.  The caller
	///     will need to do that himself.
	///     Likewise, if you want to store a copy of a ConstPropVariant, you should
	///     store it to a PropVariant using the PropVariant constructor that takes a
	///     ConstPropVariant.  If you try to store the ConstPropVariant, when the
	///     caller frees his copy, yours will no longer be valid.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public class ConstPropVariant : IDisposable
	{
		[SecurityCritical]
		[DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
		protected static extern void PropVariantCopy([Out][MarshalAs(UnmanagedType.LPStruct)] Variant pvarDest,
			[In][MarshalAs(UnmanagedType.LPStruct)]
			ConstPropVariant pvarSource);

		public enum VariantType : short
		{
			None = 0,
			Short = 2,
			Int32 = 3,
			Float = 4,
			Double = 5,
			IUnknown = 13,
			UByte = 17,
			UShort = 18,
			UInt32 = 19,
			Int64 = 20,
			UInt64 = 21,
			String = 31,
			Guid = 72,
			Blob = 0x1000 + 17,
			StringArray = 0x1000 + 31
		}

		[StructLayout(LayoutKind.Sequential)]
		[UnmanagedName("BLOB")]
		protected struct Blob
		{
			public int cbSize;
			public IntPtr pBlobData;
		}

		[StructLayout(LayoutKind.Sequential)]
		[UnmanagedName("CALPWSTR")]
		protected struct CALPWstr
		{
			public int cElems;
			public IntPtr pElems;
		}

		#region Member variables

		[FieldOffset(0)]
		protected VariantType type;

		[FieldOffset(2)]
		protected short reserved1;

		[FieldOffset(4)]
		protected short reserved2;

		[FieldOffset(6)]
		protected short reserved3;

		[FieldOffset(8)]
		protected short iVal;

		[FieldOffset(8)]
		protected ushort uiVal;

		[FieldOffset(8)]
		protected byte bVal;

		[FieldOffset(8)]
		protected int intValue;

		[FieldOffset(8)]
		protected uint uintVal;

		[FieldOffset(8)]
		protected float fltVal;

		[FieldOffset(8)]
		protected long longValue;

		[FieldOffset(8)]
		protected ulong ulongValue;

		[FieldOffset(8)]
		protected double doubleValue;

		[FieldOffset(8)]
		protected Blob blobValue;

		[FieldOffset(8)]
		protected IntPtr ptr;

		[FieldOffset(8)]
		protected CALPWstr calpwstrVal;

		#endregion

		public ConstPropVariant()
			: this(VariantType.None)
		{
		}

		protected ConstPropVariant(VariantType v) { type = v; }

		public static explicit operator string([NotNull] ConstPropVariant f) { return f.GetString(); }

		[NotNull]
		public static explicit operator string[]([NotNull] ConstPropVariant f) { return f.GetStringArray(); }

		public static explicit operator byte([NotNull] ConstPropVariant f) { return f.GetUByte(); }

		public static explicit operator short([NotNull] ConstPropVariant f) { return f.GetShort(); }

		public static explicit operator ushort([NotNull] ConstPropVariant f) { return f.GetUShort(); }

		public static explicit operator int([NotNull] ConstPropVariant f) { return f.GetInt(); }

		public static explicit operator uint([NotNull] ConstPropVariant f) { return f.GetUInt(); }

		public static explicit operator float([NotNull] ConstPropVariant f) { return f.GetFloat(); }

		public static explicit operator double([NotNull] ConstPropVariant f) { return f.GetDouble(); }

		public static explicit operator long([NotNull] ConstPropVariant f) { return f.GetLong(); }

		public static explicit operator ulong([NotNull] ConstPropVariant f) { return f.GetULong(); }

		public static explicit operator Guid([NotNull] ConstPropVariant f) { return f.GetGuid(); }

		[NotNull]
		public static explicit operator byte[]([NotNull] ConstPropVariant f) { return f.GetBlob(); }

		// I decided not to do implicit since perf is likely to be
		// better recycling the PropVariant, and the only way I can
		// see to support Implicit is to create a new PropVariant.
		// Also, since I can't free the previous instance, IUnknowns
		// will linger until the GC cleans up.  Not what I think I
		// want.

		public MFAttributeType GetMFAttributeType()
		{
			switch (type)
			{
				case VariantType.None:
				case VariantType.UInt32:
				case VariantType.UInt64:
				case VariantType.Double:
				case VariantType.Guid:
				case VariantType.String:
				case VariantType.Blob:
				case VariantType.IUnknown:
					return (MFAttributeType)type;
				default:
					throw new Exception("Type is not a MFAttributeType");
			}
		}

		public VariantType GetVariantType() { return type; }

		[NotNull]
		public string[] GetStringArray()
		{
			if (type == VariantType.StringArray)
			{
				int iCount = calpwstrVal.cElems;
				string[] sa = new string[iCount];

				for (int x = 0; x < iCount; x++) sa[x] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(calpwstrVal.pElems, x * IntPtr.Size));

				return sa;
			}

			throw new ArgumentException("PropVariant contents not a string array");
		}

		public string GetString()
		{
			if (type == VariantType.String) return Marshal.PtrToStringUni(ptr);
			throw new ArgumentException("PropVariant contents not a string");
		}

		public byte GetUByte()
		{
			if (type == VariantType.UByte) return bVal;
			throw new ArgumentException("PropVariant contents not a byte");
		}

		public short GetShort()
		{
			if (type == VariantType.Short) return iVal;
			throw new ArgumentException("PropVariant contents not a Short");
		}

		public ushort GetUShort()
		{
			if (type == VariantType.UShort) return uiVal;
			throw new ArgumentException("PropVariant contents not a UShort");
		}

		public int GetInt()
		{
			if (type == VariantType.Int32) return intValue;
			throw new ArgumentException("PropVariant contents not an int32");
		}

		public uint GetUInt()
		{
			if (type == VariantType.UInt32) return uintVal;
			throw new ArgumentException("PropVariant contents not a uint32");
		}

		public long GetLong()
		{
			if (type == VariantType.Int64) return longValue;
			throw new ArgumentException("PropVariant contents not an int64");
		}

		public ulong GetULong()
		{
			if (type == VariantType.UInt64) return ulongValue;
			throw new ArgumentException("PropVariant contents not a uint64");
		}

		public float GetFloat()
		{
			if (type == VariantType.Float) return fltVal;
			throw new ArgumentException("PropVariant contents not a Float");
		}

		public double GetDouble()
		{
			if (type == VariantType.Double) return doubleValue;
			throw new ArgumentException("PropVariant contents not a double");
		}

		public Guid GetGuid()
		{
			if (type == VariantType.Guid) return (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
			throw new ArgumentException("PropVariant contents not a Guid");
		}

		[NotNull]
		public byte[] GetBlob()
		{
			if (type == VariantType.Blob)
			{
				byte[] b = new byte[blobValue.cbSize];

				if (blobValue.cbSize > 0) Marshal.Copy(blobValue.pBlobData, b, 0, blobValue.cbSize);

				return b;
			}

			throw new ArgumentException("PropVariant contents not a Blob");
		}

		public object GetBlob(Type t, int offset)
		{
			if (type == VariantType.Blob)
			{
				object o;

				if (blobValue.cbSize > offset)
				{
					if (blobValue.cbSize >= Marshal.SizeOf(t) + offset) o = Marshal.PtrToStructure(blobValue.pBlobData + offset, t);
					else throw new ArgumentException("Blob wrong size");
				}
				else o = null;

				return o;
			}

			throw new ArgumentException("PropVariant contents not a Blob");
		}

		public object GetBlob(Type t) { return GetBlob(t, 0); }

		public object GetIUnknown()
		{
			if (type == VariantType.IUnknown)
			{
				if (ptr != IntPtr.Zero) return Marshal.GetObjectForIUnknown(ptr);

				return null;
			}

			throw new ArgumentException("PropVariant contents not an IUnknown");
		}

		public void Copy([NotNull] Variant pdest)
		{
			if (pdest == null) throw new Exception("Null PropVariant sent to Copy");

			// Copy doesn't clear the dest.
			pdest.Clear();

			PropVariantCopy(pdest, this);
		}

		public override string ToString()
		{
			// This method is primarily intended for debugging so that a readable string will show
			// up in the output window
			string sRet;

			switch (type)
			{
				case VariantType.None:
				{
					sRet = "<Empty>";
					break;
				}

				case VariantType.Blob:
				{
					const string FormatString = "x2"; // Hex 2 digit format
					const int MaxEntries = 16;

					byte[] blob = GetBlob();

					// Number of bytes we're going to format
					int n = Math.Min(MaxEntries, blob.Length);

					if (n == 0) sRet = "<Empty Array>";
					else
					{
						// Only format the first MaxEntries bytes
						sRet = blob[0].ToString(FormatString);
						for (int i = 1; i < n; i++) sRet += ',' + blob[i].ToString(FormatString);

						// If the string is longer, add an indicator
						if (blob.Length > n) sRet += "...";
					}

					break;
				}

				case VariantType.Float:
				{
					sRet = GetFloat().ToString(CultureInfo.CurrentCulture);
					break;
				}

				case VariantType.Double:
				{
					sRet = GetDouble().ToString(CultureInfo.CurrentCulture);
					break;
				}

				case VariantType.Guid:
				{
					sRet = GetGuid().ToString();
					break;
				}

				case VariantType.IUnknown:
				{
					object o = GetIUnknown();
					sRet = o != null
								? GetIUnknown().ToString()
								: "<null>";
					break;
				}

				case VariantType.String:
				{
					sRet = GetString();
					break;
				}

				case VariantType.Short:
				{
					sRet = GetShort().ToString();
					break;
				}

				case VariantType.UByte:
				{
					sRet = GetUByte().ToString();
					break;
				}

				case VariantType.UShort:
				{
					sRet = GetUShort().ToString();
					break;
				}

				case VariantType.Int32:
				{
					sRet = GetInt().ToString();
					break;
				}

				case VariantType.UInt32:
				{
					sRet = GetUInt().ToString();
					break;
				}

				case VariantType.Int64:
				{
					sRet = GetLong().ToString();
					break;
				}

				case VariantType.UInt64:
				{
					sRet = GetULong().ToString();
					break;
				}

				case VariantType.StringArray:
				{
					sRet = "";
					foreach (string entry in GetStringArray())
					{
						sRet += (sRet.Length == 0
									? "\""
									: ",\"") +
								entry +
								'\"';
					}

					break;
				}
				default:
				{
					sRet = base.ToString();
					break;
				}
			}

			return sRet;
		}

		public override int GetHashCode()
		{
			// Give a (slightly) better hash value in case someone uses PropVariants
			// in a hash table.
			int iRet;

			switch (type)
			{
				case VariantType.None:
					iRet = RuntimeHelpers.GetHashCode(this);
					break;
				case VariantType.Blob:
				{
					iRet = GetBlob().GetHashCode();
					break;
				}

				case VariantType.Float:
				{
					iRet = GetFloat().GetHashCode();
					break;
				}

				case VariantType.Double:
				{
					iRet = GetDouble().GetHashCode();
					break;
				}

				case VariantType.Guid:
				{
					iRet = GetGuid().GetHashCode();
					break;
				}

				case VariantType.IUnknown:
				{
					iRet = GetIUnknown().GetHashCode();
					break;
				}

				case VariantType.String:
				{
					iRet = GetString().GetHashCode();
					break;
				}

				case VariantType.UByte:
				{
					iRet = GetUByte().GetHashCode();
					break;
				}

				case VariantType.Short:
				{
					iRet = GetShort().GetHashCode();
					break;
				}

				case VariantType.UShort:
				{
					iRet = GetUShort().GetHashCode();
					break;
				}

				case VariantType.Int32:
				{
					iRet = GetInt().GetHashCode();
					break;
				}

				case VariantType.UInt32:
				{
					iRet = GetUInt().GetHashCode();
					break;
				}

				case VariantType.Int64:
				{
					iRet = GetLong().GetHashCode();
					break;
				}

				case VariantType.UInt64:
				{
					iRet = GetULong().GetHashCode();
					break;
				}

				case VariantType.StringArray:
				{
					iRet = GetStringArray().GetHashCode();
					break;
				}
				default:
				{
					iRet = RuntimeHelpers.GetHashCode(this);
					break;
				}
			}

			return iRet;
		}

		public override bool Equals(object obj)
		{
			bool bRet;
			Variant p = obj as Variant;

			if ((object)p == null || p.type != type) bRet = false;
			else
			{
				switch (type)
				{
					case VariantType.None:
					{
						bRet = true;
						break;
					}

					case VariantType.Blob:
					{
						byte[] b1 = GetBlob();
						byte[] b2 = p.GetBlob();

						if (b1.Length == b2.Length)
						{
							bRet = true;
							for (int x = 0; x < b1.Length; x++)
								if (b1[x] != b2[x])
								{
									bRet = false;
									break;
								}
						}
						else bRet = false;

						break;
					}

					case VariantType.Float:
					{
						bRet = Math.Abs(GetFloat() - p.GetFloat()) < float.Epsilon;
						break;
					}

					case VariantType.Double:
					{
						bRet = Math.Abs(GetDouble() - p.GetDouble()) < double.Epsilon;
						break;
					}

					case VariantType.Guid:
					{
						bRet = GetGuid() == p.GetGuid();
						break;
					}

					case VariantType.IUnknown:
					{
						bRet = GetIUnknown() == p.GetIUnknown();
						break;
					}

					case VariantType.String:
					{
						bRet = GetString() == p.GetString();
						break;
					}

					case VariantType.UByte:
					{
						bRet = GetUByte() == p.GetUByte();
						break;
					}

					case VariantType.Short:
					{
						bRet = GetShort() == p.GetShort();
						break;
					}

					case VariantType.UShort:
					{
						bRet = GetUShort() == p.GetUShort();
						break;
					}

					case VariantType.Int32:
					{
						bRet = GetInt() == p.GetInt();
						break;
					}

					case VariantType.UInt32:
					{
						bRet = GetUInt() == p.GetUInt();
						break;
					}

					case VariantType.Int64:
					{
						bRet = GetLong() == p.GetLong();
						break;
					}

					case VariantType.UInt64:
					{
						bRet = GetULong() == p.GetULong();
						break;
					}

					case VariantType.StringArray:
					{
						string[] sa1 = GetStringArray();
						string[] sa2 = p.GetStringArray();

						if (sa1.Length == sa2.Length)
						{
							bRet = true;
							for (int x = 0; x < sa1.Length; x++)
								if (sa1[x] != sa2[x])
								{
									bRet = false;
									break;
								}
						}
						else bRet = false;

						break;
					}
					default:
					{
						bRet = RuntimeHelpers.Equals(this, obj);
						break;
					}
				}
			}

			return bRet;
		}

		public static bool operator ==(ConstPropVariant pv1, ConstPropVariant pv2)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(pv1, pv2)) return true;

			// If one is null, but not both, return false.
			if ((object)pv1 == null || (object)pv2 == null) return false;

			return pv1.Equals(pv2);
		}

		public static bool operator !=(ConstPropVariant pv1, ConstPropVariant pv2) { return !(pv1 == pv2); }

		#region IDisposable Members

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() { Dispose(true); }

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources;
		///     <c>false</c> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			// If we are a ConstPropVariant, we must *not* call PropVariantClear.  That
			// would release the *caller's* copy of the data, which would probably make
			// him cranky.  If we are a PropVariant, the PropVariant.Dispose gets called
			// as well, which *does* do a PropVariantClear.
			type = VariantType.None;
#if DEBUG
			longValue = 0;
#endif
		}

		#endregion
	}

	[StructLayout(LayoutKind.Explicit)]
	public class Variant : ConstPropVariant
	{
		#region Declarations

		[SecurityCritical]
		[DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
		protected static extern void PropVariantClear([In][MarshalAs(UnmanagedType.LPStruct)] Variant pvar);

		#endregion

		public Variant()
			: base(VariantType.None)
		{
		}

		public Variant(string value)
			: base(VariantType.String)
		{
			ptr = Marshal.StringToCoTaskMemUni(value);
		}

		public Variant([NotNull] string[] value)
			: base(VariantType.StringArray)
		{
			calpwstrVal.cElems = value.Length;
			calpwstrVal.pElems = Marshal.AllocCoTaskMem(IntPtr.Size * value.Length);

			for (int x = 0; x < value.Length; x++) Marshal.WriteIntPtr(calpwstrVal.pElems, x * IntPtr.Size, Marshal.StringToCoTaskMemUni(value[x]));
		}

		public Variant(byte value)
			: base(VariantType.UByte)
		{
			bVal = value;
		}

		public Variant(short value)
			: base(VariantType.Short)
		{
			iVal = value;
		}

		public Variant(ushort value)
			: base(VariantType.UShort)
		{
			uiVal = value;
		}

		public Variant(int value)
			: base(VariantType.Int32)
		{
			intValue = value;
		}

		public Variant(uint value)
			: base(VariantType.UInt32)
		{
			uintVal = value;
		}

		public Variant(float value)
			: base(VariantType.Float)
		{
			fltVal = value;
		}

		public Variant(double value)
			: base(VariantType.Double)
		{
			doubleValue = value;
		}

		public Variant(long value)
			: base(VariantType.Int64)
		{
			longValue = value;
		}

		public Variant(ulong value)
			: base(VariantType.UInt64)
		{
			ulongValue = value;
		}

		public Variant(Guid value)
			: base(VariantType.Guid)
		{
			ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
			Marshal.StructureToPtr(value, ptr, false);
		}

		public Variant([NotNull] byte[] value)
			: base(VariantType.Blob)
		{
			blobValue.cbSize = value.Length;
			blobValue.pBlobData = Marshal.AllocCoTaskMem(value.Length);
			Marshal.Copy(value, 0, blobValue.pBlobData, value.Length);
		}

		public Variant(object value)
			: base(VariantType.IUnknown)
		{
			if (value == null) ptr = IntPtr.Zero;
			else if (Marshal.IsComObject(value)) ptr = Marshal.GetIUnknownForObject(value);
			else
			{
				type = VariantType.Blob;

				blobValue.cbSize = Marshal.SizeOf(value);
				blobValue.pBlobData = Marshal.AllocCoTaskMem(blobValue.cbSize);

				Marshal.StructureToPtr(value, blobValue.pBlobData, false);
			}
		}

		public Variant(IntPtr value)
			: base(VariantType.None)
		{
			Marshal.PtrToStructure(value, this);
		}

		public Variant([NotNull] ConstPropVariant value) { PropVariantCopy(this, value); }

		~Variant() { Dispose(false); }

		public void Clear() { PropVariantClear(this); }

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			Clear();
			if (disposing) GC.SuppressFinalize(this);
		}

		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	public class FourCC
	{
		private int m_fourCC;

		public FourCC([NotNull] string fcc)
		{
			if (fcc.Length != 4) throw new ArgumentException(fcc + " is not a valid FourCC");

			byte[] asc = Encoding.ASCII.GetBytes(fcc);

			LoadFromBytes(asc[0], asc[1], asc[2], asc[3]);
		}

		public FourCC(char a, char b, char c, char d) { LoadFromBytes((byte)a, (byte)b, (byte)c, (byte)d); }

		public FourCC(int fcc) { m_fourCC = fcc; }

		public FourCC(byte a, byte b, byte c, byte d) { LoadFromBytes(a, b, c, d); }

		public FourCC(Guid g)
		{
			if (!IsA4ccSubtype(g)) throw new Exception("Not a FourCC Guid");

			byte[] asc = g.ToByteArray();

			LoadFromBytes(asc[0], asc[1], asc[2], asc[3]);
		}

		public void LoadFromBytes(byte a, byte b, byte c, byte d) { m_fourCC = a | (b << 8) | (c << 16) | (d << 24); }

		public int ToInt32() { return m_fourCC; }

		public static explicit operator int([NotNull] FourCC f) { return f.ToInt32(); }

		public static explicit operator Guid([NotNull] FourCC f) { return f.ToMediaSubtype(); }

		public Guid ToMediaSubtype() { return new Guid(m_fourCC, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71); }

		public static bool operator ==(FourCC fcc1, FourCC fcc2)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(fcc1, fcc2)) return true;

			// If one is null, but not both, return false.
			if ((object)fcc1 == null || (object)fcc2 == null) return false;

			return fcc1.m_fourCC == fcc2.m_fourCC;
		}

		public static bool operator !=(FourCC fcc1, FourCC fcc2) { return !(fcc1 == fcc2); }

		public override bool Equals(object obj) { return obj is FourCC cc && cc.m_fourCC == m_fourCC; }

		public override int GetHashCode() { return m_fourCC.GetHashCode(); }

		[NotNull]
		public override string ToString()
		{
			char[] ca = new char[4];

			char c = Convert.ToChar(m_fourCC & 255);
			if (!char.IsLetterOrDigit(c)) c = ' ';
			ca[0] = c;

			c = Convert.ToChar((m_fourCC >> 8) & 255);
			if (!char.IsLetterOrDigit(c)) c = ' ';
			ca[1] = c;

			c = Convert.ToChar((m_fourCC >> 16) & 255);
			if (!char.IsLetterOrDigit(c)) c = ' ';
			ca[2] = c;

			c = Convert.ToChar((m_fourCC >> 24) & 255);
			if (!char.IsLetterOrDigit(c)) c = ' ';
			ca[3] = c;

			string s = new string(ca);

			return s;
		}

		public static bool IsA4ccSubtype(Guid g) { return g.ToString().EndsWith("-0000-0010-8000-00aa00389b71"); }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[UnmanagedName("WAVEFORMATEX")]
	public class WaveFormatEx
	{
		public short wFormatTag;
		public short nChannels;
		public int nSamplesPerSec;
		public int nAvgBytesPerSec;
		public short nBlockAlign;
		public short wBitsPerSample;
		public short cbSize;

		[NotNull]
		public override string ToString() { return string.Format("{0} {1} {2} {3}", wFormatTag, nChannels, nSamplesPerSec, wBitsPerSample); }

		public IntPtr GetPtr()
		{
			IntPtr ip;

			switch (this)
			{
				// See what kind of WaveFormat object we've got
				case WaveFormatExtensibleWithData:
				{
					int iExtensibleSize = Marshal.SizeOf(typeof(WaveFormatExtensible));
					int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

					// WaveFormatExtensibleWithData - Have to copy the byte array too
					WaveFormatExtensibleWithData pData = (WaveFormatExtensibleWithData)this;

					int iExtraBytes = pData.cbSize - (iExtensibleSize - iWaveFormatExSize);

					// Account for copying the array.  This may result in us allocating more bytes than we
					// need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
					int iUseSize = Math.Max(iExtraBytes, IntPtr.Size);

					// Remember, cbSize include the length of WaveFormatExtensible
					ip = Marshal.AllocCoTaskMem(iExtensibleSize + iUseSize);

					// Copies the waveformatex + waveformatextensible
					Marshal.StructureToPtr(pData, ip, false);

					// Get a pointer to the byte after the copy
					IntPtr ip2 = ip + iExtensibleSize;

					// Copy the extra bytes
					Marshal.Copy(pData.byteData, 0, ip2, pData.cbSize - (iExtensibleSize - iWaveFormatExSize));
					break;
				}
				case WaveFormatExtensible:
				{
					int iWaveFormatExtensibleSize = Marshal.SizeOf(typeof(WaveFormatExtensible));

					// WaveFormatExtensible - Just do a simple copy

					ip = Marshal.AllocCoTaskMem(iWaveFormatExtensibleSize);

					Marshal.StructureToPtr(this as WaveFormatExtensible, ip, false);
					break;
				}
				case WaveFormatExWithData:
				{
					int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

					// WaveFormatExWithData - Have to copy the byte array too
					WaveFormatExWithData pData = (WaveFormatExWithData)this;

					// Account for copying the array.  This may result in us allocating more bytes than we
					// need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
					int iUseSize = Math.Max(pData.cbSize, IntPtr.Size);

					ip = Marshal.AllocCoTaskMem(iWaveFormatExSize + iUseSize);

					Marshal.StructureToPtr(pData, ip, false);

					IntPtr ip2 = ip + iWaveFormatExSize;
					Marshal.Copy(pData.byteData, 0, ip2, pData.cbSize);
					break;
				}
				default:
				{
					int iWaveFormatExSize = Marshal.SizeOf(typeof(WaveFormatEx));

					// WaveFormatEx - just do a copy
					ip = Marshal.AllocCoTaskMem(iWaveFormatExSize);
					Marshal.StructureToPtr(this, ip, false);
					break;
				}
			}

			return ip;
		}

		[NotNull]
		public static WaveFormatEx PtrToWave(IntPtr pNativeData)
		{
			short wFormatTag = Marshal.ReadInt16(pNativeData);
			WaveFormatEx wfe;

			// WAVE_FORMAT_EXTENSIBLE == -2
			if (wFormatTag != -2)
			{
				// By spec, PCM has no cbSize element
				short cbSize = wFormatTag != 1
									? Marshal.ReadInt16(pNativeData, 16)
									: (short)0;

				// Does the structure contain extra data?
				if (cbSize == 0)
				{
					// Create a simple WaveFormatEx struct
					wfe = new WaveFormatEx();
					Marshal.PtrToStructure(pNativeData, wfe);

					// It probably already has the right value, but there is a special case
					// where it might not, so, just to be safe...
					wfe.cbSize = 0;
				}
				else
				{
					WaveFormatExWithData dat = new WaveFormatExWithData
					{
						// Manually parse the data into the structure
						wFormatTag = wFormatTag,
						nChannels = Marshal.ReadInt16(pNativeData, 2),
						nSamplesPerSec = Marshal.ReadInt32(pNativeData, 4),
						nAvgBytesPerSec = Marshal.ReadInt32(pNativeData, 8),
						nBlockAlign = Marshal.ReadInt16(pNativeData, 12),
						wBitsPerSample = Marshal.ReadInt16(pNativeData, 14),
						cbSize = cbSize
					};

					dat.byteData = new byte[dat.cbSize];
					IntPtr ip2 = pNativeData + 18;
					Marshal.Copy(ip2, dat.byteData, 0, dat.cbSize);

					wfe = dat;
				}
			}
			else
			{
				int extraSize = Marshal.SizeOf(typeof(WaveFormatExtensible)) - Marshal.SizeOf(typeof(WaveFormatEx));

				short cbSize = Marshal.ReadInt16(pNativeData, 16);
				if (cbSize == extraSize)
				{
					WaveFormatExtensible ext = new WaveFormatExtensible();
					Marshal.PtrToStructure(pNativeData, ext);
					wfe = ext;
				}
				else
				{
					WaveFormatExtensibleWithData ext = new WaveFormatExtensibleWithData();
					int iExtraBytes = cbSize - extraSize;

					ext.wFormatTag = wFormatTag;
					ext.nChannels = Marshal.ReadInt16(pNativeData, 2);
					ext.nSamplesPerSec = Marshal.ReadInt32(pNativeData, 4);
					ext.nAvgBytesPerSec = Marshal.ReadInt32(pNativeData, 8);
					ext.nBlockAlign = Marshal.ReadInt16(pNativeData, 12);
					ext.wBitsPerSample = Marshal.ReadInt16(pNativeData, 14);
					ext.cbSize = cbSize;

					ext.wValidBitsPerSample = Marshal.ReadInt16(pNativeData, 18);
					ext.dwChannelMask = (WaveMask)Marshal.ReadInt16(pNativeData, 20);

					// Read the Guid
					byte[] byteGuid = new byte[16];
					Marshal.Copy(pNativeData + 24, byteGuid, 0, 16);
					ext.SubFormat = new Guid(byteGuid);

					ext.byteData = new byte[iExtraBytes];
					IntPtr ip2 = pNativeData + Marshal.SizeOf(typeof(WaveFormatExtensible));
					Marshal.Copy(ip2, ext.byteData, 0, iExtraBytes);

					wfe = ext;
				}
			}

			return wfe;
		}

		public static bool operator ==(WaveFormatEx a, WaveFormatEx b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b)) return true;

			// If one is null, but not both, return false.
			if ((object)a == null || (object)b == null) return false;

			bool bRet;
			Type t1 = a.GetType();
			Type t2 = b.GetType();

			if (t1 == t2 &&
				a.wFormatTag == b.wFormatTag &&
				a.nChannels == b.nChannels &&
				a.nSamplesPerSec == b.nSamplesPerSec &&
				a.nAvgBytesPerSec == b.nAvgBytesPerSec &&
				a.nBlockAlign == b.nBlockAlign &&
				a.wBitsPerSample == b.wBitsPerSample &&
				a.cbSize == b.cbSize) bRet = true;
			else bRet = false;

			return bRet;
		}

		public static bool operator !=(WaveFormatEx a, WaveFormatEx b) { return !(a == b); }

		public override bool Equals(object obj) { return this == obj as WaveFormatEx; }

		public override int GetHashCode() { return nAvgBytesPerSec + wFormatTag; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[UnmanagedName("WAVEFORMATEX")]
	public class WaveFormatExWithData : WaveFormatEx
	{
		public byte[] byteData;

		public static bool operator ==(WaveFormatExWithData a, WaveFormatExWithData b)
		{
			bool bRet = a == (WaveFormatEx)b;
			if (!bRet) return false;

			if (b?.byteData == null)
			{
				bRet = a?.byteData == null;
			}
			else
			{
				if (b.byteData.Length == a.byteData.Length)
				{
					for (int x = 0; x < b.byteData.Length; x++)
					{
						if (b.byteData[x] == a.byteData[x]) continue;
						bRet = false;
						break;
					}
				}
				else
				{
					bRet = false;
				}
			}

			return bRet;
		}

		public static bool operator !=(WaveFormatExWithData a, WaveFormatExWithData b) { return !(a == b); }

		public override bool Equals(object obj) { return this == obj as WaveFormatExWithData; }

		public override int GetHashCode() { return base.GetHashCode(); }
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	[UnmanagedName("WAVEFORMATEXTENSIBLE")]
	public class WaveFormatExtensible : WaveFormatEx
	{
		[FieldOffset(0)]
		public short wValidBitsPerSample;

		[FieldOffset(0)]
		public short wSamplesPerBlock;

		[FieldOffset(0)]
		public short wReserved;

		[FieldOffset(2)]
		public WaveMask dwChannelMask;

		[FieldOffset(6)]
		public Guid SubFormat;

		public static bool operator ==(WaveFormatExtensible a, WaveFormatExtensible b)
		{
			if (ReferenceEquals(a, b)) return true;

			bool bRet = a == (WaveFormatEx)b;

			if (bRet)
			{
				bRet = a.wValidBitsPerSample == b.wValidBitsPerSample &&
						a.dwChannelMask == b.dwChannelMask &&
						a.SubFormat == b.SubFormat;
			}

			return bRet;
		}

		public static bool operator !=(WaveFormatExtensible a, WaveFormatExtensible b) { return !(a == b); }

		public override bool Equals(object obj) { return this == obj as WaveFormatExtensible; }

		public override int GetHashCode() { return base.GetHashCode(); }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[UnmanagedName("WAVEFORMATEX")]
	public class WaveFormatExtensibleWithData : WaveFormatExtensible
	{
		public byte[] byteData;

		public static bool operator ==(WaveFormatExtensibleWithData a, WaveFormatExtensibleWithData b)
		{
			if (ReferenceEquals(a, b)) return true;

			bool bRet = a == (WaveFormatExtensible)b;

			if (!bRet) return false;

			if (b.byteData == null)
			{
				bRet = a.byteData == null;
			}
			else
			{
				if (b.byteData.Length == a.byteData.Length)
				{
					for (int x = 0; x < b.byteData.Length; x++)
					{
						if (b.byteData[x] == a.byteData[x]) continue;
						bRet = false;
						break;
					}
				}
				else
				{
					bRet = false;
				}
			}

			return bRet;
		}

		public static bool operator !=(WaveFormatExtensibleWithData a, WaveFormatExtensibleWithData b) { return !(a == b); }

		public override bool Equals(object obj) { return this == obj as WaveFormatExtensibleWithData; }

		public override int GetHashCode() { return base.GetHashCode(); }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("BITMAPINFOHEADER")]
	public class BitmapInfoHeader
	{
		public int Size;
		public int Width;
		public int Height;
		public short Planes;
		public short BitCount;
		public int Compression;
		public int ImageSize;
		public int XPelsPerMeter;
		public int YPelsPerMeter;
		public int ClrUsed;
		public int ClrImportant;

		public IntPtr GetPtr()
		{
			IntPtr ip;

			switch (this)
			{
				// See what kind of BitmapInfoHeader object we've got
				case BitmapInfoHeaderWithData:
				{
					int iBitmapInfoHeaderSize = Marshal.SizeOf(typeof(BitmapInfoHeader));

					// BitmapInfoHeaderWithData - Have to copy the array too
					BitmapInfoHeaderWithData pData = (BitmapInfoHeaderWithData)this;

					// Account for copying the array.  This may result in us allocating more bytes than we
					// need (if cbSize < IntPtr.Size), but it prevents us from overrunning the buffer.
					int iUseSize = Math.Max(pData.bmiColors.Length * 4, IntPtr.Size);

					ip = Marshal.AllocCoTaskMem(iBitmapInfoHeaderSize + iUseSize);

					Marshal.StructureToPtr(pData, ip, false);

					IntPtr ip2 = ip + iBitmapInfoHeaderSize;
					Marshal.Copy(pData.bmiColors, 0, ip2, pData.bmiColors.Length);
					break;
				}
				default:
				{
					int iBitmapInfoHeaderSize = Marshal.SizeOf(typeof(BitmapInfoHeader));

					// BitmapInfoHeader - just do a copy
					ip = Marshal.AllocCoTaskMem(iBitmapInfoHeaderSize);
					Marshal.StructureToPtr(this, ip, false);
					break;
				}
			}

			return ip;
		}

		[NotNull]
		public static BitmapInfoHeader PtrToBMI(IntPtr pNativeData)
		{
			int iEntries;

			int biBitCount = Marshal.ReadInt16(pNativeData, 14);
			int biCompression = Marshal.ReadInt32(pNativeData, 16);
			int biClrUsed = Marshal.ReadInt32(pNativeData, 32);

			if (biCompression == 3) // BI_BITFIELDS
				iEntries = 3;
			else if (biClrUsed > 0) iEntries = biClrUsed;
			else if (biBitCount <= 8) iEntries = 1 << biBitCount;
			else iEntries = 0;

			BitmapInfoHeader bmi;

			if (iEntries == 0)
			{
				// Create a simple BitmapInfoHeader struct
				bmi = new BitmapInfoHeader();
				Marshal.PtrToStructure(pNativeData, bmi);
			}
			else
			{
				BitmapInfoHeaderWithData ext = new BitmapInfoHeaderWithData
				{
					Size = Marshal.ReadInt32(pNativeData, 0),
					Width = Marshal.ReadInt32(pNativeData, 4),
					Height = Marshal.ReadInt32(pNativeData, 8),
					Planes = Marshal.ReadInt16(pNativeData, 12),
					BitCount = Marshal.ReadInt16(pNativeData, 14),
					Compression = Marshal.ReadInt32(pNativeData, 16),
					ImageSize = Marshal.ReadInt32(pNativeData, 20),
					XPelsPerMeter = Marshal.ReadInt32(pNativeData, 24),
					YPelsPerMeter = Marshal.ReadInt32(pNativeData, 28),
					ClrUsed = Marshal.ReadInt32(pNativeData, 32),
					ClrImportant = Marshal.ReadInt32(pNativeData, 36)
				};

				bmi = ext;

				ext.bmiColors = new int[iEntries];
				IntPtr ip2 = pNativeData + Marshal.SizeOf(typeof(BitmapInfoHeader));
				Marshal.Copy(ip2, ext.bmiColors, 0, iEntries);
			}

			return bmi;
		}

		public void CopyFrom([NotNull] BitmapInfoHeader bmi)
		{
			Size = bmi.Size;
			Width = bmi.Width;
			Height = bmi.Height;
			Planes = bmi.Planes;
			BitCount = bmi.BitCount;
			Compression = bmi.Compression;
			ImageSize = bmi.ImageSize;
			YPelsPerMeter = bmi.YPelsPerMeter;
			ClrUsed = bmi.ClrUsed;
			ClrImportant = bmi.ClrImportant;

			if (bmi is not BitmapInfoHeaderWithData ext2) return;
			BitmapInfoHeaderWithData ext = (BitmapInfoHeaderWithData)this;

			ext.bmiColors = new int[ext2.bmiColors.Length];
			ext2.bmiColors.CopyTo(ext.bmiColors, 0);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("BITMAPINFO")]
	public class BitmapInfoHeaderWithData : BitmapInfoHeader
	{
		public int[] bmiColors;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class MFInt
	{
		protected int m_value;

		public MFInt()
			: this(0)
		{
		}

		public MFInt(int v) { m_value = v; }

		public int GetValue() { return m_value; }

		// While I *could* enable this code, it almost certainly won't do what you
		// think it will.  Generally you don't want to create a *new* instance of
		// MFInt and assign a value to it.  You want to assign a value to an
		// existing instance.  In order to do this automatically, .Net would have
		// to support overloading operator =.  But since it doesn't, use Assign()

		//public static implicit operator MFInt(int f)
		//{
		//    return new MFInt(f);
		//}

		public static implicit operator int([NotNull] MFInt f) { return f.m_value; }

		public int ToInt32() { return m_value; }

		public void Assign(int f) { m_value = f; }

		[NotNull]
		public override string ToString() { return m_value.ToString(); }

		public override int GetHashCode() { return m_value.GetHashCode(); }

		public override bool Equals(object obj)
		{
			if (obj is MFInt mfInt) return mfInt.m_value == m_value;

			return Convert.ToInt32(obj) == m_value;
		}
	}

	/// <summary>
	///     MFRect is a managed representation of the Win32 RECT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class MFRect
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		/// <summary>
		///     Empty constructor. Initialize all fields to 0
		/// </summary>
		public MFRect() { }

		/// <summary>
		///     A parameterized constructor. Initialize fields with given values.
		/// </summary>
		/// <param name="left">the left value</param>
		/// <param name="top">the top value</param>
		/// <param name="right">the right value</param>
		/// <param name="bottom">the bottom value</param>
		public MFRect(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		/// <summary>
		///     A parameterized constructor. Initialize fields with a given <see cref="System.Drawing.Rectangle" />.
		/// </summary>
		/// <param name="rectangle">A <see cref="System.Drawing.Rectangle" /></param>
		/// <remarks>
		///     Warning, MFRect define a rectangle by defining two of his corners and <see cref="System.Drawing.Rectangle" />
		///     define a rectangle with his upper/left corner, his width and his height.
		/// </remarks>
		public MFRect(Rectangle rectangle)
			: this(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom)
		{
		}

		/// <summary>
		///     Provide de string representation of this MFRect instance
		/// </summary>
		/// <returns>A string formatted like this : [left, top - right, bottom]</returns>
		[NotNull]
		public override string ToString() { return string.Format("[{0}, {1}] - [{2}, {3}]", left, top, right, bottom); }

		public override int GetHashCode()
		{
			return left.GetHashCode() |
					top.GetHashCode() |
					right.GetHashCode() |
					bottom.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is MFRect rect) return right == rect.right && bottom == rect.bottom && left == rect.left && top == rect.top;

			if (obj is Rectangle cmp) return right == cmp.Right && bottom == cmp.Bottom && left == cmp.Left && top == cmp.Top;

			return false;
		}

		/// <summary>
		///     Checks to see if the rectangle is empty
		/// </summary>
		/// <returns>Returns true if the rectangle is empty</returns>
		public bool IsEmpty() { return right <= left || bottom <= top; }

		/// <summary>
		///     Define implicit cast between MFRect and System.Drawing.Rectangle for languages supporting this feature.
		///     VB.Net doesn't support implicit cast. <see cref="MFRect.ToRectangle" /> for similar functionality.
		///     <code>
		///   // Define a new Rectangle instance
		///   Rectangle r = new Rectangle(0, 0, 100, 100);
		///   // Do implicit cast between Rectangle and MFRect
		///   MFRect mfR = r;
		/// 
		///   Console.WriteLine(mfR.ToString());
		/// </code>
		/// </summary>
		/// <param name="r">a MFRect to be cast</param>
		/// <returns>A casted System.Drawing.Rectangle</returns>
		public static implicit operator Rectangle([NotNull] MFRect r) { return r.ToRectangle(); }

		/// <summary>
		///     Define implicit cast between System.Drawing.Rectangle and MFRect for languages supporting this feature.
		///     VB.Net doesn't support implicit cast. <see cref="MFRect.FromRectangle" /> for similar functionality.
		///     <code>
		///   // Define a new MFRect instance
		///   MFRect mfR = new MFRect(0, 0, 100, 100);
		///   // Do implicit cast between MFRect and Rectangle
		///   Rectangle r = mfR;
		/// 
		///   Console.WriteLine(r.ToString());
		/// </code>
		/// </summary>
		/// <param name="r">A System.Drawing.Rectangle to be cast</param>
		/// <returns>A casted MFRect</returns>
		[NotNull]
		public static implicit operator MFRect(Rectangle r) { return new MFRect(r); }

		/// <summary>
		///     Get the System.Drawing.Rectangle equivalent to this MFRect instance.
		/// </summary>
		/// <returns>A System.Drawing.Rectangle</returns>
		public Rectangle ToRectangle() { return new Rectangle(left, top, right - left, bottom - top); }

		/// <summary>
		///     Get a new MFRect instance for a given <see cref="System.Drawing.Rectangle" />
		/// </summary>
		/// <param name="r">The <see cref="System.Drawing.Rectangle" /> used to initialize this new MFGuid</param>
		/// <returns>A new instance of MFGuid</returns>
		[NotNull]
		public static MFRect FromRectangle(Rectangle r) { return new MFRect(r); }

		/// <summary>
		///     Copy the members from an MFRect into this object
		/// </summary>
		/// <param name="from">The rectangle from which to copy the values.</param>
		public void CopyFrom([NotNull] MFRect from)
		{
			left = from.left;
			top = from.top;
			right = from.right;
			bottom = from.bottom;
		}
	}

	/// <summary>
	///     MFGuid is a wrapper class around a System.Guid value type.
	/// </summary>
	/// <remarks>
	///     This class is necessary to enable null parameters passing.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit)]
	public class MFGuid
	{
		[FieldOffset(0)]
		private Guid guid;

		public static readonly MFGuid Empty = Guid.Empty;

		/// <summary>
		///     Empty constructor.
		///     Initialize it with System.Guid.Empty
		/// </summary>
		public MFGuid()
			: this(Empty)
		{
		}

		/// <summary>
		///     Constructor.
		///     Initialize this instance with a given System.Guid string representation.
		/// </summary>
		/// <param name="g">A valid System.Guid as string</param>
		public MFGuid([NotNull] string g)
			: this(new Guid(g))
		{
		}

		/// <summary>
		///     Constructor.
		///     Initialize this instance with a given System.Guid.
		/// </summary>
		/// <param name="g">A System.Guid value type</param>
		public MFGuid(Guid g) { guid = g; }

		/// <summary>
		///     Get a string representation of this MFGuid Instance.
		/// </summary>
		/// <returns>A string representing this instance</returns>
		[NotNull]
		public override string ToString() { return guid.ToString(); }

		/// <summary>
		///     Get a string representation of this MFGuid Instance with a specific format.
		/// </summary>
		/// <param name="format"><see cref="System.Guid.ToString(string)" /> for a description of the format parameter.</param>
		/// <returns>A string representing this instance according to the format parameter</returns>
		[NotNull]
		public string ToString(string format) { return guid.ToString(format); }

		public override int GetHashCode() { return guid.GetHashCode(); }

		/// <summary>
		///     Define implicit cast between MFGuid and System.Guid for languages supporting this feature.
		///     VB.Net doesn't support implicit cast. <see cref="MFGuid.ToGuid" /> for similar functionality.
		///     <code>
		///   // Define a new MFGuid instance
		///   MFGuid mfG = new MFGuid("{33D57EBF-7C9D-435e-A15E-D300B52FBD91}");
		///   // Do implicit cast between MFGuid and Guid
		///   Guid g = mfG;
		/// 
		///   Console.WriteLine(g.ToString());
		/// </code>
		/// </summary>
		/// <param name="g">MFGuid to be cast</param>
		/// <returns>A casted System.Guid</returns>
		public static implicit operator Guid([NotNull] MFGuid g) { return g.guid; }

		/// <summary>
		///     Define implicit cast between System.Guid and MFGuid for languages supporting this feature.
		///     VB.Net doesn't support implicit cast. <see cref="MFGuid.FromGuid" /> for similar functionality.
		///     <code>
		///   // Define a new Guid instance
		///   Guid g = new Guid("{B9364217-366E-45f8-AA2D-B0ED9E7D932D}");
		///   // Do implicit cast between Guid and MFGuid
		///   MFGuid mfG = g;
		/// 
		///   Console.WriteLine(mfG.ToString());
		/// </code>
		/// </summary>
		/// <param name="g">System.Guid to be cast</param>
		/// <returns>A casted MFGuid</returns>
		[NotNull]
		public static implicit operator MFGuid(Guid g) { return new MFGuid(g); }

		/// <summary>
		///     Get the System.Guid equivalent to this MFGuid instance.
		/// </summary>
		/// <returns>A System.Guid</returns>
		public Guid ToGuid() { return guid; }

		/// <summary>
		///     Get a new MFGuid instance for a given System.Guid
		/// </summary>
		/// <param name="g">The System.Guid to wrap into a MFGuid</param>
		/// <returns>A new instance of MFGuid</returns>
		[NotNull]
		public static MFGuid FromGuid(Guid g) { return new MFGuid(g); }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("SIZE")]
	public class MFSize
	{
		public int cx;
		public int cy;

		public MFSize()
		{
			cx = 0;
			cy = 0;
		}

		public MFSize(int iWidth, int iHeight)
		{
			cx = iWidth;
			cy = iHeight;
		}

		public int Width { get => cx; set => cx = value; }
		public int Height { get => cy; set => cy = value; }
	}

	[UnmanagedName("CLSID_CColorConvertDMO")]
	[ComImport]
	[Guid("98230571-0087-4204-b020-3282538e57d3")]
	public class CColorConvertDMO
	{
	}

	[UnmanagedName("CLSID_PlayToSourceClassFactory")]
	[ComImport]
	[Guid("DA17539A-3DC3-42C1-A749-A183B51F085E")]
	public class PlayToSourceClassFactory
	{
	}

	[UnmanagedName("CLSID_MFMediaSharingEngineClassFactory")]
	[ComImport]
	[Guid("F8E307FB-6D45-4AD3-9993-66CD5A529659")]
	public class MFMediaSharingEngineClassFactory
	{
	}

	[UnmanagedName("CLSID_MFMediaEngineClassFactory")]
	[ComImport]
	[Guid("B44392DA-499B-446b-A4CB-005FEAD0E6D5")]
	public class MFMediaEngineClassFactory
	{
	}

	[UnmanagedName("CLSID_HttpSchemePlugin")]
	[ComImport]
	[Guid("44cb442b-9da9-49df-b3fd-023777b16e50")]
	public class HttpSchemePlugin
	{
	}

	[UnmanagedName("CLSID_NetSchemePlugin")]
	[ComImport]
	[Guid("e9f4ebab-d97b-463e-a2b1-c54ee3f9414d")]
	public class NetSchemePlugin
	{
	}

	[UnmanagedName("CLSID_MPEG2ByteStreamPlugin")]
	[ComImport]
	[Guid("40871C59-AB40-471F-8DC3-1F259D862479")]
	public class MPEG2ByteStreamPlugin
	{
	}

	[UnmanagedName("CLSID_MFByteStreamProxyClassFactory")]
	[ComImport]
	[Guid("770e8e77-4916-441c-a9a7-b342d0eebc71")]
	public class MFByteStreamProxyClassFactory
	{
	}

	[UnmanagedName("CLSID_UrlmonSchemePlugin")]
	[ComImport]
	[Guid("9EC4B4F9-3029-45ad-947B-344DE2A249E2")]
	public class UrlmonSchemePlugin
	{
	}

	[UnmanagedName("CLSID_MFCaptureEngineClassFactory")]
	[ComImport]
	[Guid("efce38d3-8914-4674-a7df-ae1b3d654b8a")]
	public class MFCaptureEngineClassFactory
	{
	}

	[UnmanagedName("CLSID_MSH264DecoderMFT")]
	[ComImport]
	[Guid("62CE7E72-4C71-4d20-B15D-452831A87D9D")]
	public class MSH264DecoderMFT
	{
	}

	[UnmanagedName("CLSID_MSH264EncoderMFT")]
	[ComImport]
	[Guid("6ca50344-051a-4ded-9779-a43305165e35")]
	public class MSH264EncoderMFT
	{
	}

	[UnmanagedName("CLSID_MSDDPlusDecMFT")]
	[ComImport]
	[Guid("177C0AFE-900B-48d4-9E4C-57ADD250B3D4")]
	public class MSDDPlusDecMFT
	{
	}

	[UnmanagedName("CLSID_MP3DecMediaObject")]
	[ComImport]
	[Guid("bbeea841-0a63-4f52-a7ab-a9b3a84ed38a")]
	public class MP3DecMediaObject
	{
	}

	[UnmanagedName("CLSID_MSAACDecMFT")]
	[ComImport]
	[Guid("32d186a7-218f-4c75-8876-dd77273a8999")]
	public class MSAACDecMFT
	{
	}

	[UnmanagedName("CLSID_MSH265DecoderMFT")]
	[ComImport]
	[Guid("420A51A3-D605-430C-B4FC-45274FA6C562")]
	public class MSH265DecoderMFT
	{
	}

	[UnmanagedName("CLSID_WMVDecoderMFT")]
	[ComImport]
	[Guid("82d353df-90bd-4382-8bc2-3f6192b76e34")]
	public class WMVDecoderMFT
	{
	}

	[UnmanagedName("CLSID_WMADecMediaObject")]
	[ComImport]
	[Guid("2eeb4adf-4578-4d10-bca7-bb955f56320a")]
	public class WMADecMediaObject
	{
	}

	[UnmanagedName("CLSID_MSMPEGAudDecMFT")]
	[ComImport]
	[Guid("70707B39-B2CA-4015-ABEA-F8447D22D88B")]
	public class MSMPEGAudDecMFT
	{
	}

	[UnmanagedName("CLSID_MSMPEGDecoderMFT")]
	[ComImport]
	[Guid("2D709E52-123F-49b5-9CBC-9AF5CDE28FB9")]
	public class MSMPEGDecoderMFT
	{
	}

	[UnmanagedName("CLSID_AudioResamplerMediaObject")]
	[ComImport]
	[Guid("f447b69e-1884-4a7e-8055-346f74d6edb3")]
	public class AudioResamplerMediaObject
	{
	}

	[UnmanagedName("CLSID_VideoProcessorMFT")]
	[ComImport]
	[Guid("88753B26-5B24-49bd-B2E7-0C445C78C982")]
	public class VideoProcessorMFT
	{
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[UnmanagedName("MFAYUVSample")]
	public class MFAYUVSample
	{
		public byte bCrValue;
		public byte bCbValue;
		public byte bYValue;
		public byte bSampleAlpha8;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[UnmanagedName("MFARGB")]
	public class MFARGB
	{
		public byte rgbBlue;
		public byte rgbGreen;
		public byte rgbRed;
		public byte rgbAlpha;

		public int ToInt32()
		{
			return rgbBlue | (rgbGreen << 8) | (rgbRed << 16) | (rgbAlpha << 24);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MPEG2VIDEOINFO")]
	public class Mpeg2VideoInfo
	{
		public VideoInfoHeader2 hdr;
		public int dwStartTimeCode;
		public int cbSequenceHeader;
		public AM_MPEG2Profile dwProfile;
		public AM_MPEG2Level dwLevel;
		public AMMPEG2_Flags dwFlags;
		public int[] dwSequenceHeader;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFNetCredentialManagerGetParam")]
	public class MFNetCredentialManagerGetParam
	{
		public int hrOp;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fAllowLoggedOnUser;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fClearTextPackage;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszUrl;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszSite;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszRealm;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pszPackage;
		public int nRetries;
	}

	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MFBYTESTREAM_BUFFERING_PARAMS")]
	public class MFByteStreamBufferingParams
	{
		public long cbTotalFileSize;
		public long cbPlayableDataSize;
		public MF_LeakyBucketPair[] prgBuckets;
		public int cBuckets;
		public long qwNetBufferingTime;
		public long qwExtraBufferingTimeDuringSeek;
		public long qwPlayDuration;
		public float dRate;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MF_LEAKY_BUCKET_PAIR")]
	public class MF_LeakyBucketPair
	{
		public int dwBitrate;
		public int msBufferWindow;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFVideoArea")]
	public class MFVideoArea
	{
		public MFOffset OffsetX;
		public MFOffset OffsetY;
		public MFSize Area;

		public MFVideoArea()
		{
			OffsetX = new MFOffset();
			OffsetY = new MFOffset();
		}

		public MFVideoArea(float x, float y, int width, int height)
		{
			OffsetX = new MFOffset(x);
			OffsetY = new MFOffset(y);
			Area = new MFSize(width, height);
		}

		public void MakeArea(float x, float y, int width, int height)
		{
			OffsetX.MakeOffset(x);
			OffsetY.MakeOffset(y);
			Area.Width = width;
			Area.Height = height;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[UnmanagedName("MFOffset")]
	public class MFOffset
	{
		public short fract;
		public short Value;

		public MFOffset()
		{
		}

		public MFOffset(float v)
		{
			Value = (short)v;
			fract = (short)(65536 * (v - Value));
		}

		public void MakeOffset(float v)
		{
			Value = (short)v;
			fract = (short)(65536 * (v - Value));
		}

		public float GetOffset()
		{
			return Value + fract / 65536.0f;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[UnmanagedName("MFVIDEOFORMAT")]
	public class MFVideoFormat
	{
		public int dwSize;
		public MFVideoInfo videoInfo;
		public Guid guidFormat;
		public MFVideoCompressedInfo compressedInfo;
		public MFVideoSurfaceInfo surfaceInfo;
	}

	[UnmanagedName("MPEG1VIDEOINFO"), StructLayout(LayoutKind.Sequential)]
	public class Mpeg1VideoInfo
	{
		public VideoInfoHeader hdr;
		public int dwStartTimeCode;
		public int cbSequenceHeader;
		public byte[] bSequenceHeader;
	}

	/// <summary>
	/// When you are done with an instance of this class,
	/// it should be released with FreeAMMediaType() to avoid leaking
	/// </summary>
	[UnmanagedName("AM_MEDIA_TYPE"), StructLayout(LayoutKind.Sequential)]
	public class AMMediaType
	{
		public Guid majorType;
		public Guid subType;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fixedSizeSamples;
		[MarshalAs(UnmanagedType.Bool)]
		public bool temporalCompression;
		public int sampleSize;
		public Guid formatType;
		public IntPtr unkPtr; // IUnknown Pointer
		public int formatSize;
		public IntPtr formatPtr; // Pointer to a buff determined by formatType
	}

	[UnmanagedName("VIDEOINFOHEADER"), StructLayout(LayoutKind.Sequential)]
	public class VideoInfoHeader
	{
		public MFRect SrcRect;
		public MFRect TargetRect;
		public int BitRate;
		public int BitErrorRate;
		public long AvgTimePerFrame;
		public BitmapInfoHeader BmiHeader;  // Custom marshaler?
	}

	[UnmanagedName("VIDEOINFOHEADER2"), StructLayout(LayoutKind.Sequential)]
	public class VideoInfoHeader2
	{
		public MFRect SrcRect;
		public MFRect TargetRect;
		public int BitRate;
		public int BitErrorRate;
		public long AvgTimePerFrame;
		public AMInterlace InterlaceFlags;
		public AMCopyProtect CopyProtectFlags;
		public int PictAspectRatioX;
		public int PictAspectRatioY;
		public AMControl ControlFlags;
		public int Reserved2;
		public BitmapInfoHeader BmiHeader;  // Custom marshaler?
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4), UnmanagedName("PROPERTYKEY")]
	public class PropertyKey
	{
		public Guid fmtid;
		public int pID;

		public PropertyKey()
		{
		}

		public PropertyKey(Guid f, int p)
		{
			fmtid = f;
			pID = p;
		}
	}
	#endregion

	#region static classes
	public static class ResultMF
	{
		public const int MF_E_PLATFORM_NOT_INITIALIZED = unchecked((int)0xC00D36B0);
		public const int MF_E_BUFFERTOOSMALL = unchecked((int)0xC00D36B1);
		public const int MF_E_INVALIDREQUEST = unchecked((int)0xC00D36B2);
		public const int MF_E_INVALIDSTREAMNUMBER = unchecked((int)0xC00D36B3);
		public const int MF_E_INVALIDMEDIATYPE = unchecked((int)0xC00D36B4);
		public const int MF_E_NOTACCEPTING = unchecked((int)0xC00D36B5);
		public const int MF_E_NOT_INITIALIZED = unchecked((int)0xC00D36B6);
		public const int MF_E_UNSUPPORTED_REPRESENTATION = unchecked((int)0xC00D36B7);
		public const int MF_E_NO_MORE_TYPES = unchecked((int)0xC00D36B9);
		public const int MF_E_UNSUPPORTED_SERVICE = unchecked((int)0xC00D36BA);
		public const int MF_E_UNEXPECTED = unchecked((int)0xC00D36BB);
		public const int MF_E_INVALIDNAME = unchecked((int)0xC00D36BC);
		public const int MF_E_INVALIDTYPE = unchecked((int)0xC00D36BD);
		public const int MF_E_INVALID_FILE_FORMAT = unchecked((int)0xC00D36BE);
		public const int MF_E_INVALIDINDEX = unchecked((int)0xC00D36BF);
		public const int MF_E_INVALID_TIMESTAMP = unchecked((int)0xC00D36C0);
		public const int MF_E_UNSUPPORTED_SCHEME = unchecked((int)0xC00D36C3);
		public const int MF_E_UNSUPPORTED_BYTESTREAM_TYPE = unchecked((int)0xC00D36C4);
		public const int MF_E_UNSUPPORTED_TIME_FORMAT = unchecked((int)0xC00D36C5);
		public const int MF_E_NO_SAMPLE_TIMESTAMP = unchecked((int)0xC00D36C8);
		public const int MF_E_NO_SAMPLE_DURATION = unchecked((int)0xC00D36C9);
		public const int MF_E_INVALID_STREAM_DATA = unchecked((int)0xC00D36CB);
		public const int MF_E_RT_UNAVAILABLE = unchecked((int)0xC00D36CF);
		public const int MF_E_UNSUPPORTED_RATE = unchecked((int)0xC00D36D0);
		public const int MF_E_THINNING_UNSUPPORTED = unchecked((int)0xC00D36D1);
		public const int MF_E_REVERSE_UNSUPPORTED = unchecked((int)0xC00D36D2);
		public const int MF_E_UNSUPPORTED_RATE_TRANSITION = unchecked((int)0xC00D36D3);
		public const int MF_E_RATE_CHANGE_PREEMPTED = unchecked((int)0xC00D36D4);
		public const int MF_E_NOT_FOUND = unchecked((int)0xC00D36D5);
		public const int MF_E_NOT_AVAILABLE = unchecked((int)0xC00D36D6);
		public const int MF_E_NO_CLOCK = unchecked((int)0xC00D36D7);
		public const int MF_S_MULTIPLE_BEGIN = 0x000D36D8;
		public const int MF_E_MULTIPLE_BEGIN = unchecked((int)0xC00D36D9);
		public const int MF_E_MULTIPLE_SUBSCRIBERS = unchecked((int)0xC00D36DA);
		public const int MF_E_TIMER_ORPHANED = unchecked((int)0xC00D36DB);
		public const int MF_E_STATE_TRANSITION_PENDING = unchecked((int)0xC00D36DC);
		public const int MF_E_UNSUPPORTED_STATE_TRANSITION = unchecked((int)0xC00D36DD);
		public const int MF_E_UNRECOVERABLE_ERROR_OCCURRED = unchecked((int)0xC00D36DE);
		public const int MF_E_SAMPLE_HAS_TOO_MANY_BUFFERS = unchecked((int)0xC00D36DF);
		public const int MF_E_SAMPLE_NOT_WRITABLE = unchecked((int)0xC00D36E0);
		public const int MF_E_INVALID_KEY = unchecked((int)0xC00D36E2);
		public const int MF_E_BAD_STARTUP_VERSION = unchecked((int)0xC00D36E3);
		public const int MF_E_UNSUPPORTED_CAPTION = unchecked((int)0xC00D36E4);
		public const int MF_E_INVALID_POSITION = unchecked((int)0xC00D36E5);
		public const int MF_E_ATTRIBUTENOTFOUND = unchecked((int)0xC00D36E6);
		public const int MF_E_PROPERTY_TYPE_NOT_ALLOWED = unchecked((int)0xC00D36E7);
		public const int MF_E_PROPERTY_TYPE_NOT_SUPPORTED = unchecked((int)0xC00D36E8);
		public const int MF_E_PROPERTY_EMPTY = unchecked((int)0xC00D36E9);
		public const int MF_E_PROPERTY_NOT_EMPTY = unchecked((int)0xC00D36EA);
		public const int MF_E_PROPERTY_VECTOR_NOT_ALLOWED = unchecked((int)0xC00D36EB);
		public const int MF_E_PROPERTY_VECTOR_REQUIRED = unchecked((int)0xC00D36EC);
		public const int MF_E_OPERATION_CANCELLED = unchecked((int)0xC00D36ED);
		public const int MF_E_BYTESTREAM_NOT_SEEKABLE = unchecked((int)0xC00D36EE);
		public const int MF_E_DISABLED_IN_SAFEMODE = unchecked((int)0xC00D36EF);
		public const int MF_E_CANNOT_PARSE_BYTESTREAM = unchecked((int)0xC00D36F0);
		public const int MF_E_SOURCERESOLVER_MUTUALLY_EXCLUSIVE_FLAGS = unchecked((int)0xC00D36F1);
		public const int MF_E_MEDIAPROC_WRONGSTATE = unchecked((int)0xC00D36F2);
		public const int MF_E_RT_THROUGHPUT_NOT_AVAILABLE = unchecked((int)0xC00D36F3);
		public const int MF_E_RT_TOO_MANY_CLASSES = unchecked((int)0xC00D36F4);
		public const int MF_E_RT_WOULDBLOCK = unchecked((int)0xC00D36F5);
		public const int MF_E_NO_BITPUMP = unchecked((int)0xC00D36F6);
		public const int MF_E_RT_OUTOFMEMORY = unchecked((int)0xC00D36F7);
		public const int MF_E_RT_WORKQUEUE_CLASS_NOT_SPECIFIED = unchecked((int)0xC00D36F8);
		public const int MF_E_INSUFFICIENT_BUFFER = unchecked((int)0xC00D7170);
		public const int MF_E_CANNOT_CREATE_SINK = unchecked((int)0xC00D36FA);
		public const int MF_E_BYTESTREAM_UNKNOWN_LENGTH = unchecked((int)0xC00D36FB);
		public const int MF_E_SESSION_PAUSEWHILESTOPPED = unchecked((int)0xC00D36FC);
		public const int MF_S_ACTIVATE_REPLACED = 0x000D36FD;
		public const int MF_E_FORMAT_CHANGE_NOT_SUPPORTED = unchecked((int)0xC00D36FE);
		public const int MF_E_INVALID_WORKQUEUE = unchecked((int)0xC00D36FF);
		public const int MF_E_DRM_UNSUPPORTED = unchecked((int)0xC00D3700);
		public const int MF_E_UNAUTHORIZED = unchecked((int)0xC00D3701);
		public const int MF_E_OUT_OF_RANGE = unchecked((int)0xC00D3702);
		public const int MF_E_INVALID_CODEC_MERIT = unchecked((int)0xC00D3703);
		public const int MF_E_HW_MFT_FAILED_START_STREAMING = unchecked((int)0xC00D3704);
		public const int MF_E_OPERATION_IN_PROGRESS = unchecked((int)0xC00D3705);
		public const int MF_S_ASF_PARSEINPROGRESS = 0x400D3A98;
		public const int MF_E_ASF_PARSINGINCOMPLETE = unchecked((int)0xC00D3A98);
		public const int MF_E_ASF_MISSINGDATA = unchecked((int)0xC00D3A99);
		public const int MF_E_ASF_INVALIDDATA = unchecked((int)0xC00D3A9A);
		public const int MF_E_ASF_OPAQUEPACKET = unchecked((int)0xC00D3A9B);
		public const int MF_E_ASF_NOINDEX = unchecked((int)0xC00D3A9C);
		public const int MF_E_ASF_OUTOFRANGE = unchecked((int)0xC00D3A9D);
		public const int MF_E_ASF_INDEXNOTLOADED = unchecked((int)0xC00D3A9E);
		public const int MF_E_ASF_TOO_MANY_PAYLOADS = unchecked((int)0xC00D3A9F);
		public const int MF_E_ASF_UNSUPPORTED_STREAM_TYPE = unchecked((int)0xC00D3AA0);
		public const int MF_E_ASF_DROPPED_PACKET = unchecked((int)0xC00D3AA1);
		public const int MF_E_NO_EVENTS_AVAILABLE = unchecked((int)0xC00D3E80);
		public const int MF_E_INVALID_STATE_TRANSITION = unchecked((int)0xC00D3E82);
		public const int MF_E_END_OF_STREAM = unchecked((int)0xC00D3E84);
		public const int MF_E_SHUTDOWN = unchecked((int)0xC00D3E85);
		public const int MF_E_MP3_NOTFOUND = unchecked((int)0xC00D3E86);
		public const int MF_E_MP3_OUTOFDATA = unchecked((int)0xC00D3E87);
		public const int MF_E_MP3_NOTMP3 = unchecked((int)0xC00D3E88);
		public const int MF_E_MP3_NOTSUPPORTED = unchecked((int)0xC00D3E89);
		public const int MF_E_NO_DURATION = unchecked((int)0xC00D3E8A);
		public const int MF_E_INVALID_FORMAT = unchecked((int)0xC00D3E8C);
		public const int MF_E_PROPERTY_NOT_FOUND = unchecked((int)0xC00D3E8D);
		public const int MF_E_PROPERTY_READ_ONLY = unchecked((int)0xC00D3E8E);
		public const int MF_E_PROPERTY_NOT_ALLOWED = unchecked((int)0xC00D3E8F);
		public const int MF_E_MEDIA_SOURCE_NOT_STARTED = unchecked((int)0xC00D3E91);
		public const int MF_E_UNSUPPORTED_FORMAT = unchecked((int)0xC00D3E98);
		public const int MF_E_MP3_BAD_CRC = unchecked((int)0xC00D3E99);
		public const int MF_E_NOT_PROTECTED = unchecked((int)0xC00D3E9A);
		public const int MF_E_MEDIA_SOURCE_WRONGSTATE = unchecked((int)0xC00D3E9B);
		public const int MF_E_MEDIA_SOURCE_NO_STREAMS_SELECTED = unchecked((int)0xC00D3E9C);
		public const int MF_E_CANNOT_FIND_KEYFRAME_SAMPLE = unchecked((int)0xC00D3E9D);
		public const int MF_E_UNSUPPORTED_CHARACTERISTICS = unchecked((int)0xC00D3E9E);
		public const int MF_E_NO_AUDIO_RECORDING_DEVICE = unchecked((int)0xC00D3E9F);
		public const int MF_E_AUDIO_RECORDING_DEVICE_IN_USE = unchecked((int)0xC00D3EA0);
		public const int MF_E_AUDIO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA1);
		public const int MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA2);
		public const int MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED = unchecked((int)0xC00D3EA3);
		public const int MF_E_NETWORK_RESOURCE_FAILURE = unchecked((int)0xC00D4268);
		public const int MF_E_NET_WRITE = unchecked((int)0xC00D4269);
		public const int MF_E_NET_READ = unchecked((int)0xC00D426A);
		public const int MF_E_NET_REQUIRE_NETWORK = unchecked((int)0xC00D426B);
		public const int MF_E_NET_REQUIRE_ASYNC = unchecked((int)0xC00D426C);
		public const int MF_E_NET_BWLEVEL_NOT_SUPPORTED = unchecked((int)0xC00D426D);
		public const int MF_E_NET_STREAMGROUPS_NOT_SUPPORTED = unchecked((int)0xC00D426E);
		public const int MF_E_NET_MANUALSS_NOT_SUPPORTED = unchecked((int)0xC00D426F);
		public const int MF_E_NET_INVALID_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D4270);
		public const int MF_E_NET_CACHESTREAM_NOT_FOUND = unchecked((int)0xC00D4271);
		public const int MF_I_MANUAL_PROXY = 0x400D4272;
		public const int MF_E_NET_REQUIRE_INPUT = unchecked((int)0xC00D4274);
		public const int MF_E_NET_REDIRECT = unchecked((int)0xC00D4275);
		public const int MF_E_NET_REDIRECT_TO_PROXY = unchecked((int)0xC00D4276);
		public const int MF_E_NET_TOO_MANY_REDIRECTS = unchecked((int)0xC00D4277);
		public const int MF_E_NET_TIMEOUT = unchecked((int)0xC00D4278);
		public const int MF_E_NET_CLIENT_CLOSE = unchecked((int)0xC00D4279);
		public const int MF_E_NET_BAD_CONTROL_DATA = unchecked((int)0xC00D427A);
		public const int MF_E_NET_INCOMPATIBLE_SERVER = unchecked((int)0xC00D427B);
		public const int MF_E_NET_UNSAFE_URL = unchecked((int)0xC00D427C);
		public const int MF_E_NET_CACHE_NO_DATA = unchecked((int)0xC00D427D);
		public const int MF_E_NET_EOL = unchecked((int)0xC00D427E);
		public const int MF_E_NET_BAD_REQUEST = unchecked((int)0xC00D427F);
		public const int MF_E_NET_INTERNAL_SERVER_ERROR = unchecked((int)0xC00D4280);
		public const int MF_E_NET_SESSION_NOT_FOUND = unchecked((int)0xC00D4281);
		public const int MF_E_NET_NOCONNECTION = unchecked((int)0xC00D4282);
		public const int MF_E_NET_CONNECTION_FAILURE = unchecked((int)0xC00D4283);
		public const int MF_E_NET_INCOMPATIBLE_PUSHSERVER = unchecked((int)0xC00D4284);
		public const int MF_E_NET_SERVER_ACCESSDENIED = unchecked((int)0xC00D4285);
		public const int MF_E_NET_PROXY_ACCESSDENIED = unchecked((int)0xC00D4286);
		public const int MF_E_NET_CANNOTCONNECT = unchecked((int)0xC00D4287);
		public const int MF_E_NET_INVALID_PUSH_TEMPLATE = unchecked((int)0xC00D4288);
		public const int MF_E_NET_INVALID_PUSH_PUBLISHING_POINT = unchecked((int)0xC00D4289);
		public const int MF_E_NET_BUSY = unchecked((int)0xC00D428A);
		public const int MF_E_NET_RESOURCE_GONE = unchecked((int)0xC00D428B);
		public const int MF_E_NET_ERROR_FROM_PROXY = unchecked((int)0xC00D428C);
		public const int MF_E_NET_PROXY_TIMEOUT = unchecked((int)0xC00D428D);
		public const int MF_E_NET_SERVER_UNAVAILABLE = unchecked((int)0xC00D428E);
		public const int MF_E_NET_TOO_MUCH_DATA = unchecked((int)0xC00D428F);
		public const int MF_E_NET_SESSION_INVALID = unchecked((int)0xC00D4290);
		public const int MF_E_OFFLINE_MODE = unchecked((int)0xC00D4291);
		public const int MF_E_NET_UDP_BLOCKED = unchecked((int)0xC00D4292);
		public const int MF_E_NET_UNSUPPORTED_CONFIGURATION = unchecked((int)0xC00D4293);
		public const int MF_E_NET_PROTOCOL_DISABLED = unchecked((int)0xC00D4294);
		public const int MF_E_ALREADY_INITIALIZED = unchecked((int)0xC00D4650);
		public const int MF_E_BANDWIDTH_OVERRUN = unchecked((int)0xC00D4651);
		public const int MF_E_LATE_SAMPLE = unchecked((int)0xC00D4652);
		public const int MF_E_FLUSH_NEEDED = unchecked((int)0xC00D4653);
		public const int MF_E_INVALID_PROFILE = unchecked((int)0xC00D4654);
		public const int MF_E_INDEX_NOT_COMMITTED = unchecked((int)0xC00D4655);
		public const int MF_E_NO_INDEX = unchecked((int)0xC00D4656);
		public const int MF_E_CANNOT_INDEX_IN_PLACE = unchecked((int)0xC00D4657);
		public const int MF_E_MISSING_ASF_LEAKYBUCKET = unchecked((int)0xC00D4658);
		public const int MF_E_INVALID_ASF_STREAMID = unchecked((int)0xC00D4659);
		public const int MF_E_STREAMSINK_REMOVED = unchecked((int)0xC00D4A38);
		public const int MF_E_STREAMSINKS_OUT_OF_SYNC = unchecked((int)0xC00D4A3A);
		public const int MF_E_STREAMSINKS_FIXED = unchecked((int)0xC00D4A3B);
		public const int MF_E_STREAMSINK_EXISTS = unchecked((int)0xC00D4A3C);
		public const int MF_E_SAMPLEALLOCATOR_CANCELED = unchecked((int)0xC00D4A3D);
		public const int MF_E_SAMPLEALLOCATOR_EMPTY = unchecked((int)0xC00D4A3E);
		public const int MF_E_SINK_ALREADYSTOPPED = unchecked((int)0xC00D4A3F);
		public const int MF_E_ASF_FILESINK_BITRATE_UNKNOWN = unchecked((int)0xC00D4A40);
		public const int MF_E_SINK_NO_STREAMS = unchecked((int)0xC00D4A41);
		public const int MF_S_SINK_NOT_FINALIZED = 0x000D4A42;
		public const int MF_E_METADATA_TOO_LONG = unchecked((int)0xC00D4A43);
		public const int MF_E_SINK_NO_SAMPLES_PROCESSED = unchecked((int)0xC00D4A44);
		public const int MF_E_SINK_HEADERS_NOT_FOUND = unchecked((int)0xC00D4A45);
		public const int MF_E_VIDEO_REN_NO_PROCAMP_HW = unchecked((int)0xC00D4E20);
		public const int MF_E_VIDEO_REN_NO_DEINTERLACE_HW = unchecked((int)0xC00D4E21);
		public const int MF_E_VIDEO_REN_COPYPROT_FAILED = unchecked((int)0xC00D4E22);
		public const int MF_E_VIDEO_REN_SURFACE_NOT_SHARED = unchecked((int)0xC00D4E23);
		public const int MF_E_VIDEO_DEVICE_LOCKED = unchecked((int)0xC00D4E24);
		public const int MF_E_NEW_VIDEO_DEVICE = unchecked((int)0xC00D4E25);
		public const int MF_E_NO_VIDEO_SAMPLE_AVAILABLE = unchecked((int)0xC00D4E26);
		public const int MF_E_NO_AUDIO_PLAYBACK_DEVICE = unchecked((int)0xC00D4E84);
		public const int MF_E_AUDIO_PLAYBACK_DEVICE_IN_USE = unchecked((int)0xC00D4E85);
		public const int MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED = unchecked((int)0xC00D4E86);
		public const int MF_E_AUDIO_SERVICE_NOT_RUNNING = unchecked((int)0xC00D4E87);
		public const int MF_E_TOPO_INVALID__NODE = unchecked((int)0xC00D520E);
		public const int MF_E_TOPO_CANNOT_FIND_DECRYPTOR = unchecked((int)0xC00D5211);
		public const int MF_E_TOPO_CODEC_NOT_FOUND = unchecked((int)0xC00D5212);
		public const int MF_E_TOPO_CANNOT_CONNECT = unchecked((int)0xC00D5213);
		public const int MF_E_TOPO_UNSUPPORTED = unchecked((int)0xC00D5214);
		public const int MF_E_TOPO_INVALID_TIME_ATTRIBUTES = unchecked((int)0xC00D5215);
		public const int MF_E_TOPO_LOOPS_IN_TOPOLOGY = unchecked((int)0xC00D5216);
		public const int MF_E_TOPO_MISSING_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D5217);
		public const int MF_E_TOPO_MISSING_STREAM_DESCRIPTOR = unchecked((int)0xC00D5218);
		public const int MF_E_TOPO_STREAM_DESCRIPTOR_NOT_SELECTED = unchecked((int)0xC00D5219);
		public const int MF_E_TOPO_MISSING_SOURCE = unchecked((int)0xC00D521A);
		public const int MF_E_TOPO_SINK_ACTIVATES_UNSUPPORTED = unchecked((int)0xC00D521B);
		public const int MF_E_SEQUENCER_UNKNOWN_SEGMENT_ID = unchecked((int)0xC00D61AC);
		public const int MF_S_SEQUENCER_CONTEXT_CANCELED = 0x000D61AD;
		public const int MF_E_NO_SOURCE_IN_CACHE = unchecked((int)0xC00D61AE);
		public const int MF_S_SEQUENCER_SEGMENT_AT_END_OF_STREAM = 0x000D61AF;
		public const int MF_E_TRANSFORM_TYPE_NOT_SET = unchecked((int)0xC00D6D60);
		public const int MF_E_TRANSFORM_STREAM_CHANGE = unchecked((int)0xC00D6D61);
		public const int MF_E_TRANSFORM_INPUT_REMAINING = unchecked((int)0xC00D6D62);
		public const int MF_E_TRANSFORM_PROFILE_MISSING = unchecked((int)0xC00D6D63);
		public const int MF_E_TRANSFORM_PROFILE_INVALID_OR_CORRUPT = unchecked((int)0xC00D6D64);
		public const int MF_E_TRANSFORM_PROFILE_TRUNCATED = unchecked((int)0xC00D6D65);
		public const int MF_E_TRANSFORM_PROPERTY_PID_NOT_RECOGNIZED = unchecked((int)0xC00D6D66);
		public const int MF_E_TRANSFORM_PROPERTY_VARIANT_TYPE_WRONG = unchecked((int)0xC00D6D67);
		public const int MF_E_TRANSFORM_PROPERTY_NOT_WRITEABLE = unchecked((int)0xC00D6D68);
		public const int MF_E_TRANSFORM_PROPERTY_ARRAY_VALUE_WRONG_NUM_DIM = unchecked((int)0xC00D6D69);
		public const int MF_E_TRANSFORM_PROPERTY_VALUE_SIZE_WRONG = unchecked((int)0xC00D6D6A);
		public const int MF_E_TRANSFORM_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC00D6D6B);
		public const int MF_E_TRANSFORM_PROPERTY_VALUE_INCOMPATIBLE = unchecked((int)0xC00D6D6C);
		public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_OUTPUT_MEDIATYPE = unchecked((int)0xC00D6D6D);
		public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_INPUT_MEDIATYPE = unchecked((int)0xC00D6D6E);
		public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_MEDIATYPE_COMBINATION = unchecked((int)0xC00D6D6F);
		public const int MF_E_TRANSFORM_CONFLICTS_WITH_OTHER_CURRENTLY_ENABLED_FEATURES = unchecked((int)0xC00D6D70);
		public const int MF_E_TRANSFORM_NEED_MORE_INPUT = unchecked((int)0xC00D6D72);
		public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_SPKR_CONFIG = unchecked((int)0xC00D6D73);
		public const int MF_E_TRANSFORM_CANNOT_CHANGE_MEDIATYPE_WHILE_PROCESSING = unchecked((int)0xC00D6D74);
		public const int MF_S_TRANSFORM_DO_NOT_PROPAGATE_EVENT = 0x000D6D75;
		public const int MF_E_UNSUPPORTED_D3D_TYPE = unchecked((int)0xC00D6D76);
		public const int MF_E_TRANSFORM_ASYNC_LOCKED = unchecked((int)0xC00D6D77);
		public const int MF_E_TRANSFORM_CANNOT_INITIALIZE_ACM_DRIVER = unchecked((int)0xC00D6D78);
		public const int MF_E_LICENSE_INCORRECT_RIGHTS = unchecked((int)0xC00D7148);
		public const int MF_E_LICENSE_OUTOFDATE = unchecked((int)0xC00D7149);
		public const int MF_E_LICENSE_REQUIRED = unchecked((int)0xC00D714A);
		public const int MF_E_DRM_HARDWARE_INCONSISTENT = unchecked((int)0xC00D714B);
		public const int MF_E_NO_CONTENT_PROTECTION_MANAGER = unchecked((int)0xC00D714C);
		public const int MF_E_LICENSE_RESTORE_NO_RIGHTS = unchecked((int)0xC00D714D);
		public const int MF_E_BACKUP_RESTRICTED_LICENSE = unchecked((int)0xC00D714E);
		public const int MF_E_LICENSE_RESTORE_NEEDS_INDIVIDUALIZATION = unchecked((int)0xC00D714F);
		public const int MF_S_PROTECTION_NOT_REQUIRED = 0x000D7150;
		public const int MF_E_COMPONENT_REVOKED = unchecked((int)0xC00D7151);
		public const int MF_E_TRUST_DISABLED = unchecked((int)0xC00D7152);
		public const int MF_E_WMDRMOTA_NO_ACTION = unchecked((int)0xC00D7153);
		public const int MF_E_WMDRMOTA_ACTION_ALREADY_SET = unchecked((int)0xC00D7154);
		public const int MF_E_WMDRMOTA_DRM_HEADER_NOT_AVAILABLE = unchecked((int)0xC00D7155);
		public const int MF_E_WMDRMOTA_DRM_ENCRYPTION_SCHEME_NOT_SUPPORTED = unchecked((int)0xC00D7156);
		public const int MF_E_WMDRMOTA_ACTION_MISMATCH = unchecked((int)0xC00D7157);
		public const int MF_E_WMDRMOTA_INVALID_POLICY = unchecked((int)0xC00D7158);
		public const int MF_E_POLICY_UNSUPPORTED = unchecked((int)0xC00D7159);
		public const int MF_E_OPL_NOT_SUPPORTED = unchecked((int)0xC00D715A);
		public const int MF_E_TOPOLOGY_VERIFICATION_FAILED = unchecked((int)0xC00D715B);
		public const int MF_E_SIGNATURE_VERIFICATION_FAILED = unchecked((int)0xC00D715C);
		public const int MF_E_DEBUGGING_NOT_ALLOWED = unchecked((int)0xC00D715D);
		public const int MF_E_CODE_EXPIRED = unchecked((int)0xC00D715E);
		public const int MF_E_GRL_VERSION_TOO_LOW = unchecked((int)0xC00D715F);
		public const int MF_E_GRL_RENEWAL_NOT_FOUND = unchecked((int)0xC00D7160);
		public const int MF_E_GRL_EXTENSIBLE_ENTRY_NOT_FOUND = unchecked((int)0xC00D7161);
		public const int MF_E_KERNEL_UNTRUSTED = unchecked((int)0xC00D7162);
		public const int MF_E_PEAUTH_UNTRUSTED = unchecked((int)0xC00D7163);
		public const int MF_E_NON_PE_PROCESS = unchecked((int)0xC00D7165);
		public const int MF_E_REBOOT_REQUIRED = unchecked((int)0xC00D7167);
		public const int MF_S_WAIT_FOR_POLICY_SET = 0x000D7168;
		public const int MF_S_VIDEO_DISABLED_WITH_UNKNOWN_SOFTWARE_OUTPUT = 0x000D7169;
		public const int MF_E_GRL_INVALID_FORMAT = unchecked((int)0xC00D716A);
		public const int MF_E_GRL_UNRECOGNIZED_FORMAT = unchecked((int)0xC00D716B);
		public const int MF_E_ALL_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716C);
		public const int MF_E_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716D);
		public const int MF_E_USERMODE_UNTRUSTED = unchecked((int)0xC00D716E);
		public const int MF_E_PEAUTH_SESSION_NOT_STARTED = unchecked((int)0xC00D716F);
		public const int MF_E_PEAUTH_PUBLICKEY_REVOKED = unchecked((int)0xC00D7171);
		public const int MF_E_GRL_ABSENT = unchecked((int)0xC00D7172);
		public const int MF_S_PE_TRUSTED = 0x000D7173;
		public const int MF_E_PE_UNTRUSTED = unchecked((int)0xC00D7174);
		public const int MF_E_PEAUTH_NOT_STARTED = unchecked((int)0xC00D7175);
		public const int MF_E_INCOMPATIBLE_SAMPLE_PROTECTION = unchecked((int)0xC00D7176);
		public const int MF_E_PE_SESSIONS_MAXED = unchecked((int)0xC00D7177);
		public const int MF_E_HIGH_SECURITY_LEVEL_CONTENT_NOT_ALLOWED = unchecked((int)0xC00D7178);
		public const int MF_E_TEST_SIGNED_COMPONENTS_NOT_ALLOWED = unchecked((int)0xC00D7179);
		public const int MF_E_ITA_UNSUPPORTED_ACTION = unchecked((int)0xC00D717A);
		public const int MF_E_ITA_ERROR_PARSING_SAP_PARAMETERS = unchecked((int)0xC00D717B);
		public const int MF_E_POLICY_MGR_ACTION_OUTOFBOUNDS = unchecked((int)0xC00D717C);
		public const int MF_E_BAD_OPL_STRUCTURE_FORMAT = unchecked((int)0xC00D717D);
		public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_PROTECTION_GUID = unchecked((int)0xC00D717E);
		public const int MF_E_NO_PMP_HOST = unchecked((int)0xC00D717F);
		public const int MF_E_ITA_OPL_DATA_NOT_INITIALIZED = unchecked((int)0xC00D7180);
		public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_OUTPUT = unchecked((int)0xC00D7181);
		public const int MF_E_ITA_UNRECOGNIZED_DIGITAL_VIDEO_OUTPUT = unchecked((int)0xC00D7182);
		public const int MF_E_RESOLUTION_REQUIRES_PMP_CREATION_CALLBACK = unchecked((int)0xC00D7183);
		public const int MF_E_INVALID_AKE_CHANNEL_PARAMETERS = unchecked((int)0xC00D7184);
		public const int MF_E_CONTENT_PROTECTION_SYSTEM_NOT_ENABLED = unchecked((int)0xC00D7185);
		public const int MF_E_UNSUPPORTED_CONTENT_PROTECTION_SYSTEM = unchecked((int)0xC00D7186);
		public const int MF_E_DRM_MIGRATION_NOT_SUPPORTED = unchecked((int)0xC00D7187);
		public const int MF_E_CLOCK_INVALID_CONTINUITY_KEY = unchecked((int)0xC00D9C40);
		public const int MF_E_CLOCK_NO_TIME_SOURCE = unchecked((int)0xC00D9C41);
		public const int MF_E_CLOCK_STATE_ALREADY_SET = unchecked((int)0xC00D9C42);
		public const int MF_E_CLOCK_NOT_SIMPLE = unchecked((int)0xC00D9C43);
		public const int MF_S_CLOCK_STOPPED = 0x000D9C44;
		public const int MF_E_NO_MORE_DROP_MODES = unchecked((int)0xC00DA028);
		public const int MF_E_NO_MORE_QUALITY_LEVELS = unchecked((int)0xC00DA029);
		public const int MF_E_DROPTIME_NOT_SUPPORTED = unchecked((int)0xC00DA02A);
		public const int MF_E_QUALITYKNOB_WAIT_LONGER = unchecked((int)0xC00DA02B);
		public const int MF_E_QM_INVALIDSTATE = unchecked((int)0xC00DA02C);
		public const int MF_E_TRANSCODE_NO_CONTAINERTYPE = unchecked((int)0xC00DA410);
		public const int MF_E_TRANSCODE_PROFILE_NO_MATCHING_STREAMS = unchecked((int)0xC00DA411);
		public const int MF_E_TRANSCODE_NO_MATCHING_ENCODER = unchecked((int)0xC00DA412);
		public const int MF_E_TRANSCODE_INVALID_PROFILE = unchecked((int)0xC00DA413);
		public const int MF_E_ALLOCATOR_NOT_INITIALIZED = unchecked((int)0xC00DA7F8);
		public const int MF_E_ALLOCATOR_NOT_COMMITED = unchecked((int)0xC00DA7F9);
		public const int MF_E_ALLOCATOR_ALREADY_COMMITED = unchecked((int)0xC00DA7FA);
		public const int MF_E_STREAM_ERROR = unchecked((int)0xC00DA7FB);
		public const int MF_E_INVALID_STREAM_STATE = unchecked((int)0xC00DA7FC);
		public const int MF_E_HW_STREAM_NOT_CONNECTED = unchecked((int)0xC00DA7FD);
		public const int MF_E_NO_CAPTURE_DEVICES_AVAILABLE = unchecked((int)0xC00DABE0);
		public const int MF_E_CAPTURE_SINK_OUTPUT_NOT_SET = unchecked((int)0xC00DABE1);
		public const int MF_E_CAPTURE_SINK_MIRROR_ERROR = unchecked((int)0xC00DABE2);
		public const int MF_E_CAPTURE_SINK_ROTATE_ERROR = unchecked((int)0xC00DABE3);
		public const int MF_E_CAPTURE_ENGINE_INVALID_OP = unchecked((int)0xC00DABE4);
		public const int MF_E_CAPTURE_ENGINE_ALL_EFFECTS_REMOVED = unchecked((int)0xC00DABE5);
		public const int MF_E_CAPTURE_SOURCE_NO_INDEPENDENT_PHOTO_STREAM_PRESENT = unchecked((int)0xC00DABE6);
		public const int MF_E_CAPTURE_SOURCE_NO_VIDEO_STREAM_PRESENT = unchecked((int)0xC00DABE7);
		public const int MF_E_CAPTURE_SOURCE_NO_AUDIO_STREAM_PRESENT = unchecked((int)0xC00DABE8);
		public const int MF_E_CAPTURE_SOURCE_DEVICE_EXTENDEDPROP_OP_IN_PROGRESS = unchecked((int)0xC00DABE9);
		public const int MF_E_CAPTURE_PROPERTY_SET_DURING_PHOTO = unchecked((int)0xC00DABEA);
		public const int MF_E_CAPTURE_NO_SAMPLES_IN_QUEUE = unchecked((int)0xC00DABEB);
		public const int MF_E_HW_ACCELERATED_THUMBNAIL_NOT_SUPPORTED = unchecked((int)0xC00DABEC);
	}

	public static class MF_MEDIA_ENGINE
	{
		// MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE9
		public static readonly Guid BROWSER_COMPATIBILITY_MODE_IE9 = new Guid(0x052c2d39, 0x40c0, 0x4188, 0xab, 0x86, 0xf8, 0x28, 0x27, 0x3b, 0x75, 0x22);

		// MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE10
		public static readonly Guid BROWSER_COMPATIBILITY_MODE_IE10 = new Guid(0x11a47afd, 0x6589, 0x4124, 0xb3, 0x12, 0x61, 0x58, 0xec, 0x51, 0x7f, 0xc3);

		// MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE11
		public static readonly Guid BROWSER_COMPATIBILITY_MODE_IE11 = new Guid(0x1cf1315f, 0xce3f, 0x4035, 0x93, 0x91, 0x16, 0x14, 0x2f, 0x77, 0x51, 0x89);
	}

	public static class MFAttributesClsid
	{
		// Audio Renderer Attributes
		public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ID = new Guid(0xb10aaec3, 0xef71, 0x4cc3, 0xb8, 0x73, 0x5, 0xa9, 0xa0, 0x8b, 0x9f, 0x8e);
		public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ROLE = new Guid(0x6ba644ff, 0x27c5, 0x4d02, 0x98, 0x87, 0xc2, 0x86, 0x19, 0xfd, 0xb9, 0x1b);
		public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_FLAGS = new Guid(0xede4b5e0, 0xf805, 0x4d6c, 0x99, 0xb3, 0xdb, 0x01, 0xbf, 0x95, 0xdf, 0xab);
		public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_SESSION_ID = new Guid(0xede4b5e3, 0xf805, 0x4d6c, 0x99, 0xb3, 0xdb, 0x01, 0xbf, 0x95, 0xdf, 0xab);

		// Byte Stream Attributes
		public static readonly Guid MF_BYTESTREAM_ORIGIN_NAME = new Guid(0xfc358288, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_CONTENT_TYPE = new Guid(0xfc358289, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_DURATION = new Guid(0xfc35828a, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_LAST_MODIFIED_TIME = new Guid(0xfc35828b, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_IFO_FILE_URI = new Guid(0xfc35828c, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_DLNA_PROFILE_ID = new Guid(0xfc35828d, 0x3cb6, 0x460c, 0xa4, 0x24, 0xb6, 0x68, 0x12, 0x60, 0x37, 0x5a);
		public static readonly Guid MF_BYTESTREAM_EFFECTIVE_URL = new Guid(0x9afa0209, 0x89d1, 0x42af, 0x84, 0x56, 0x1d, 0xe6, 0xb5, 0x62, 0xd6, 0x91);
		public static readonly Guid MF_BYTESTREAM_TRANSCODED = new Guid(0xb6c5c282, 0x4dc9, 0x4db9, 0xab, 0x48, 0xcf, 0x3b, 0x6d, 0x8b, 0xc5, 0xe0);

		// Enhanced Video Renderer Attributes
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_ACTIVATE = new Guid(0xba491361, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_CLSID = new Guid(0xba491360, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_MIXER_FLAGS = new Guid(0xba491362, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_ACTIVATE = new Guid(0xba491365, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_CLSID = new Guid(0xba491364, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_CUSTOM_VIDEO_PRESENTER_FLAGS = new Guid(0xba491366, 0xbe50, 0x451e, 0x95, 0xab, 0x6d, 0x4a, 0xcc, 0xc7, 0xda, 0xd8);
		public static readonly Guid MF_ACTIVATE_VIDEO_WINDOW = new Guid(0x9a2dbbdd, 0xf57e, 0x4162, 0x82, 0xb9, 0x68, 0x31, 0x37, 0x76, 0x82, 0xd3);
		public static readonly Guid MF_SA_REQUIRED_SAMPLE_COUNT = new Guid(0x18802c61, 0x324b, 0x4952, 0xab, 0xd0, 0x17, 0x6f, 0xf5, 0xc6, 0x96, 0xff);
		public static readonly Guid MF_SA_REQUIRED_SAMPLE_COUNT_PROGRESSIVE = new Guid(0xb172d58e, 0xfa77, 0x4e48, 0x8d, 0x2a, 0x1d, 0xf2, 0xd8, 0x50, 0xea, 0xc2);
		public static readonly Guid MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT = new Guid(0x851745d5, 0xc3d6, 0x476d, 0x95, 0x27, 0x49, 0x8e, 0xf2, 0xd1, 0xd, 0x18);
		public static readonly Guid MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT_PROGRESSIVE = new Guid(0xf5523a5, 0x1cb2, 0x47c5, 0xa5, 0x50, 0x2e, 0xeb, 0x84, 0xb4, 0xd1, 0x4a);
		public static readonly Guid VIDEO_ZOOM_RECT = new Guid(0x7aaa1638, 0x1b7f, 0x4c93, 0xbd, 0x89, 0x5b, 0x9c, 0x9f, 0xb6, 0xfc, 0xf0);

		// Event Attributes

		// removed by MS - public static readonly Guid MF_EVENT_FORMAT_CHANGE_REQUEST_SOURCE_SAR = new Guid(0xb26fbdfd, 0xc32c, 0x41fe, 0x9c, 0xf0, 0x8, 0x3c, 0xd5, 0xc7, 0xf8, 0xa4);

		// MF_EVENT_DO_THINNING {321EA6FB-DAD9-46e4-B31D-D2EAE7090E30}
		public static readonly Guid MF_EVENT_DO_THINNING = new Guid(0x321ea6fb, 0xdad9, 0x46e4, 0xb3, 0x1d, 0xd2, 0xea, 0xe7, 0x9, 0xe, 0x30);

		// MF_EVENT_OUTPUT_NODE {830f1a8b-c060-46dd-a801-1c95dec9b107}
		public static readonly Guid MF_EVENT_OUTPUT_NODE = new Guid(0x830f1a8b, 0xc060, 0x46dd, 0xa8, 0x01, 0x1c, 0x95, 0xde, 0xc9, 0xb1, 0x07);

		// MF_EVENT_MFT_INPUT_STREAM_ID {F29C2CCA-7AE6-42d2-B284-BF837CC874E2}
		public static readonly Guid MF_EVENT_MFT_INPUT_STREAM_ID = new Guid(0xf29c2cca, 0x7ae6, 0x42d2, 0xb2, 0x84, 0xbf, 0x83, 0x7c, 0xc8, 0x74, 0xe2);

		// MF_EVENT_MFT_CONTEXT {B7CD31F1-899E-4b41-80C9-26A896D32977}
		public static readonly Guid MF_EVENT_MFT_CONTEXT = new Guid(0xb7cd31f1, 0x899e, 0x4b41, 0x80, 0xc9, 0x26, 0xa8, 0x96, 0xd3, 0x29, 0x77);

		// MF_EVENT_PRESENTATION_TIME_OFFSET {5AD914D1-9B45-4a8d-A2C0-81D1E50BFB07}
		public static readonly Guid MF_EVENT_PRESENTATION_TIME_OFFSET = new Guid(0x5ad914d1, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

		// MF_EVENT_SCRUBSAMPLE_TIME {9AC712B3-DCB8-44d5-8D0C-37455A2782E3}
		public static readonly Guid MF_EVENT_SCRUBSAMPLE_TIME = new Guid(0x9ac712b3, 0xdcb8, 0x44d5, 0x8d, 0xc, 0x37, 0x45, 0x5a, 0x27, 0x82, 0xe3);

		// MF_EVENT_SESSIONCAPS {7E5EBCD0-11B8-4abe-AFAD-10F6599A7F42}
		public static readonly Guid MF_EVENT_SESSIONCAPS = new Guid(0x7e5ebcd0, 0x11b8, 0x4abe, 0xaf, 0xad, 0x10, 0xf6, 0x59, 0x9a, 0x7f, 0x42);

		// MF_EVENT_SESSIONCAPS_DELTA {7E5EBCD1-11B8-4abe-AFAD-10F6599A7F42}
		// Type: UINT32
		public static readonly Guid MF_EVENT_SESSIONCAPS_DELTA = new Guid(0x7e5ebcd1, 0x11b8, 0x4abe, 0xaf, 0xad, 0x10, 0xf6, 0x59, 0x9a, 0x7f, 0x42);

		// MF_EVENT_SOURCE_ACTUAL_START {a8cc55a9-6b31-419f-845d-ffb351a2434b}
		public static readonly Guid MF_EVENT_SOURCE_ACTUAL_START = new Guid(0xa8cc55a9, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

		// MF_EVENT_SOURCE_CHARACTERISTICS {47DB8490-8B22-4f52-AFDA-9CE1B2D3CFA8}
		public static readonly Guid MF_EVENT_SOURCE_CHARACTERISTICS = new Guid(0x47db8490, 0x8b22, 0x4f52, 0xaf, 0xda, 0x9c, 0xe1, 0xb2, 0xd3, 0xcf, 0xa8);

		// MF_EVENT_SOURCE_CHARACTERISTICS_OLD {47DB8491-8B22-4f52-AFDA-9CE1B2D3CFA8}
		public static readonly Guid MF_EVENT_SOURCE_CHARACTERISTICS_OLD = new Guid(0x47db8491, 0x8b22, 0x4f52, 0xaf, 0xda, 0x9c, 0xe1, 0xb2, 0xd3, 0xcf, 0xa8);

		// MF_EVENT_SOURCE_FAKE_START {a8cc55a7-6b31-419f-845d-ffb351a2434b}
		public static readonly Guid MF_EVENT_SOURCE_FAKE_START = new Guid(0xa8cc55a7, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

		// MF_EVENT_SOURCE_PROJECTSTART {a8cc55a8-6b31-419f-845d-ffb351a2434b}
		public static readonly Guid MF_EVENT_SOURCE_PROJECTSTART = new Guid(0xa8cc55a8, 0x6b31, 0x419f, 0x84, 0x5d, 0xff, 0xb3, 0x51, 0xa2, 0x43, 0x4b);

		// MF_EVENT_SOURCE_TOPOLOGY_CANCELED {DB62F650-9A5E-4704-ACF3-563BC6A73364}
		public static readonly Guid MF_EVENT_SOURCE_TOPOLOGY_CANCELED = new Guid(0xdb62f650, 0x9a5e, 0x4704, 0xac, 0xf3, 0x56, 0x3b, 0xc6, 0xa7, 0x33, 0x64);

		// MF_EVENT_START_PRESENTATION_TIME {5AD914D0-9B45-4a8d-A2C0-81D1E50BFB07}
		public static readonly Guid MF_EVENT_START_PRESENTATION_TIME = new Guid(0x5ad914d0, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

		// MF_EVENT_START_PRESENTATION_TIME_AT_OUTPUT {5AD914D2-9B45-4a8d-A2C0-81D1E50BFB07}
		public static readonly Guid MF_EVENT_START_PRESENTATION_TIME_AT_OUTPUT = new Guid(0x5ad914d2, 0x9b45, 0x4a8d, 0xa2, 0xc0, 0x81, 0xd1, 0xe5, 0xb, 0xfb, 0x7);

		// MF_EVENT_TOPOLOGY_STATUS {30C5018D-9A53-454b-AD9E-6D5F8FA7C43B}
		public static readonly Guid MF_EVENT_TOPOLOGY_STATUS = new Guid(0x30c5018d, 0x9a53, 0x454b, 0xad, 0x9e, 0x6d, 0x5f, 0x8f, 0xa7, 0xc4, 0x3b);

		// MF_EVENT_STREAM_METADATA_KEYDATA {CD59A4A1-4A3B-4BBD-8665-72A40FBEA776}
		public static readonly Guid MF_EVENT_STREAM_METADATA_KEYDATA = new Guid(0xcd59a4a1, 0x4a3b, 0x4bbd, 0x86, 0x65, 0x72, 0xa4, 0xf, 0xbe, 0xa7, 0x76);

		// MF_EVENT_STREAM_METADATA_CONTENT_KEYIDS {5063449D-CC29-4FC6-A75A-D247B35AF85C}
		public static readonly Guid MF_EVENT_STREAM_METADATA_CONTENT_KEYIDS = new Guid(0x5063449d, 0xcc29, 0x4fc6, 0xa7, 0x5a, 0xd2, 0x47, 0xb3, 0x5a, 0xf8, 0x5c);

		// MF_EVENT_STREAM_METADATA_SYSTEMID {1EA2EF64-BA16-4A36-8719-FE7560BA32AD}
		public static readonly Guid MF_EVENT_STREAM_METADATA_SYSTEMID = new Guid(0x1ea2ef64, 0xba16, 0x4a36, 0x87, 0x19, 0xfe, 0x75, 0x60, 0xba, 0x32, 0xad);

		public static readonly Guid MF_SESSION_APPROX_EVENT_OCCURRENCE_TIME = new Guid(0x190e852f, 0x6238, 0x42d1, 0xb5, 0xaf, 0x69, 0xea, 0x33, 0x8e, 0xf8, 0x50);

		// Media Session Attributes

		public static readonly Guid MF_SESSION_CONTENT_PROTECTION_MANAGER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x74);
		public static readonly Guid MF_SESSION_GLOBAL_TIME = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x72);
		public static readonly Guid MF_SESSION_QUALITY_MANAGER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x73);
		public static readonly Guid MF_SESSION_REMOTE_SOURCE_MODE = new Guid(0xf4033ef4, 0x9bb3, 0x4378, 0x94, 0x1f, 0x85, 0xa0, 0x85, 0x6b, 0xc2, 0x44);
		public static readonly Guid MF_SESSION_SERVER_CONTEXT = new Guid(0xafe5b291, 0x50fa, 0x46e8, 0xb9, 0xbe, 0xc, 0xc, 0x3c, 0xe4, 0xb3, 0xa5);
		public static readonly Guid MF_SESSION_TOPOLOADER = new Guid(0x1e83d482, 0x1f1c, 0x4571, 0x84, 0x5, 0x88, 0xf4, 0xb2, 0x18, 0x1f, 0x71);

		// Media Type Attributes

		// {48eba18e-f8c9-4687-bf11-0a74c9f96a8f}   MF_MT_MAJOR_TYPE                {GUID}
		public static readonly Guid MF_MT_MAJOR_TYPE = new Guid(0x48eba18e, 0xf8c9, 0x4687, 0xbf, 0x11, 0x0a, 0x74, 0xc9, 0xf9, 0x6a, 0x8f);

		// {f7e34c9a-42e8-4714-b74b-cb29d72c35e5}   MF_MT_SUBTYPE                   {GUID}
		public static readonly Guid MF_MT_SUBTYPE = new Guid(0xf7e34c9a, 0x42e8, 0x4714, 0xb7, 0x4b, 0xcb, 0x29, 0xd7, 0x2c, 0x35, 0xe5);

		// {c9173739-5e56-461c-b713-46fb995cb95f}   MF_MT_ALL_SAMPLES_INDEPENDENT   {UINT32 (BOOL)}
		public static readonly Guid MF_MT_ALL_SAMPLES_INDEPENDENT = new Guid(0xc9173739, 0x5e56, 0x461c, 0xb7, 0x13, 0x46, 0xfb, 0x99, 0x5c, 0xb9, 0x5f);

		// {b8ebefaf-b718-4e04-b0a9-116775e3321b}   MF_MT_FIXED_SIZE_SAMPLES        {UINT32 (BOOL)}
		public static readonly Guid MF_MT_FIXED_SIZE_SAMPLES = new Guid(0xb8ebefaf, 0xb718, 0x4e04, 0xb0, 0xa9, 0x11, 0x67, 0x75, 0xe3, 0x32, 0x1b);

		// {3afd0cee-18f2-4ba5-a110-8bea502e1f92}   MF_MT_COMPRESSED                {UINT32 (BOOL)}
		public static readonly Guid MF_MT_COMPRESSED = new Guid(0x3afd0cee, 0x18f2, 0x4ba5, 0xa1, 0x10, 0x8b, 0xea, 0x50, 0x2e, 0x1f, 0x92);

		// {dad3ab78-1990-408b-bce2-eba673dacc10}   MF_MT_SAMPLE_SIZE               {UINT32}
		public static readonly Guid MF_MT_SAMPLE_SIZE = new Guid(0xdad3ab78, 0x1990, 0x408b, 0xbc, 0xe2, 0xeb, 0xa6, 0x73, 0xda, 0xcc, 0x10);

		// 4d3f7b23-d02f-4e6c-9bee-e4bf2c6c695d     MF_MT_WRAPPED_TYPE              {Blob}
		public static readonly Guid MF_MT_WRAPPED_TYPE = new Guid(0x4d3f7b23, 0xd02f, 0x4e6c, 0x9b, 0xee, 0xe4, 0xbf, 0x2c, 0x6c, 0x69, 0x5d);

		// {37e48bf5-645e-4c5b-89de-ada9e29b696a}   MF_MT_AUDIO_NUM_CHANNELS            {UINT32}
		public static readonly Guid MF_MT_AUDIO_NUM_CHANNELS = new Guid(0x37e48bf5, 0x645e, 0x4c5b, 0x89, 0xde, 0xad, 0xa9, 0xe2, 0x9b, 0x69, 0x6a);

		// {5faeeae7-0290-4c31-9e8a-c534f68d9dba}   MF_MT_AUDIO_SAMPLES_PER_SECOND      {UINT32}
		public static readonly Guid MF_MT_AUDIO_SAMPLES_PER_SECOND = new Guid(0x5faeeae7, 0x0290, 0x4c31, 0x9e, 0x8a, 0xc5, 0x34, 0xf6, 0x8d, 0x9d, 0xba);

		// {fb3b724a-cfb5-4319-aefe-6e42b2406132}   MF_MT_AUDIO_FLOAT_SAMPLES_PER_SECOND {double}
		public static readonly Guid MF_MT_AUDIO_FLOAT_SAMPLES_PER_SECOND = new Guid(0xfb3b724a, 0xcfb5, 0x4319, 0xae, 0xfe, 0x6e, 0x42, 0xb2, 0x40, 0x61, 0x32);

		// {1aab75c8-cfef-451c-ab95-ac034b8e1731}   MF_MT_AUDIO_AVG_BYTES_PER_SECOND    {UINT32}
		public static readonly Guid MF_MT_AUDIO_AVG_BYTES_PER_SECOND = new Guid(0x1aab75c8, 0xcfef, 0x451c, 0xab, 0x95, 0xac, 0x03, 0x4b, 0x8e, 0x17, 0x31);

		// {322de230-9eeb-43bd-ab7a-ff412251541d}   MF_MT_AUDIO_BLOCK_ALIGNMENT         {UINT32}
		public static readonly Guid MF_MT_AUDIO_BLOCK_ALIGNMENT = new Guid(0x322de230, 0x9eeb, 0x43bd, 0xab, 0x7a, 0xff, 0x41, 0x22, 0x51, 0x54, 0x1d);

		// {f2deb57f-40fa-4764-aa33-ed4f2d1ff669}   MF_MT_AUDIO_BITS_PER_SAMPLE         {UINT32}
		public static readonly Guid MF_MT_AUDIO_BITS_PER_SAMPLE = new Guid(0xf2deb57f, 0x40fa, 0x4764, 0xaa, 0x33, 0xed, 0x4f, 0x2d, 0x1f, 0xf6, 0x69);

		// {d9bf8d6a-9530-4b7c-9ddf-ff6fd58bbd06}   MF_MT_AUDIO_VALID_BITS_PER_SAMPLE   {UINT32}
		public static readonly Guid MF_MT_AUDIO_VALID_BITS_PER_SAMPLE = new Guid(0xd9bf8d6a, 0x9530, 0x4b7c, 0x9d, 0xdf, 0xff, 0x6f, 0xd5, 0x8b, 0xbd, 0x06);

		// {aab15aac-e13a-4995-9222-501ea15c6877}   MF_MT_AUDIO_SAMPLES_PER_BLOCK       {UINT32}
		public static readonly Guid MF_MT_AUDIO_SAMPLES_PER_BLOCK = new Guid(0xaab15aac, 0xe13a, 0x4995, 0x92, 0x22, 0x50, 0x1e, 0xa1, 0x5c, 0x68, 0x77);

		// {55fb5765-644a-4caf-8479-938983bb1588}`  MF_MT_AUDIO_CHANNEL_MASK            {UINT32}
		public static readonly Guid MF_MT_AUDIO_CHANNEL_MASK = new Guid(0x55fb5765, 0x644a, 0x4caf, 0x84, 0x79, 0x93, 0x89, 0x83, 0xbb, 0x15, 0x88);

		// {9d62927c-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_FOLDDOWN_MATRIX         {BLOB, MFFOLDDOWN_MATRIX}
		public static readonly Guid MF_MT_AUDIO_FOLDDOWN_MATRIX = new Guid(0x9d62927c, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

		// {0x9d62927d-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_PEAKREF         {UINT32}
		public static readonly Guid MF_MT_AUDIO_WMADRC_PEAKREF = new Guid(0x9d62927d, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

		// {0x9d62927e-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_PEAKTARGET        {UINT32}
		public static readonly Guid MF_MT_AUDIO_WMADRC_PEAKTARGET = new Guid(0x9d62927e, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

		// {0x9d62927f-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_AVGREF         {UINT32}
		public static readonly Guid MF_MT_AUDIO_WMADRC_AVGREF = new Guid(0x9d62927f, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

		// {0x9d629280-36be-4cf2-b5c4-a3926e3e8711}`  MF_MT_AUDIO_WMADRC_AVGTARGET      {UINT32}
		public static readonly Guid MF_MT_AUDIO_WMADRC_AVGTARGET = new Guid(0x9d629280, 0x36be, 0x4cf2, 0xb5, 0xc4, 0xa3, 0x92, 0x6e, 0x3e, 0x87, 0x11);

		// {a901aaba-e037-458a-bdf6-545be2074042}   MF_MT_AUDIO_PREFER_WAVEFORMATEX     {UINT32 (BOOL)}
		public static readonly Guid MF_MT_AUDIO_PREFER_WAVEFORMATEX = new Guid(0xa901aaba, 0xe037, 0x458a, 0xbd, 0xf6, 0x54, 0x5b, 0xe2, 0x07, 0x40, 0x42);

		// {BFBABE79-7434-4d1c-94F0-72A3B9E17188} MF_MT_AAC_PAYLOAD_TYPE       {UINT32}
		public static readonly Guid MF_MT_AAC_PAYLOAD_TYPE = new Guid(0xbfbabe79, 0x7434, 0x4d1c, 0x94, 0xf0, 0x72, 0xa3, 0xb9, 0xe1, 0x71, 0x88);

		// {7632F0E6-9538-4d61-ACDA-EA29C8C14456} MF_MT_AAC_AUDIO_PROFILE_LEVEL_INDICATION       {UINT32}
		public static readonly Guid MF_MT_AAC_AUDIO_PROFILE_LEVEL_INDICATION = new Guid(0x7632f0e6, 0x9538, 0x4d61, 0xac, 0xda, 0xea, 0x29, 0xc8, 0xc1, 0x44, 0x56);

		// {1652c33d-d6b2-4012-b834-72030849a37d}   MF_MT_FRAME_SIZE                {UINT64 (HI32(Width),LO32(Height))}
		public static readonly Guid MF_MT_FRAME_SIZE = new Guid(0x1652c33d, 0xd6b2, 0x4012, 0xb8, 0x34, 0x72, 0x03, 0x08, 0x49, 0xa3, 0x7d);

		// {c459a2e8-3d2c-4e44-b132-fee5156c7bb0}   MF_MT_FRAME_RATE                {UINT64 (HI32(Numerator),LO32(Denominator))}
		public static readonly Guid MF_MT_FRAME_RATE = new Guid(0xc459a2e8, 0x3d2c, 0x4e44, 0xb1, 0x32, 0xfe, 0xe5, 0x15, 0x6c, 0x7b, 0xb0);

		// {c6376a1e-8d0a-4027-be45-6d9a0ad39bb6}   MF_MT_PIXEL_ASPECT_RATIO        {UINT64 (HI32(Numerator),LO32(Denominator))}
		public static readonly Guid MF_MT_PIXEL_ASPECT_RATIO = new Guid(0xc6376a1e, 0x8d0a, 0x4027, 0xbe, 0x45, 0x6d, 0x9a, 0x0a, 0xd3, 0x9b, 0xb6);

		// {8772f323-355a-4cc7-bb78-6d61a048ae82}   MF_MT_DRM_FLAGS                 {UINT32 (anyof MFVideoDRMFlags)}
		public static readonly Guid MF_MT_DRM_FLAGS = new Guid(0x8772f323, 0x355a, 0x4cc7, 0xbb, 0x78, 0x6d, 0x61, 0xa0, 0x48, 0xae, 0x82);

		// {24974215-1B7B-41e4-8625-AC469F2DEDAA}   MF_MT_TIMESTAMP_CAN_BE_DTS      {UINT32 (BOOL)}
		public static readonly Guid MF_MT_TIMESTAMP_CAN_BE_DTS = new Guid(0x24974215, 0x1b7b, 0x41e4, 0x86, 0x25, 0xac, 0x46, 0x9f, 0x2d, 0xed, 0xaa);

		// {A20AF9E8-928A-4B26-AAA9-F05C74CAC47C}   MF_MT_MPEG2_STANDARD            {UINT32 (0 for default MPEG2, 1  to use ATSC standard, 2 to use DVB standard, 3 to use ARIB standard)}
		public static readonly Guid MF_MT_MPEG2_STANDARD = new Guid(0xa20af9e8, 0x928a, 0x4b26, 0xaa, 0xa9, 0xf0, 0x5c, 0x74, 0xca, 0xc4, 0x7c);

		// {5229BA10-E29D-4F80-A59C-DF4F180207D2}   MF_MT_MPEG2_TIMECODE            {UINT32 (0 for no timecode, 1 to append an 4 byte timecode to the front of each transport packet)}
		public static readonly Guid MF_MT_MPEG2_TIMECODE = new Guid(0x5229ba10, 0xe29d, 0x4f80, 0xa5, 0x9c, 0xdf, 0x4f, 0x18, 0x2, 0x7, 0xd2);

		// {825D55E4-4F12-4197-9EB3-59B6E4710F06}   MF_MT_MPEG2_CONTENT_PACKET      {UINT32 (0 for no content packet, 1 to append a 14 byte Content Packet header according to the ARIB specification to the beginning a transport packet at 200-1000 ms intervals.)}
		public static readonly Guid MF_MT_MPEG2_CONTENT_PACKET = new Guid(0x825d55e4, 0x4f12, 0x4197, 0x9e, 0xb3, 0x59, 0xb6, 0xe4, 0x71, 0xf, 0x6);

		//
		// VIDEO - H264 extra data
		//

		// {F5929986-4C45-4FBB-BB49-6CC534D05B9B}  {UINT32, UVC 1.5 H.264 format descriptor: bMaxCodecConfigDelay}
		public static readonly Guid MF_MT_H264_MAX_CODEC_CONFIG_DELAY = new Guid(0xf5929986, 0x4c45, 0x4fbb, 0xbb, 0x49, 0x6c, 0xc5, 0x34, 0xd0, 0x5b, 0x9b);

		// {C8BE1937-4D64-4549-8343-A8086C0BFDA5} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedSliceModes}
		public static readonly Guid MF_MT_H264_SUPPORTED_SLICE_MODES = new Guid(0xc8be1937, 0x4d64, 0x4549, 0x83, 0x43, 0xa8, 0x8, 0x6c, 0xb, 0xfd, 0xa5);

		// {89A52C01-F282-48D2-B522-22E6AE633199} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedSyncFrameTypes}
		public static readonly Guid MF_MT_H264_SUPPORTED_SYNC_FRAME_TYPES = new Guid(0x89a52c01, 0xf282, 0x48d2, 0xb5, 0x22, 0x22, 0xe6, 0xae, 0x63, 0x31, 0x99);

		// {E3854272-F715-4757-BA90-1B696C773457} {UINT32, UVC 1.5 H.264 format descriptor: bResolutionScaling}
		public static readonly Guid MF_MT_H264_RESOLUTION_SCALING = new Guid(0xe3854272, 0xf715, 0x4757, 0xba, 0x90, 0x1b, 0x69, 0x6c, 0x77, 0x34, 0x57);

		// {9EA2D63D-53F0-4A34-B94E-9DE49A078CB3} {UINT32, UVC 1.5 H.264 format descriptor: bSimulcastSupport}
		public static readonly Guid MF_MT_H264_SIMULCAST_SUPPORT = new Guid(0x9ea2d63d, 0x53f0, 0x4a34, 0xb9, 0x4e, 0x9d, 0xe4, 0x9a, 0x7, 0x8c, 0xb3);

		// {6A8AC47E-519C-4F18-9BB3-7EEAAEA5594D} {UINT32, UVC 1.5 H.264 format descriptor: bmSupportedRateControlModes}
		public static readonly Guid MF_MT_H264_SUPPORTED_RATE_CONTROL_MODES = new Guid(0x6a8ac47e, 0x519c, 0x4f18, 0x9b, 0xb3, 0x7e, 0xea, 0xae, 0xa5, 0x59, 0x4d);

		// {45256D30-7215-4576-9336-B0F1BCD59BB2}  {Blob of size 20 * sizeof(WORD), UVC 1.5 H.264 format descriptor: wMaxMBperSec*}
		public static readonly Guid MF_MT_H264_MAX_MB_PER_SEC = new Guid(0x45256d30, 0x7215, 0x4576, 0x93, 0x36, 0xb0, 0xf1, 0xbc, 0xd5, 0x9b, 0xb2);

		// {60B1A998-DC01-40CE-9736-ABA845A2DBDC}         {UINT32, UVC 1.5 H.264 frame descriptor: bmSupportedUsages}
		public static readonly Guid MF_MT_H264_SUPPORTED_USAGES = new Guid(0x60b1a998, 0xdc01, 0x40ce, 0x97, 0x36, 0xab, 0xa8, 0x45, 0xa2, 0xdb, 0xdc);

		// {BB3BD508-490A-11E0-99E4-1316DFD72085}         {UINT32, UVC 1.5 H.264 frame descriptor: bmCapabilities}
		public static readonly Guid MF_MT_H264_CAPABILITIES = new Guid(0xbb3bd508, 0x490a, 0x11e0, 0x99, 0xe4, 0x13, 0x16, 0xdf, 0xd7, 0x20, 0x85);

		// {F8993ABE-D937-4A8F-BBCA-6966FE9E1152}         {UINT32, UVC 1.5 H.264 frame descriptor: bmSVCCapabilities}
		public static readonly Guid MF_MT_H264_SVC_CAPABILITIES = new Guid(0xf8993abe, 0xd937, 0x4a8f, 0xbb, 0xca, 0x69, 0x66, 0xfe, 0x9e, 0x11, 0x52);

		// {359CE3A5-AF00-49CA-A2F4-2AC94CA82B61}         {UINT32, UVC 1.5 H.264 Probe/Commit Control: bUsage}
		public static readonly Guid MF_MT_H264_USAGE = new Guid(0x359ce3a5, 0xaf00, 0x49ca, 0xa2, 0xf4, 0x2a, 0xc9, 0x4c, 0xa8, 0x2b, 0x61);

		//{705177D8-45CB-11E0-AC7D-B91CE0D72085}          {UINT32, UVC 1.5 H.264 Probe/Commit Control: bmRateControlModes}
		public static readonly Guid MF_MT_H264_RATE_CONTROL_MODES = new Guid(0x705177d8, 0x45cb, 0x11e0, 0xac, 0x7d, 0xb9, 0x1c, 0xe0, 0xd7, 0x20, 0x85);

		//{85E299B2-90E3-4FE8-B2F5-C067E0BFE57A}          {UINT64, UVC 1.5 H.264 Probe/Commit Control: bmLayoutPerStream}
		public static readonly Guid MF_MT_H264_LAYOUT_PER_STREAM = new Guid(0x85e299b2, 0x90e3, 0x4fe8, 0xb2, 0xf5, 0xc0, 0x67, 0xe0, 0xbf, 0xe5, 0x7a);

		// {4d0e73e5-80ea-4354-a9d0-1176ceb028ea}   MF_MT_PAD_CONTROL_FLAGS         {UINT32 (oneof MFVideoPadFlags)}
		public static readonly Guid MF_MT_PAD_CONTROL_FLAGS = new Guid(0x4d0e73e5, 0x80ea, 0x4354, 0xa9, 0xd0, 0x11, 0x76, 0xce, 0xb0, 0x28, 0xea);

		// {68aca3cc-22d0-44e6-85f8-28167197fa38}   MF_MT_SOURCE_CONTENT_HINT       {UINT32 (oneof MFVideoSrcContentHintFlags)}
		public static readonly Guid MF_MT_SOURCE_CONTENT_HINT = new Guid(0x68aca3cc, 0x22d0, 0x44e6, 0x85, 0xf8, 0x28, 0x16, 0x71, 0x97, 0xfa, 0x38);

		// {65df2370-c773-4c33-aa64-843e068efb0c}   MF_MT_CHROMA_SITING             {UINT32 (anyof MFVideoChromaSubsampling)}
		public static readonly Guid MF_MT_VIDEO_CHROMA_SITING = new Guid(0x65df2370, 0xc773, 0x4c33, 0xaa, 0x64, 0x84, 0x3e, 0x06, 0x8e, 0xfb, 0x0c);

		// {e2724bb8-e676-4806-b4b2-a8d6efb44ccd}   MF_MT_INTERLACE_MODE            {UINT32 (oneof MFVideoInterlaceMode)}
		public static readonly Guid MF_MT_INTERLACE_MODE = new Guid(0xe2724bb8, 0xe676, 0x4806, 0xb4, 0xb2, 0xa8, 0xd6, 0xef, 0xb4, 0x4c, 0xcd);

		// {5fb0fce9-be5c-4935-a811-ec838f8eed93}   MF_MT_TRANSFER_FUNCTION         {UINT32 (oneof MFVideoTransferFunction)}
		public static readonly Guid MF_MT_TRANSFER_FUNCTION = new Guid(0x5fb0fce9, 0xbe5c, 0x4935, 0xa8, 0x11, 0xec, 0x83, 0x8f, 0x8e, 0xed, 0x93);

		// {dbfbe4d7-0740-4ee0-8192-850ab0e21935}   MF_MT_VIDEO_PRIMARIES           {UINT32 (oneof MFVideoPrimaries)}
		public static readonly Guid MF_MT_VIDEO_PRIMARIES = new Guid(0xdbfbe4d7, 0x0740, 0x4ee0, 0x81, 0x92, 0x85, 0x0a, 0xb0, 0xe2, 0x19, 0x35);

		// {47537213-8cfb-4722-aa34-fbc9e24d77b8}   MF_MT_CUSTOM_VIDEO_PRIMARIES    {BLOB (MT_CUSTOM_VIDEO_PRIMARIES)}
		public static readonly Guid MF_MT_CUSTOM_VIDEO_PRIMARIES = new Guid(0x47537213, 0x8cfb, 0x4722, 0xaa, 0x34, 0xfb, 0xc9, 0xe2, 0x4d, 0x77, 0xb8);

		// {3e23d450-2c75-4d25-a00e-b91670d12327}   MF_MT_YUV_MATRIX                {UINT32 (oneof MFVideoTransferMatrix)}
		public static readonly Guid MF_MT_YUV_MATRIX = new Guid(0x3e23d450, 0x2c75, 0x4d25, 0xa0, 0x0e, 0xb9, 0x16, 0x70, 0xd1, 0x23, 0x27);

		// {53a0529c-890b-4216-8bf9-599367ad6d20}   MF_MT_VIDEO_LIGHTING            {UINT32 (oneof MFVideoLighting)}
		public static readonly Guid MF_MT_VIDEO_LIGHTING = new Guid(0x53a0529c, 0x890b, 0x4216, 0x8b, 0xf9, 0x59, 0x93, 0x67, 0xad, 0x6d, 0x20);

		// {c21b8ee5-b956-4071-8daf-325edf5cab11}   MF_MT_VIDEO_NOMINAL_RANGE       {UINT32 (oneof MFNominalRange)}
		public static readonly Guid MF_MT_VIDEO_NOMINAL_RANGE = new Guid(0xc21b8ee5, 0xb956, 0x4071, 0x8d, 0xaf, 0x32, 0x5e, 0xdf, 0x5c, 0xab, 0x11);

		// {66758743-7e5f-400d-980a-aa8596c85696}   MF_MT_GEOMETRIC_APERTURE        {BLOB (MFVideoArea)}
		public static readonly Guid MF_MT_GEOMETRIC_APERTURE = new Guid(0x66758743, 0x7e5f, 0x400d, 0x98, 0x0a, 0xaa, 0x85, 0x96, 0xc8, 0x56, 0x96);

		// {d7388766-18fe-48c6-a177-ee894867c8c4}   MF_MT_MINIMUM_DISPLAY_APERTURE  {BLOB (MFVideoArea)}
		public static readonly Guid MF_MT_MINIMUM_DISPLAY_APERTURE = new Guid(0xd7388766, 0x18fe, 0x48c6, 0xa1, 0x77, 0xee, 0x89, 0x48, 0x67, 0xc8, 0xc4);

		// {79614dde-9187-48fb-b8c7-4d52689de649}   MF_MT_PAN_SCAN_APERTURE         {BLOB (MFVideoArea)}
		public static readonly Guid MF_MT_PAN_SCAN_APERTURE = new Guid(0x79614dde, 0x9187, 0x48fb, 0xb8, 0xc7, 0x4d, 0x52, 0x68, 0x9d, 0xe6, 0x49);

		// {4b7f6bc3-8b13-40b2-a993-abf630b8204e}   MF_MT_PAN_SCAN_ENABLED          {UINT32 (BOOL)}
		public static readonly Guid MF_MT_PAN_SCAN_ENABLED = new Guid(0x4b7f6bc3, 0x8b13, 0x40b2, 0xa9, 0x93, 0xab, 0xf6, 0x30, 0xb8, 0x20, 0x4e);

		// {20332624-fb0d-4d9e-bd0d-cbf6786c102e}   MF_MT_AVG_BITRATE               {UINT32}
		public static readonly Guid MF_MT_AVG_BITRATE = new Guid(0x20332624, 0xfb0d, 0x4d9e, 0xbd, 0x0d, 0xcb, 0xf6, 0x78, 0x6c, 0x10, 0x2e);

		// {799cabd6-3508-4db4-a3c7-569cd533deb1}   MF_MT_AVG_BIT_ERROR_RATE        {UINT32}
		public static readonly Guid MF_MT_AVG_BIT_ERROR_RATE = new Guid(0x799cabd6, 0x3508, 0x4db4, 0xa3, 0xc7, 0x56, 0x9c, 0xd5, 0x33, 0xde, 0xb1);

		// {c16eb52b-73a1-476f-8d62-839d6a020652}   MF_MT_MAX_KEYFRAME_SPACING      {UINT32}
		public static readonly Guid MF_MT_MAX_KEYFRAME_SPACING = new Guid(0xc16eb52b, 0x73a1, 0x476f, 0x8d, 0x62, 0x83, 0x9d, 0x6a, 0x02, 0x06, 0x52);

		// {644b4e48-1e02-4516-b0eb-c01ca9d49ac6}   MF_MT_DEFAULT_STRIDE            {UINT32 (INT32)} // in bytes
		public static readonly Guid MF_MT_DEFAULT_STRIDE = new Guid(0x644b4e48, 0x1e02, 0x4516, 0xb0, 0xeb, 0xc0, 0x1c, 0xa9, 0xd4, 0x9a, 0xc6);

		// {6d283f42-9846-4410-afd9-654d503b1a54}   MF_MT_PALETTE                   {BLOB (array of MFPaletteEntry - usually 256)}
		public static readonly Guid MF_MT_PALETTE = new Guid(0x6d283f42, 0x9846, 0x4410, 0xaf, 0xd9, 0x65, 0x4d, 0x50, 0x3b, 0x1a, 0x54);

		// {b6bc765f-4c3b-40a4-bd51-2535b66fe09d}   MF_MT_USER_DATA                 {BLOB}
		public static readonly Guid MF_MT_USER_DATA = new Guid(0xb6bc765f, 0x4c3b, 0x40a4, 0xbd, 0x51, 0x25, 0x35, 0xb6, 0x6f, 0xe0, 0x9d);

		// {73d1072d-1870-4174-a063-29ff4ff6c11e}
		public static readonly Guid MF_MT_AM_FORMAT_TYPE = new Guid(0x73d1072d, 0x1870, 0x4174, 0xa0, 0x63, 0x29, 0xff, 0x4f, 0xf6, 0xc1, 0x1e);

		// {ad76a80b-2d5c-4e0b-b375-64e520137036}   MF_MT_VIDEO_PROFILE             {UINT32}    This is an alias of  MF_MT_MPEG2_PROFILE
		public static readonly Guid MF_MT_VIDEO_PROFILE = new Guid(0xad76a80b, 0x2d5c, 0x4e0b, 0xb3, 0x75, 0x64, 0xe5, 0x20, 0x13, 0x70, 0x36);

		// {96f66574-11c5-4015-8666-bff516436da7}   MF_MT_VIDEO_LEVEL               {UINT32}    This is an alias of  MF_MT_MPEG2_LEVEL
		public static readonly Guid MF_MT_VIDEO_LEVEL = new Guid(0x96f66574, 0x11c5, 0x4015, 0x86, 0x66, 0xbf, 0xf5, 0x16, 0x43, 0x6d, 0xa7);

		// {91f67885-4333-4280-97cd-bd5a6c03a06e}   MF_MT_MPEG_START_TIME_CODE      {UINT32}
		public static readonly Guid MF_MT_MPEG_START_TIME_CODE = new Guid(0x91f67885, 0x4333, 0x4280, 0x97, 0xcd, 0xbd, 0x5a, 0x6c, 0x03, 0xa0, 0x6e);

		// {ad76a80b-2d5c-4e0b-b375-64e520137036}   MF_MT_MPEG2_PROFILE             {UINT32 (oneof AM_MPEG2Profile)}
		public static readonly Guid MF_MT_MPEG2_PROFILE = new Guid(0xad76a80b, 0x2d5c, 0x4e0b, 0xb3, 0x75, 0x64, 0xe5, 0x20, 0x13, 0x70, 0x36);

		// {96f66574-11c5-4015-8666-bff516436da7}   MF_MT_MPEG2_LEVEL               {UINT32 (oneof AM_MPEG2Level)}
		public static readonly Guid MF_MT_MPEG2_LEVEL = new Guid(0x96f66574, 0x11c5, 0x4015, 0x86, 0x66, 0xbf, 0xf5, 0x16, 0x43, 0x6d, 0xa7);

		// {31e3991d-f701-4b2f-b426-8ae3bda9e04b}   MF_MT_MPEG2_FLAGS               {UINT32 (anyof AMMPEG2_xxx flags)}
		public static readonly Guid MF_MT_MPEG2_FLAGS = new Guid(0x31e3991d, 0xf701, 0x4b2f, 0xb4, 0x26, 0x8a, 0xe3, 0xbd, 0xa9, 0xe0, 0x4b);

		// {3c036de7-3ad0-4c9e-9216-ee6d6ac21cb3}   MF_MT_MPEG_SEQUENCE_HEADER      {BLOB}
		public static readonly Guid MF_MT_MPEG_SEQUENCE_HEADER = new Guid(0x3c036de7, 0x3ad0, 0x4c9e, 0x92, 0x16, 0xee, 0x6d, 0x6a, 0xc2, 0x1c, 0xb3);

		// {84bd5d88-0fb8-4ac8-be4b-a8848bef98f3}   MF_MT_DV_AAUX_SRC_PACK_0        {UINT32}
		public static readonly Guid MF_MT_DV_AAUX_SRC_PACK_0 = new Guid(0x84bd5d88, 0x0fb8, 0x4ac8, 0xbe, 0x4b, 0xa8, 0x84, 0x8b, 0xef, 0x98, 0xf3);

		// {f731004e-1dd1-4515-aabe-f0c06aa536ac}   MF_MT_DV_AAUX_CTRL_PACK_0       {UINT32}
		public static readonly Guid MF_MT_DV_AAUX_CTRL_PACK_0 = new Guid(0xf731004e, 0x1dd1, 0x4515, 0xaa, 0xbe, 0xf0, 0xc0, 0x6a, 0xa5, 0x36, 0xac);

		// {720e6544-0225-4003-a651-0196563a958e}   MF_MT_DV_AAUX_SRC_PACK_1        {UINT32}
		public static readonly Guid MF_MT_DV_AAUX_SRC_PACK_1 = new Guid(0x720e6544, 0x0225, 0x4003, 0xa6, 0x51, 0x01, 0x96, 0x56, 0x3a, 0x95, 0x8e);

		// {cd1f470d-1f04-4fe0-bfb9-d07ae0386ad8}   MF_MT_DV_AAUX_CTRL_PACK_1       {UINT32}
		public static readonly Guid MF_MT_DV_AAUX_CTRL_PACK_1 = new Guid(0xcd1f470d, 0x1f04, 0x4fe0, 0xbf, 0xb9, 0xd0, 0x7a, 0xe0, 0x38, 0x6a, 0xd8);

		// {41402d9d-7b57-43c6-b129-2cb997f15009}   MF_MT_DV_VAUX_SRC_PACK          {UINT32}
		public static readonly Guid MF_MT_DV_VAUX_SRC_PACK = new Guid(0x41402d9d, 0x7b57, 0x43c6, 0xb1, 0x29, 0x2c, 0xb9, 0x97, 0xf1, 0x50, 0x09);

		// {2f84e1c4-0da1-4788-938e-0dfbfbb34b48}   MF_MT_DV_VAUX_CTRL_PACK         {UINT32}
		public static readonly Guid MF_MT_DV_VAUX_CTRL_PACK = new Guid(0x2f84e1c4, 0x0da1, 0x4788, 0x93, 0x8e, 0x0d, 0xfb, 0xfb, 0xb3, 0x4b, 0x48);

		// {5315d8a0-87c5-4697-b793-666c67c49b}         MF_MT_VIDEO_3D_FORMAT           {UINT32 (anyof MFVideo3DFormat)}
		public static readonly Guid MF_MT_VIDEO_3D_FORMAT = new Guid(0x5315d8a0, 0x87c5, 0x4697, 0xb7, 0x93, 0x66, 0x6, 0xc6, 0x7c, 0x4, 0x9b);

		// {BB077E8A-DCBF-42eb-AF60-418DF98AA495}       MF_MT_VIDEO_3D_NUM_VIEW         {UINT32}
		public static readonly Guid MF_MT_VIDEO_3D_NUM_VIEWS = new Guid(0xbb077e8a, 0xdcbf, 0x42eb, 0xaf, 0x60, 0x41, 0x8d, 0xf9, 0x8a, 0xa4, 0x95);

		// {6D4B7BFF-5629-4404-948C-C634F4CE26D4}       MF_MT_VIDEO_3D_LEFT_IS_BASE     {UINT32}
		public static readonly Guid MF_MT_VIDEO_3D_LEFT_IS_BASE = new Guid(0x6d4b7bff, 0x5629, 0x4404, 0x94, 0x8c, 0xc6, 0x34, 0xf4, 0xce, 0x26, 0xd4);

		// {EC298493-0ADA-4ea1-A4FE-CBBD36CE9331}       MF_MT_VIDEO_3D_FIRST_IS_LEFT    {UINT32 (BOOL)}
		public static readonly Guid MF_MT_VIDEO_3D_FIRST_IS_LEFT = new Guid(0xec298493, 0xada, 0x4ea1, 0xa4, 0xfe, 0xcb, 0xbd, 0x36, 0xce, 0x93, 0x31);

		public static readonly Guid MF_MT_VIDEO_ROTATION = new Guid(0xc380465d, 0x2271, 0x428c, 0x9b, 0x83, 0xec, 0xea, 0x3b, 0x4a, 0x85, 0xc1);

		// Sample Attributes

		public static readonly Guid MFSampleExtension_DecodeTimestamp = new Guid(0x73a954d4, 0x9e2, 0x4861, 0xbe, 0xfc, 0x94, 0xbd, 0x97, 0xc0, 0x8e, 0x6e);

		public static readonly Guid MFSampleExtension_VideoEncodeQP = new Guid(0xb2efe478, 0xf979, 0x4c66, 0xb9, 0x5e, 0xee, 0x2b, 0x82, 0xc8, 0x2f, 0x36);

		public static readonly Guid MFSampleExtension_VideoEncodePictureType = new Guid(0x973704e6, 0xcd14, 0x483c, 0x8f, 0x20, 0xc9, 0xfc, 0x9, 0x28, 0xba, 0xd5);

		public static readonly Guid MFSampleExtension_FrameCorruption = new Guid(0xb4dd4a8c, 0xbeb, 0x44c4, 0x8b, 0x75, 0xb0, 0x2b, 0x91, 0x3b, 0x4, 0xf0);

		// {941ce0a3-6ae3-4dda-9a08-a64298340617}   MFSampleExtension_BottomFieldFirst
		public static readonly Guid MFSampleExtension_BottomFieldFirst = new Guid(0x941ce0a3, 0x6ae3, 0x4dda, 0x9a, 0x08, 0xa6, 0x42, 0x98, 0x34, 0x06, 0x17);

		// MFSampleExtension_CleanPoint {9cdf01d8-a0f0-43ba-b077-eaa06cbd728a}
		public static readonly Guid MFSampleExtension_CleanPoint = new Guid(0x9cdf01d8, 0xa0f0, 0x43ba, 0xb0, 0x77, 0xea, 0xa0, 0x6c, 0xbd, 0x72, 0x8a);

		// {6852465a-ae1c-4553-8e9b-c3420fcb1637}   MFSampleExtension_DerivedFromTopField
		public static readonly Guid MFSampleExtension_DerivedFromTopField = new Guid(0x6852465a, 0xae1c, 0x4553, 0x8e, 0x9b, 0xc3, 0x42, 0x0f, 0xcb, 0x16, 0x37);

		// MFSampleExtension_MeanAbsoluteDifference {1cdbde11-08b4-4311-a6dd-0f9f371907aa}
		public static readonly Guid MFSampleExtension_MeanAbsoluteDifference = new Guid(0x1cdbde11, 0x08b4, 0x4311, 0xa6, 0xdd, 0x0f, 0x9f, 0x37, 0x19, 0x07, 0xaa);

		// MFSampleExtension_LongTermReferenceFrameInfo {9154733f-e1bd-41bf-81d3-fcd918f71332}
		public static readonly Guid MFSampleExtension_LongTermReferenceFrameInfo = new Guid(0x9154733f, 0xe1bd, 0x41bf, 0x81, 0xd3, 0xfc, 0xd9, 0x18, 0xf7, 0x13, 0x32);

		// MFSampleExtension_ROIRectangle {3414a438-4998-4d2c-be82-be3ca0b24d43}
		public static readonly Guid MFSampleExtension_ROIRectangle = new Guid(0x3414a438, 0x4998, 0x4d2c, 0xbe, 0x82, 0xbe, 0x3c, 0xa0, 0xb2, 0x4d, 0x43);

		// MFSampleExtension_PhotoThumbnail {74BBC85C-C8BB-42DC-B586DA17FFD35DCC}
		public static readonly Guid MFSampleExtension_PhotoThumbnail = new Guid(0x74BBC85C, 0xC8BB, 0x42DC, 0xB5, 0x86, 0xDA, 0x17, 0xFF, 0xD3, 0x5D, 0xCC);

		// MFSampleExtension_PhotoThumbnailMediaType {61AD5420-EBF8-4143-89AF6BF25F672DEF}
		public static readonly Guid MFSampleExtension_PhotoThumbnailMediaType = new Guid(0x61AD5420, 0xEBF8, 0x4143, 0x89, 0xAF, 0x6B, 0xF2, 0x5F, 0x67, 0x2D, 0xEF);

		// MFSampleExtension_CaptureMetadata
		public static readonly Guid MFSampleExtension_CaptureMetadata = new Guid(0x2EBE23A8, 0xFAF5, 0x444A, 0xA6, 0xA2, 0xEB, 0x81, 0x08, 0x80, 0xAB, 0x5D);

		public static readonly Guid MF_CAPTURE_METADATA_PHOTO_FRAME_FLASH = new Guid(0x0F9DD6C6, 0x6003, 0x45D8, 0xBD, 0x59, 0xF1, 0xF5, 0x3E, 0x3D, 0x04, 0xE8);

		// MFSampleExtension_Discontinuity {9cdf01d9-a0f0-43ba-b077-eaa06cbd728a}
		public static readonly Guid MFSampleExtension_Discontinuity = new Guid(0x9cdf01d9, 0xa0f0, 0x43ba, 0xb0, 0x77, 0xea, 0xa0, 0x6c, 0xbd, 0x72, 0x8a);

		// {b1d5830a-deb8-40e3-90fa-389943716461}   MFSampleExtension_Interlaced
		public static readonly Guid MFSampleExtension_Interlaced = new Guid(0xb1d5830a, 0xdeb8, 0x40e3, 0x90, 0xfa, 0x38, 0x99, 0x43, 0x71, 0x64, 0x61);

		// {304d257c-7493-4fbd-b149-9228de8d9a99}   MFSampleExtension_RepeatFirstField
		public static readonly Guid MFSampleExtension_RepeatFirstField = new Guid(0x304d257c, 0x7493, 0x4fbd, 0xb1, 0x49, 0x92, 0x28, 0xde, 0x8d, 0x9a, 0x99);

		// {9d85f816-658b-455a-bde0-9fa7e15ab8f9}   MFSampleExtension_SingleField
		public static readonly Guid MFSampleExtension_SingleField = new Guid(0x9d85f816, 0x658b, 0x455a, 0xbd, 0xe0, 0x9f, 0xa7, 0xe1, 0x5a, 0xb8, 0xf9);

		// MFSampleExtension_Token {8294da66-f328-4805-b551-00deb4c57a61}
		public static readonly Guid MFSampleExtension_Token = new Guid(0x8294da66, 0xf328, 0x4805, 0xb5, 0x51, 0x00, 0xde, 0xb4, 0xc5, 0x7a, 0x61);

		// MFSampleExtension_3DVideo                    {F86F97A4-DD54-4e2e-9A5E-55FC2D74A005}
		public static readonly Guid MFSampleExtension_3DVideo = new Guid(0xf86f97a4, 0xdd54, 0x4e2e, 0x9a, 0x5e, 0x55, 0xfc, 0x2d, 0x74, 0xa0, 0x05);

		// MFSampleExtension_3DVideo_SampleFormat       {08671772-E36F-4cff-97B3-D72E20987A48}
		public static readonly Guid MFSampleExtension_3DVideo_SampleFormat = new Guid(0x8671772, 0xe36f, 0x4cff, 0x97, 0xb3, 0xd7, 0x2e, 0x20, 0x98, 0x7a, 0x48);

		public static readonly Guid MFSampleExtension_MaxDecodeFrameSize = new Guid(0xd3cc654f, 0xf9f3, 0x4a13, 0x88, 0x9f, 0xf0, 0x4e, 0xb2, 0xb5, 0xb9, 0x57);
		public static readonly Guid MFSampleExtension_AccumulatedNonRefPicPercent = new Guid(0x79ea74df, 0xa740, 0x445b, 0xbc, 0x98, 0xc9, 0xed, 0x1f, 0x26, 0xe, 0xee);
		public static readonly Guid MFSampleExtension_Encryption_SubSample_Mapping = new Guid(0x8444F27A, 0x69A1, 0x48DA, 0xBD, 0x08, 0x11, 0xCE, 0xF3, 0x68, 0x30, 0xD2);
		public static readonly Guid MFSampleExtension_Encryption_ClearSliceHeaderData = new Guid(0x5509a4f4, 0x320d, 0x4e6c, 0x8d, 0x1a, 0x94, 0xc6, 0x6d, 0xd2, 0xc, 0xb0);
		public static readonly Guid MFSampleExtension_Encryption_HardwareProtection_KeyInfoID = new Guid(0x8cbfcceb, 0x94a5, 0x4de1, 0x82, 0x31, 0xa8, 0x5e, 0x47, 0xcf, 0x81, 0xe7);
		public static readonly Guid MFSampleExtension_Encryption_HardwareProtection_KeyInfo = new Guid(0xb2372080, 0x455b, 0x4dd7, 0x99, 0x89, 0x1a, 0x95, 0x57, 0x84, 0xb7, 0x54);
		public static readonly Guid MFSampleExtension_Encryption_HardwareProtection_VideoDecryptorContext = new Guid(0x693470c8, 0xe837, 0x47a0, 0x88, 0xcb, 0x53, 0x5b, 0x90, 0x5e, 0x35, 0x82);
		public static readonly Guid MFSampleExtension_Encryption_Opaque_Data = new Guid(0x224d77e5, 0x1391, 0x4ffb, 0x9f, 0x41, 0xb4, 0x32, 0xf6, 0x8c, 0x61, 0x1d);
		public static readonly Guid MFSampleExtension_NALULengthInfo = new Guid(0x19124E7C, 0xAD4B, 0x465F, 0xBB, 0x18, 0x20, 0x18, 0x62, 0x87, 0xB6, 0xAF);
		public static readonly Guid MFSampleExtension_Encryption_NALUTypes = new Guid(0xb0f067c7, 0x714c, 0x416c, 0x8d, 0x59, 0x5f, 0x4d, 0xdf, 0x89, 0x13, 0xb6);
		public static readonly Guid MFSampleExtension_Encryption_SPSPPSData = new Guid(0xaede0fa2, 0xe0c, 0x453c, 0xb7, 0xf3, 0xde, 0x86, 0x93, 0x36, 0x4d, 0x11);
		public static readonly Guid MFSampleExtension_Encryption_SEIData = new Guid(0x3cf0e972, 0x4542, 0x4687, 0x99, 0x99, 0x58, 0x5f, 0x56, 0x5f, 0xba, 0x7d);
		public static readonly Guid MFSampleExtension_Encryption_HardwareProtection = new Guid(0x9a2b2d2b, 0x8270, 0x43e3, 0x84, 0x48, 0x99, 0x4f, 0x42, 0x6e, 0x88, 0x86);
		public static readonly Guid MFSampleExtension_ClosedCaption_CEA708 = new Guid(0x26f09068, 0xe744, 0x47dc, 0xaa, 0x03, 0xdb, 0xf2, 0x04, 0x03, 0xbd, 0xe6);
		public static readonly Guid MFSampleExtension_DirtyRects = new Guid(0x9ba70225, 0xb342, 0x4e97, 0x91, 0x26, 0x0b, 0x56, 0x6a, 0xb7, 0xea, 0x7e);
		public static readonly Guid MFSampleExtension_MoveRegions = new Guid(0xe2a6c693, 0x3a8b, 0x4b8d, 0x95, 0xd0, 0xf6, 0x02, 0x81, 0xa1, 0x2f, 0xb7);
		public static readonly Guid MFSampleExtension_HDCP_FrameCounter = new Guid(0x9d389c60, 0xf507, 0x4aa6, 0xa4, 0xa, 0x71, 0x2, 0x7a, 0x2, 0xf3, 0xde);
		public static readonly Guid MFSampleExtension_MDLCacheCookie = new Guid(0x5F002AF9, 0xD8F9, 0x41A3, 0xB6, 0xC3, 0xA2, 0xAD, 0x43, 0xF6, 0x47, 0xAD);

		// Sample Grabber Sink Attributes
		public static readonly Guid MF_SAMPLEGRABBERSINK_SAMPLE_TIME_OFFSET = new Guid(0x62e3d776, 0x8100, 0x4e03, 0xa6, 0xe8, 0xbd, 0x38, 0x57, 0xac, 0x9c, 0x47);

		// Stream descriptor Attributes

		public static readonly Guid MF_SD_LANGUAGE = new Guid(0xaf2180, 0xbdc2, 0x423c, 0xab, 0xca, 0xf5, 0x3, 0x59, 0x3b, 0xc1, 0x21);
		public static readonly Guid MF_SD_PROTECTED = new Guid(0xaf2181, 0xbdc2, 0x423c, 0xab, 0xca, 0xf5, 0x3, 0x59, 0x3b, 0xc1, 0x21);
		public static readonly Guid MF_SD_SAMI_LANGUAGE = new Guid(0x36fcb98a, 0x6cd0, 0x44cb, 0xac, 0xb9, 0xa8, 0xf5, 0x60, 0xd, 0xd0, 0xbb);

		// Topology Attributes
		public static readonly Guid MF_TOPOLOGY_NO_MARKIN_MARKOUT = new Guid(0x7ed3f804, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
		public static readonly Guid MF_TOPOLOGY_PROJECTSTART = new Guid(0x7ed3f802, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
		public static readonly Guid MF_TOPOLOGY_PROJECTSTOP = new Guid(0x7ed3f803, 0x86bb, 0x4b3f, 0xb7, 0xe4, 0x7c, 0xb4, 0x3a, 0xfd, 0x4b, 0x80);
		public static readonly Guid MF_TOPOLOGY_RESOLUTION_STATUS = new Guid(0x494bbcde, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);

		// Topology Node Attributes
		public static readonly Guid MF_TOPONODE_CONNECT_METHOD = new Guid(0x494bbcf1, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_D3DAWARE = new Guid(0x494bbced, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_DECODER = new Guid(0x494bbd02, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_DECRYPTOR = new Guid(0x494bbcfa, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_DISABLE_PREROLL = new Guid(0x14932f9e, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
		public static readonly Guid MF_TOPONODE_DISCARDABLE = new Guid(0x494bbcfb, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_DRAIN = new Guid(0x494bbce9, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_ERROR_MAJORTYPE = new Guid(0x494bbcfd, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_ERROR_SUBTYPE = new Guid(0x494bbcfe, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_ERRORCODE = new Guid(0x494bbcee, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_FLUSH = new Guid(0x494bbce8, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_LOCKED = new Guid(0x494bbcf7, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_MARKIN_HERE = new Guid(0x494bbd00, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_MARKOUT_HERE = new Guid(0x494bbd01, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_MEDIASTART = new Guid(0x835c58ea, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_MEDIASTOP = new Guid(0x835c58eb, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_NOSHUTDOWN_ON_REMOVE = new Guid(0x14932f9c, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
		public static readonly Guid MF_TOPONODE_PRESENTATION_DESCRIPTOR = new Guid(0x835c58ed, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_PRIMARYOUTPUT = new Guid(0x6304ef99, 0x16b2, 0x4ebe, 0x9d, 0x67, 0xe4, 0xc5, 0x39, 0xb3, 0xa2, 0x59);
		public static readonly Guid MF_TOPONODE_RATELESS = new Guid(0x14932f9d, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
		public static readonly Guid MF_TOPONODE_SEQUENCE_ELEMENTID = new Guid(0x835c58ef, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_SOURCE = new Guid(0x835c58ec, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_STREAM_DESCRIPTOR = new Guid(0x835c58ee, 0xe075, 0x4bc7, 0xbc, 0xba, 0x4d, 0xe0, 0x00, 0xdf, 0x9a, 0xe6);
		public static readonly Guid MF_TOPONODE_STREAMID = new Guid(0x14932f9b, 0x9087, 0x4bb4, 0x84, 0x12, 0x51, 0x67, 0x14, 0x5c, 0xbe, 0x04);
		public static readonly Guid MF_TOPONODE_TRANSFORM_OBJECTID = new Guid(0x88dcc0c9, 0x293e, 0x4e8b, 0x9a, 0xeb, 0xa, 0xd6, 0x4c, 0xc0, 0x16, 0xb0);
		public static readonly Guid MF_TOPONODE_WORKQUEUE_ID = new Guid(0x494bbcf8, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_CLASS = new Guid(0x494bbcf9, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);
		public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_TASKID = new Guid(0x494bbcff, 0xb031, 0x4e38, 0x97, 0xc4, 0xd5, 0x42, 0x2d, 0xd6, 0x18, 0xdc);

		// Transform Attributes
		public static readonly Guid MF_ACTIVATE_MFT_LOCKED = new Guid(0xc1f6093c, 0x7f65, 0x4fbd, 0x9e, 0x39, 0x5f, 0xae, 0xc3, 0xc4, 0xfb, 0xd7);
		public static readonly Guid MF_SA_D3D_AWARE = new Guid(0xeaa35c29, 0x775e, 0x488e, 0x9b, 0x61, 0xb3, 0x28, 0x3e, 0x49, 0x58, 0x3b);
		public static readonly Guid MFT_SUPPORT_3DVIDEO = new Guid(0x93f81b1, 0x4f2e, 0x4631, 0x81, 0x68, 0x79, 0x34, 0x3, 0x2a, 0x1, 0xd3);

		// {53476A11-3F13-49fb-AC42-EE2733C96741} MFT_SUPPORT_DYNAMIC_FORMAT_CHANGE {UINT32 (BOOL)}
		public static readonly Guid MFT_SUPPORT_DYNAMIC_FORMAT_CHANGE = new Guid(0x53476a11, 0x3f13, 0x49fb, 0xac, 0x42, 0xee, 0x27, 0x33, 0xc9, 0x67, 0x41);

		public static readonly Guid MFT_REMUX_MARK_I_PICTURE_AS_CLEAN_POINT = new Guid(0x364e8f85, 0x3f2e, 0x436c, 0xb2, 0xa2, 0x44, 0x40, 0xa0, 0x12, 0xa9, 0xe8);
		public static readonly Guid MFT_ENCODER_SUPPORTS_CONFIG_EVENT = new Guid(0x86a355ae, 0x3a77, 0x4ec4, 0x9f, 0x31, 0x1, 0x14, 0x9a, 0x4e, 0x92, 0xde);

		// Presentation Descriptor Attributes

		public static readonly Guid MF_PD_APP_CONTEXT = new Guid(0x6c990d32, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_DURATION = new Guid(0x6c990d33, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_LAST_MODIFIED_TIME = new Guid(0x6c990d38, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_MIME_TYPE = new Guid(0x6c990d37, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_PMPHOST_CONTEXT = new Guid(0x6c990d31, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_SAMI_STYLELIST = new Guid(0xe0b73c7f, 0x486d, 0x484e, 0x98, 0x72, 0x4d, 0xe5, 0x19, 0x2a, 0x7b, 0xf8);
		public static readonly Guid MF_PD_TOTAL_FILE_SIZE = new Guid(0x6c990d34, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_AUDIO_ENCODING_BITRATE = new Guid(0x6c990d35, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_VIDEO_ENCODING_BITRATE = new Guid(0x6c990d36, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);

		// wmcontainer.h Attributes
		public static readonly Guid MFASFSampleExtension_SampleDuration = new Guid(0xc6bd9450, 0x867f, 0x4907, 0x83, 0xa3, 0xc7, 0x79, 0x21, 0xb7, 0x33, 0xad);
		public static readonly Guid MFSampleExtension_SampleKeyID = new Guid(0x9ed713c8, 0x9b87, 0x4b26, 0x82, 0x97, 0xa9, 0x3b, 0x0c, 0x5a, 0x8a, 0xcc);

		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_FILE_ID = new Guid(0x3de649b4, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_CREATION_TIME = new Guid(0x3de649b6, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PACKETS = new Guid(0x3de649b7, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PLAY_DURATION = new Guid(0x3de649b8, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_SEND_DURATION = new Guid(0x3de649b9, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_PREROLL = new Guid(0x3de649ba, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_FLAGS = new Guid(0x3de649bb, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MIN_PACKET_SIZE = new Guid(0x3de649bc, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MAX_PACKET_SIZE = new Guid(0x3de649bd, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_FILEPROPERTIES_MAX_BITRATE = new Guid(0x3de649be, 0xd76d, 0x4e66, 0x9e, 0xc9, 0x78, 0x12, 0xf, 0xb4, 0xc7, 0xe3);
		public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_TYPE = new Guid(0x8520fe3d, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
		public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_KEYID = new Guid(0x8520fe3e, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
		public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_SECRET_DATA = new Guid(0x8520fe3f, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
		public static readonly Guid MF_PD_ASF_CONTENTENCRYPTION_LICENSE_URL = new Guid(0x8520fe40, 0x277e, 0x46ea, 0x99, 0xe4, 0xe3, 0xa, 0x86, 0xdb, 0x12, 0xbe);
		public static readonly Guid MF_PD_ASF_CONTENTENCRYPTIONEX_ENCRYPTION_DATA = new Guid(0x62508be5, 0xecdf, 0x4924, 0xa3, 0x59, 0x72, 0xba, 0xb3, 0x39, 0x7b, 0x9d);
		public static readonly Guid MF_PD_ASF_LANGLIST = new Guid(0xf23de43c, 0x9977, 0x460d, 0xa6, 0xec, 0x32, 0x93, 0x7f, 0x16, 0xf, 0x7d);
		public static readonly Guid MF_PD_ASF_LANGLIST_LEGACYORDER = new Guid(0xf23de43d, 0x9977, 0x460d, 0xa6, 0xec, 0x32, 0x93, 0x7f, 0x16, 0xf, 0x7d);
		public static readonly Guid MF_PD_ASF_MARKER = new Guid(0x5134330e, 0x83a6, 0x475e, 0xa9, 0xd5, 0x4f, 0xb8, 0x75, 0xfb, 0x2e, 0x31);
		public static readonly Guid MF_PD_ASF_SCRIPT = new Guid(0xe29cd0d7, 0xd602, 0x4923, 0xa7, 0xfe, 0x73, 0xfd, 0x97, 0xec, 0xc6, 0x50);
		public static readonly Guid MF_PD_ASF_CODECLIST = new Guid(0xe4bb3509, 0xc18d, 0x4df1, 0xbb, 0x99, 0x7a, 0x36, 0xb3, 0xcc, 0x41, 0x19);
		public static readonly Guid MF_PD_ASF_METADATA_IS_VBR = new Guid(0x5fc6947a, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
		public static readonly Guid MF_PD_ASF_METADATA_V8_VBRPEAK = new Guid(0x5fc6947b, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
		public static readonly Guid MF_PD_ASF_METADATA_V8_BUFFERAVERAGE = new Guid(0x5fc6947c, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
		public static readonly Guid MF_PD_ASF_METADATA_LEAKY_BUCKET_PAIRS = new Guid(0x5fc6947d, 0xef60, 0x445d, 0xb4, 0x49, 0x44, 0x2e, 0xcc, 0x78, 0xb4, 0xc1);
		public static readonly Guid MF_PD_ASF_DATA_START_OFFSET = new Guid(0xe7d5b3e7, 0x1f29, 0x45d3, 0x88, 0x22, 0x3e, 0x78, 0xfa, 0xe2, 0x72, 0xed);
		public static readonly Guid MF_PD_ASF_DATA_LENGTH = new Guid(0xe7d5b3e8, 0x1f29, 0x45d3, 0x88, 0x22, 0x3e, 0x78, 0xfa, 0xe2, 0x72, 0xed);
		public static readonly Guid MF_SD_ASF_EXTSTRMPROP_LANGUAGE_ID_INDEX = new Guid(0x48f8a522, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
		public static readonly Guid MF_SD_ASF_EXTSTRMPROP_AVG_DATA_BITRATE = new Guid(0x48f8a523, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
		public static readonly Guid MF_SD_ASF_EXTSTRMPROP_AVG_BUFFERSIZE = new Guid(0x48f8a524, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
		public static readonly Guid MF_SD_ASF_EXTSTRMPROP_MAX_DATA_BITRATE = new Guid(0x48f8a525, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
		public static readonly Guid MF_SD_ASF_EXTSTRMPROP_MAX_BUFFERSIZE = new Guid(0x48f8a526, 0x305d, 0x422d, 0x85, 0x24, 0x25, 0x2, 0xdd, 0xa3, 0x36, 0x80);
		public static readonly Guid MF_SD_ASF_STREAMBITRATES_BITRATE = new Guid(0xa8e182ed, 0xafc8, 0x43d0, 0xb0, 0xd1, 0xf6, 0x5b, 0xad, 0x9d, 0xa5, 0x58);
		public static readonly Guid MF_SD_ASF_METADATA_DEVICE_CONFORMANCE_TEMPLATE = new Guid(0x245e929d, 0xc44e, 0x4f7e, 0xbb, 0x3c, 0x77, 0xd4, 0xdf, 0xd2, 0x7f, 0x8a);
		public static readonly Guid MF_PD_ASF_INFO_HAS_AUDIO = new Guid(0x80e62295, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
		public static readonly Guid MF_PD_ASF_INFO_HAS_VIDEO = new Guid(0x80e62296, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
		public static readonly Guid MF_PD_ASF_INFO_HAS_NON_AUDIO_VIDEO = new Guid(0x80e62297, 0x2296, 0x4a44, 0xb3, 0x1c, 0xd1, 0x3, 0xc6, 0xfe, 0xd2, 0x3c);
		public static readonly Guid MF_ASFSTREAMCONFIG_LEAKYBUCKET1 = new Guid(0xc69b5901, 0xea1a, 0x4c9b, 0xb6, 0x92, 0xe2, 0xa0, 0xd2, 0x9a, 0x8a, 0xdd);
		public static readonly Guid MF_ASFSTREAMCONFIG_LEAKYBUCKET2 = new Guid(0xc69b5902, 0xea1a, 0x4c9b, 0xb6, 0x92, 0xe2, 0xa0, 0xd2, 0x9a, 0x8a, 0xdd);

		// Arbitrary

		// {9E6BD6F5-0109-4f95-84AC-9309153A19FC}   MF_MT_ARBITRARY_HEADER          {MT_ARBITRARY_HEADER}
		public static readonly Guid MF_MT_ARBITRARY_HEADER = new Guid(0x9e6bd6f5, 0x109, 0x4f95, 0x84, 0xac, 0x93, 0x9, 0x15, 0x3a, 0x19, 0xfc);

		// {5A75B249-0D7D-49a1-A1C3-E0D87F0CADE5}   MF_MT_ARBITRARY_FORMAT          {Blob}
		public static readonly Guid MF_MT_ARBITRARY_FORMAT = new Guid(0x5a75b249, 0xd7d, 0x49a1, 0xa1, 0xc3, 0xe0, 0xd8, 0x7f, 0xc, 0xad, 0xe5);

		// Image

		// {ED062CF4-E34E-4922-BE99-934032133D7C}   MF_MT_IMAGE_LOSS_TOLERANT       {UINT32 (BOOL)}
		public static readonly Guid MF_MT_IMAGE_LOSS_TOLERANT = new Guid(0xed062cf4, 0xe34e, 0x4922, 0xbe, 0x99, 0x93, 0x40, 0x32, 0x13, 0x3d, 0x7c);

		// MPEG-4 Media Type Attributes

		// {261E9D83-9529-4B8F-A111-8B9C950A81A9}   MF_MT_MPEG4_SAMPLE_DESCRIPTION   {BLOB}
		public static readonly Guid MF_MT_MPEG4_SAMPLE_DESCRIPTION = new Guid(0x261e9d83, 0x9529, 0x4b8f, 0xa1, 0x11, 0x8b, 0x9c, 0x95, 0x0a, 0x81, 0xa9);

		// {9aa7e155-b64a-4c1d-a500-455d600b6560}   MF_MT_MPEG4_CURRENT_SAMPLE_ENTRY {UINT32}
		public static readonly Guid MF_MT_MPEG4_CURRENT_SAMPLE_ENTRY = new Guid(0x9aa7e155, 0xb64a, 0x4c1d, 0xa5, 0x00, 0x45, 0x5d, 0x60, 0x0b, 0x65, 0x60);

		// Save original format information for AVI and WAV files

		// {d7be3fe0-2bc7-492d-b843-61a1919b70c3}   MF_MT_ORIGINAL_4CC               (UINT32)
		public static readonly Guid MF_MT_ORIGINAL_4CC = new Guid(0xd7be3fe0, 0x2bc7, 0x492d, 0xb8, 0x43, 0x61, 0xa1, 0x91, 0x9b, 0x70, 0xc3);

		// {8cbbc843-9fd9-49c2-882f-a72586c408ad}   MF_MT_ORIGINAL_WAVE_FORMAT_TAG   (UINT32)
		public static readonly Guid MF_MT_ORIGINAL_WAVE_FORMAT_TAG = new Guid(0x8cbbc843, 0x9fd9, 0x49c2, 0x88, 0x2f, 0xa7, 0x25, 0x86, 0xc4, 0x08, 0xad);

		// Video Capture Media Type Attributes

		// {D2E7558C-DC1F-403f-9A72-D28BB1EB3B5E}   MF_MT_FRAME_RATE_RANGE_MIN      {UINT64 (HI32(Numerator),LO32(Denominator))}
		public static readonly Guid MF_MT_FRAME_RATE_RANGE_MIN = new Guid(0xd2e7558c, 0xdc1f, 0x403f, 0x9a, 0x72, 0xd2, 0x8b, 0xb1, 0xeb, 0x3b, 0x5e);

		// {E3371D41-B4CF-4a05-BD4E-20B88BB2C4D6}   MF_MT_FRAME_RATE_RANGE_MAX      {UINT64 (HI32(Numerator),LO32(Denominator))}
		public static readonly Guid MF_MT_FRAME_RATE_RANGE_MAX = new Guid(0xe3371d41, 0xb4cf, 0x4a05, 0xbd, 0x4e, 0x20, 0xb8, 0x8b, 0xb2, 0xc4, 0xd6);

		public static readonly Guid MF_LOW_LATENCY = new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee);

		// {E3F2E203-D445-4B8C-9211-AE390D3BA017}  {UINT32} Maximum macroblocks per second that can be handled by MFT
		public static readonly Guid MF_VIDEO_MAX_MB_PER_SEC = new Guid(0xe3f2e203, 0xd445, 0x4b8c, 0x92, 0x11, 0xae, 0x39, 0xd, 0x3b, 0xa0, 0x17);
		public static readonly Guid MF_VIDEO_PROCESSOR_ALGORITHM = new Guid(0x4a0a1e1f, 0x272c, 0x4fb6, 0x9e, 0xb1, 0xdb, 0x33, 0xc, 0xbc, 0x97, 0xca);

		public static readonly Guid MF_TOPOLOGY_DXVA_MODE = new Guid(0x1e8d34f6, 0xf5ab, 0x4e23, 0xbb, 0x88, 0x87, 0x4a, 0xa3, 0xa1, 0xa7, 0x4d);
		public static readonly Guid MF_TOPOLOGY_ENABLE_XVP_FOR_PLAYBACK = new Guid(0x1967731f, 0xcd78, 0x42fc, 0xb0, 0x26, 0x9, 0x92, 0xa5, 0x6e, 0x56, 0x93);

		public static readonly Guid MF_TOPOLOGY_STATIC_PLAYBACK_OPTIMIZATIONS = new Guid(0xb86cac42, 0x41a6, 0x4b79, 0x89, 0x7a, 0x1a, 0xb0, 0xe5, 0x2b, 0x4a, 0x1b);
		public static readonly Guid MF_TOPOLOGY_PLAYBACK_MAX_DIMS = new Guid(0x5715cf19, 0x5768, 0x44aa, 0xad, 0x6e, 0x87, 0x21, 0xf1, 0xb0, 0xf9, 0xbb);

		public static readonly Guid MF_TOPOLOGY_HARDWARE_MODE = new Guid(0xd2d362fd, 0x4e4f, 0x4191, 0xa5, 0x79, 0xc6, 0x18, 0xb6, 0x67, 0x6, 0xaf);
		public static readonly Guid MF_TOPOLOGY_PLAYBACK_FRAMERATE = new Guid(0xc164737a, 0xc2b1, 0x4553, 0x83, 0xbb, 0x5a, 0x52, 0x60, 0x72, 0x44, 0x8f);
		public static readonly Guid MF_TOPOLOGY_DYNAMIC_CHANGE_NOT_ALLOWED = new Guid(0xd529950b, 0xd484, 0x4527, 0xa9, 0xcd, 0xb1, 0x90, 0x95, 0x32, 0xb5, 0xb0);
		public static readonly Guid MF_TOPOLOGY_ENUMERATE_SOURCE_TYPES = new Guid(0x6248c36d, 0x5d0b, 0x4f40, 0xa0, 0xbb, 0xb0, 0xb3, 0x05, 0xf7, 0x76, 0x98);
		public static readonly Guid MF_TOPOLOGY_START_TIME_ON_PRESENTATION_SWITCH = new Guid(0xc8cc113f, 0x7951, 0x4548, 0xaa, 0xd6, 0x9e, 0xd6, 0x20, 0x2e, 0x62, 0xb3);

		public static readonly Guid MF_PD_PLAYBACK_ELEMENT_ID = new Guid(0x6c990d39, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_PREFERRED_LANGUAGE = new Guid(0x6c990d3A, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_PLAYBACK_BOUNDARY_TIME = new Guid(0x6c990d3b, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		public static readonly Guid MF_PD_AUDIO_ISVARIABLEBITRATE = new Guid(0x33026ee0, 0xe387, 0x4582, 0xae, 0x0a, 0x34, 0xa2, 0xad, 0x3b, 0xaa, 0x18);

		public static readonly Guid MF_SD_STREAM_NAME = new Guid(0x4f1b099d, 0xd314, 0x41e5, 0xa7, 0x81, 0x7f, 0xef, 0xaa, 0x4c, 0x50, 0x1f);
		public static readonly Guid MF_SD_MUTUALLY_EXCLUSIVE = new Guid(0x23ef79c, 0x388d, 0x487f, 0xac, 0x17, 0x69, 0x6c, 0xd6, 0xe3, 0xc6, 0xf5);

		public static readonly Guid MF_SAMPLEGRABBERSINK_IGNORE_CLOCK = new Guid(0x0efda2c0, 0x2b69, 0x4e2e, 0xab, 0x8d, 0x46, 0xdc, 0xbf, 0xf7, 0xd2, 0x5d);
		public static readonly Guid MF_BYTESTREAMHANDLER_ACCEPTS_SHARE_WRITE = new Guid(0xa6e1f733, 0x3001, 0x4915, 0x81, 0x50, 0x15, 0x58, 0xa2, 0x18, 0xe, 0xc8);

		public static readonly Guid MF_TRANSCODE_CONTAINERTYPE = new Guid(0x150ff23f, 0x4abc, 0x478b, 0xac, 0x4f, 0xe1, 0x91, 0x6f, 0xba, 0x1c, 0xca);
		public static readonly Guid MF_TRANSCODE_SKIP_METADATA_TRANSFER = new Guid(0x4e4469ef, 0xb571, 0x4959, 0x8f, 0x83, 0x3d, 0xcf, 0xba, 0x33, 0xa3, 0x93);
		public static readonly Guid MF_TRANSCODE_TOPOLOGYMODE = new Guid(0x3e3df610, 0x394a, 0x40b2, 0x9d, 0xea, 0x3b, 0xab, 0x65, 0xb, 0xeb, 0xf2);
		public static readonly Guid MF_TRANSCODE_ADJUST_PROFILE = new Guid(0x9c37c21b, 0x60f, 0x487c, 0xa6, 0x90, 0x80, 0xd7, 0xf5, 0xd, 0x1c, 0x72);

		public static readonly Guid MF_TRANSCODE_ENCODINGPROFILE = new Guid(0x6947787c, 0xf508, 0x4ea9, 0xb1, 0xe9, 0xa1, 0xfe, 0x3a, 0x49, 0xfb, 0xc9);
		public static readonly Guid MF_TRANSCODE_QUALITYVSSPEED = new Guid(0x98332df8, 0x03cd, 0x476b, 0x89, 0xfa, 0x3f, 0x9e, 0x44, 0x2d, 0xec, 0x9f);
		public static readonly Guid MF_TRANSCODE_DONOT_INSERT_ENCODER = new Guid(0xf45aa7ce, 0xab24, 0x4012, 0xa1, 0x1b, 0xdc, 0x82, 0x20, 0x20, 0x14, 0x10);

		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE = new Guid(0xc60ac5fe, 0x252a, 0x478f, 0xa0, 0xef, 0xbc, 0x8f, 0xa5, 0xf7, 0xca, 0xd3);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_HW_SOURCE = new Guid(0xde7046ba, 0x54d6, 0x4487, 0xa2, 0xa4, 0xec, 0x7c, 0xd, 0x1b, 0xd1, 0x63);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME = new Guid(0x60d0e559, 0x52f8, 0x4fa2, 0xbb, 0xce, 0xac, 0xdb, 0x34, 0xa8, 0xec, 0x1);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_MEDIA_TYPE = new Guid(0x56a819ca, 0xc78, 0x4de4, 0xa0, 0xa7, 0x3d, 0xda, 0xba, 0xf, 0x24, 0xd4);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY = new Guid(0x77f0ae69, 0xc3bd, 0x4509, 0x94, 0x1d, 0x46, 0x7e, 0x4d, 0x24, 0x89, 0x9e);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK = new Guid(0x58f0aad8, 0x22bf, 0x4f8a, 0xbb, 0x3d, 0xd2, 0xc4, 0x97, 0x8c, 0x6e, 0x2f);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_MAX_BUFFERS = new Guid(0x7dd9b730, 0x4f2d, 0x41d5, 0x8f, 0x95, 0xc, 0xc9, 0xa9, 0x12, 0xba, 0x26);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ENDPOINT_ID = new Guid(0x30da9258, 0xfeb9, 0x47a7, 0xa4, 0x53, 0x76, 0x3a, 0x7a, 0x8e, 0x1c, 0x5f);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ROLE = new Guid(0xbc9d118e, 0x8c67, 0x4a18, 0x85, 0xd4, 0x12, 0xd3, 0x0, 0x40, 0x5, 0x52);

		public static readonly Guid MFSampleExtension_DeviceTimestamp = new Guid(0x8f3e35e7, 0x2dcd, 0x4887, 0x86, 0x22, 0x2a, 0x58, 0xba, 0xa6, 0x52, 0xb0);

		public static readonly Guid MF_TRANSFORM_ASYNC = new Guid(0xf81a699a, 0x649a, 0x497d, 0x8c, 0x73, 0x29, 0xf8, 0xfe, 0xd6, 0xad, 0x7a);
		public static readonly Guid MF_TRANSFORM_ASYNC_UNLOCK = new Guid(0xe5666d6b, 0x3422, 0x4eb6, 0xa4, 0x21, 0xda, 0x7d, 0xb1, 0xf8, 0xe2, 0x7);
		public static readonly Guid MF_TRANSFORM_FLAGS_Attribute = new Guid(0x9359bb7e, 0x6275, 0x46c4, 0xa0, 0x25, 0x1c, 0x1, 0xe4, 0x5f, 0x1a, 0x86);
		public static readonly Guid MF_TRANSFORM_CATEGORY_Attribute = new Guid(0xceabba49, 0x506d, 0x4757, 0xa6, 0xff, 0x66, 0xc1, 0x84, 0x98, 0x7e, 0x4e);
		public static readonly Guid MFT_TRANSFORM_CLSID_Attribute = new Guid(0x6821c42b, 0x65a4, 0x4e82, 0x99, 0xbc, 0x9a, 0x88, 0x20, 0x5e, 0xcd, 0xc);
		public static readonly Guid MFT_INPUT_TYPES_Attributes = new Guid(0x4276c9b1, 0x759d, 0x4bf3, 0x9c, 0xd0, 0xd, 0x72, 0x3d, 0x13, 0x8f, 0x96);
		public static readonly Guid MFT_OUTPUT_TYPES_Attributes = new Guid(0x8eae8cf3, 0xa44f, 0x4306, 0xba, 0x5c, 0xbf, 0x5d, 0xda, 0x24, 0x28, 0x18);
		public static readonly Guid MFT_ENUM_HARDWARE_URL_Attribute = new Guid(0x2fb866ac, 0xb078, 0x4942, 0xab, 0x6c, 0x0, 0x3d, 0x5, 0xcd, 0xa6, 0x74);
		public static readonly Guid MFT_FRIENDLY_NAME_Attribute = new Guid(0x314ffbae, 0x5b41, 0x4c95, 0x9c, 0x19, 0x4e, 0x7d, 0x58, 0x6f, 0xac, 0xe3);
		public static readonly Guid MFT_CONNECTED_STREAM_ATTRIBUTE = new Guid(0x71eeb820, 0xa59f, 0x4de2, 0xbc, 0xec, 0x38, 0xdb, 0x1d, 0xd6, 0x11, 0xa4);
		public static readonly Guid MFT_CONNECTED_TO_HW_STREAM = new Guid(0x34e6e728, 0x6d6, 0x4491, 0xa5, 0x53, 0x47, 0x95, 0x65, 0xd, 0xb9, 0x12);
		public static readonly Guid MFT_PREFERRED_OUTPUTTYPE_Attribute = new Guid(0x7e700499, 0x396a, 0x49ee, 0xb1, 0xb4, 0xf6, 0x28, 0x2, 0x1e, 0x8c, 0x9d);
		public static readonly Guid MFT_PROCESS_LOCAL_Attribute = new Guid(0x543186e4, 0x4649, 0x4e65, 0xb5, 0x88, 0x4a, 0xa3, 0x52, 0xaf, 0xf3, 0x79);
		public static readonly Guid MFT_PREFERRED_ENCODER_PROFILE = new Guid(0x53004909, 0x1ef5, 0x46d7, 0xa1, 0x8e, 0x5a, 0x75, 0xf8, 0xb5, 0x90, 0x5f);
		public static readonly Guid MFT_HW_TIMESTAMP_WITH_QPC_Attribute = new Guid(0x8d030fb8, 0xcc43, 0x4258, 0xa2, 0x2e, 0x92, 0x10, 0xbe, 0xf8, 0x9b, 0xe4);
		public static readonly Guid MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x8ec2e9fd, 0x9148, 0x410d, 0x83, 0x1e, 0x70, 0x24, 0x39, 0x46, 0x1a, 0x8e);
		public static readonly Guid MFT_CODEC_MERIT_Attribute = new Guid(0x88a7cb15, 0x7b07, 0x4a34, 0x91, 0x28, 0xe6, 0x4c, 0x67, 0x3, 0xc4, 0xd3);
		public static readonly Guid MFT_ENUM_TRANSCODE_ONLY_ATTRIBUTE = new Guid(0x111ea8cd, 0xb62a, 0x4bdb, 0x89, 0xf6, 0x67, 0xff, 0xcd, 0xc2, 0x45, 0x8b);

		public static readonly Guid MF_MP2DLNA_USE_MMCSS = new Guid(0x54f3e2ee, 0xa2a2, 0x497d, 0x98, 0x34, 0x97, 0x3a, 0xfd, 0xe5, 0x21, 0xeb);
		public static readonly Guid MF_MP2DLNA_VIDEO_BIT_RATE = new Guid(0xe88548de, 0x73b4, 0x42d7, 0x9c, 0x75, 0xad, 0xfa, 0xa, 0x2a, 0x6e, 0x4c);
		public static readonly Guid MF_MP2DLNA_AUDIO_BIT_RATE = new Guid(0x2d1c070e, 0x2b5f, 0x4ab3, 0xa7, 0xe6, 0x8d, 0x94, 0x3b, 0xa8, 0xd0, 0x0a);
		public static readonly Guid MF_MP2DLNA_ENCODE_QUALITY = new Guid(0xb52379d7, 0x1d46, 0x4fb6, 0xa3, 0x17, 0xa4, 0xa5, 0xf6, 0x09, 0x59, 0xf8);
		public static readonly Guid MF_MP2DLNA_STATISTICS = new Guid(0x75e488a3, 0xd5ad, 0x4898, 0x85, 0xe0, 0xbc, 0xce, 0x24, 0xa7, 0x22, 0xd7);

		public static readonly Guid MF_SINK_WRITER_ASYNC_CALLBACK = new Guid(0x48cb183e, 0x7b0b, 0x46f4, 0x82, 0x2e, 0x5e, 0x1d, 0x2d, 0xda, 0x43, 0x54);
		public static readonly Guid MF_SINK_WRITER_DISABLE_THROTTLING = new Guid(0x08b845d8, 0x2b74, 0x4afe, 0x9d, 0x53, 0xbe, 0x16, 0xd2, 0xd5, 0xae, 0x4f);
		public static readonly Guid MF_SINK_WRITER_D3D_MANAGER = new Guid(0xec822da2, 0xe1e9, 0x4b29, 0xa0, 0xd8, 0x56, 0x3c, 0x71, 0x9f, 0x52, 0x69);
		public static readonly Guid MF_SINK_WRITER_ENCODER_CONFIG = new Guid(0xad91cd04, 0xa7cc, 0x4ac7, 0x99, 0xb6, 0xa5, 0x7b, 0x9a, 0x4a, 0x7c, 0x70);
		public static readonly Guid MF_READWRITE_DISABLE_CONVERTERS = new Guid(0x98d5b065, 0x1374, 0x4847, 0x8d, 0x5d, 0x31, 0x52, 0x0f, 0xee, 0x71, 0x56);
		public static readonly Guid MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS = new Guid(0xa634a91c, 0x822b, 0x41b9, 0xa4, 0x94, 0x4d, 0xe4, 0x64, 0x36, 0x12, 0xb0);
		public static readonly Guid MF_READWRITE_MMCSS_CLASS = new Guid(0x39384300, 0xd0eb, 0x40b1, 0x87, 0xa0, 0x33, 0x18, 0x87, 0x1b, 0x5a, 0x53);
		public static readonly Guid MF_READWRITE_MMCSS_PRIORITY = new Guid(0x43ad19ce, 0xf33f, 0x4ba9, 0xa5, 0x80, 0xe4, 0xcd, 0x12, 0xf2, 0xd1, 0x44);
		public static readonly Guid MF_READWRITE_MMCSS_CLASS_AUDIO = new Guid(0x430847da, 0x0890, 0x4b0e, 0x93, 0x8c, 0x05, 0x43, 0x32, 0xc5, 0x47, 0xe1);
		public static readonly Guid MF_READWRITE_MMCSS_PRIORITY_AUDIO = new Guid(0x273db885, 0x2de2, 0x4db2, 0xa6, 0xa7, 0xfd, 0xb6, 0x6f, 0xb4, 0x0b, 0x61);
		public static readonly Guid MF_READWRITE_D3D_OPTIONAL = new Guid(0x216479d9, 0x3071, 0x42ca, 0xbb, 0x6c, 0x4c, 0x22, 0x10, 0x2e, 0x1d, 0x18);

		public static readonly Guid MF_SOURCE_READER_ASYNC_CALLBACK = new Guid(0x1e3dbeac, 0xbb43, 0x4c35, 0xb5, 0x07, 0xcd, 0x64, 0x44, 0x64, 0xc9, 0x65);
		public static readonly Guid MF_SOURCE_READER_D3D_MANAGER = new Guid(0xec822da2, 0xe1e9, 0x4b29, 0xa0, 0xd8, 0x56, 0x3c, 0x71, 0x9f, 0x52, 0x69);
		public static readonly Guid MF_SOURCE_READER_DISABLE_DXVA = new Guid(0xaa456cfd, 0x3943, 0x4a1e, 0xa7, 0x7d, 0x18, 0x38, 0xc0, 0xea, 0x2e, 0x35);
		public static readonly Guid MF_SOURCE_READER_MEDIASOURCE_CONFIG = new Guid(0x9085abeb, 0x0354, 0x48f9, 0xab, 0xb5, 0x20, 0x0d, 0xf8, 0x38, 0xc6, 0x8e);
		public static readonly Guid MF_SOURCE_READER_MEDIASOURCE_CHARACTERISTICS = new Guid(0x6d23f5c8, 0xc5d7, 0x4a9b, 0x99, 0x71, 0x5d, 0x11, 0xf8, 0xbc, 0xa8, 0x80);
		public static readonly Guid MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING = new Guid(0xfb394f3d, 0xccf1, 0x42ee, 0xbb, 0xb3, 0xf9, 0xb8, 0x45, 0xd5, 0x68, 0x1d);
		public static readonly Guid MF_SOURCE_READER_DISCONNECT_MEDIASOURCE_ON_SHUTDOWN = new Guid(0x56b67165, 0x219e, 0x456d, 0xa2, 0x2e, 0x2d, 0x30, 0x04, 0xc7, 0xfe, 0x56);
		public static readonly Guid MF_SOURCE_READER_ENABLE_ADVANCED_VIDEO_PROCESSING = new Guid(0xf81da2c, 0xb537, 0x4672, 0xa8, 0xb2, 0xa6, 0x81, 0xb1, 0x73, 0x7, 0xa3);
		public static readonly Guid MF_SOURCE_READER_DISABLE_CAMERA_PLUGINS = new Guid(0x9d3365dd, 0x58f, 0x4cfb, 0x9f, 0x97, 0xb3, 0x14, 0xcc, 0x99, 0xc8, 0xad);
		public static readonly Guid MF_SOURCE_READER_ENABLE_TRANSCODE_ONLY_TRANSFORMS = new Guid(0xdfd4f008, 0xb5fd, 0x4e78, 0xae, 0x44, 0x62, 0xa1, 0xe6, 0x7b, 0xbe, 0x27);

		// codecapi.h
		public static readonly Guid CODECAPI_AVEncCommonFormatConstraint = new Guid(0x57cbb9b8, 0x116f, 0x4951, 0xb4, 0x0c, 0xc2, 0xa0, 0x35, 0xed, 0x8f, 0x17);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatUnSpecified = new Guid(0xaf46a35a, 0x6024, 0x4525, 0xa4, 0x8a, 0x09, 0x4b, 0x97, 0xf5, 0xb3, 0xc2);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatDVD_V = new Guid(0xcc9598c4, 0xe7fe, 0x451d, 0xb1, 0xca, 0x76, 0x1b, 0xc8, 0x40, 0xb7, 0xf3);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatDVD_DashVR = new Guid(0xe55199d6, 0x044c, 0x4dae, 0xa4, 0x88, 0x53, 0x1e, 0xd3, 0x06, 0x23, 0x5b);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatDVD_PlusVR = new Guid(0xe74c6f2e, 0xec37, 0x478d, 0x9a, 0xf4, 0xa5, 0xe1, 0x35, 0xb6, 0x27, 0x1c);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatVCD = new Guid(0x95035bf7, 0x9d90, 0x40ff, 0xad, 0x5c, 0x5c, 0xf8, 0xcf, 0x71, 0xca, 0x1d);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatSVCD = new Guid(0x51d85818, 0x8220, 0x448c, 0x80, 0x66, 0xd6, 0x9b, 0xed, 0x16, 0xc9, 0xad);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatATSC = new Guid(0x8d7b897c, 0xa019, 0x4670, 0xaa, 0x76, 0x2e, 0xdc, 0xac, 0x7a, 0xc2, 0x96);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatDVB = new Guid(0x71830d8f, 0x6c33, 0x430d, 0x84, 0x4b, 0xc2, 0x70, 0x5b, 0xaa, 0xe6, 0xdb);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatMP3 = new Guid(0x349733cd, 0xeb08, 0x4dc2, 0x81, 0x97, 0xe4, 0x98, 0x35, 0xef, 0x82, 0x8b);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatHighMAT = new Guid(0x1eabe760, 0xfb2b, 0x4928, 0x90, 0xd1, 0x78, 0xdb, 0x88, 0xee, 0xe8, 0x89);
		public static readonly Guid CODECAPI_GUID_AVEncCommonFormatHighMPV = new Guid(0xa2d25db8, 0xb8f9, 0x42c2, 0x8b, 0xc7, 0x0b, 0x93, 0xcf, 0x60, 0x47, 0x88);
		public static readonly Guid CODECAPI_AVEncCodecType = new Guid(0x08af4ac1, 0xf3f2, 0x4c74, 0x9d, 0xcf, 0x37, 0xf2, 0xec, 0x79, 0xf8, 0x26);
		public static readonly Guid CODECAPI_GUID_AVEncMPEG1Video = new Guid(0xc8dafefe, 0xda1e, 0x4774, 0xb2, 0x7d, 0x11, 0x83, 0x0c, 0x16, 0xb1, 0xfe);
		public static readonly Guid CODECAPI_GUID_AVEncMPEG2Video = new Guid(0x046dc19a, 0x6677, 0x4aaa, 0xa3, 0x1d, 0xc1, 0xab, 0x71, 0x6f, 0x45, 0x60);
		public static readonly Guid CODECAPI_GUID_AVEncMPEG1Audio = new Guid(0xd4dd1362, 0xcd4a, 0x4cd6, 0x81, 0x38, 0xb9, 0x4d, 0xb4, 0x54, 0x2b, 0x04);
		public static readonly Guid CODECAPI_GUID_AVEncMPEG2Audio = new Guid(0xee4cbb1f, 0x9c3f, 0x4770, 0x92, 0xb5, 0xfc, 0xb7, 0xc2, 0xa8, 0xd3, 0x81);
		public static readonly Guid CODECAPI_GUID_AVEncWMV = new Guid(0x4e0fef9b, 0x1d43, 0x41bd, 0xb8, 0xbd, 0x4d, 0x7b, 0xf7, 0x45, 0x7a, 0x2a);
		public static readonly Guid CODECAPI_GUID_AVEndMPEG4Video = new Guid(0xdd37b12a, 0x9503, 0x4f8b, 0xb8, 0xd0, 0x32, 0x4a, 0x00, 0xc0, 0xa1, 0xcf);
		public static readonly Guid CODECAPI_GUID_AVEncH264Video = new Guid(0x95044eab, 0x31b3, 0x47de, 0x8e, 0x75, 0x38, 0xa4, 0x2b, 0xb0, 0x3e, 0x28);
		public static readonly Guid CODECAPI_GUID_AVEncDV = new Guid(0x09b769c7, 0x3329, 0x44fb, 0x89, 0x54, 0xfa, 0x30, 0x93, 0x7d, 0x3d, 0x5a);
		public static readonly Guid CODECAPI_GUID_AVEncWMAPro = new Guid(0x1955f90c, 0x33f7, 0x4a68, 0xab, 0x81, 0x53, 0xf5, 0x65, 0x71, 0x25, 0xc4);
		public static readonly Guid CODECAPI_GUID_AVEncWMALossless = new Guid(0x55ca7265, 0x23d8, 0x4761, 0x90, 0x31, 0xb7, 0x4f, 0xbe, 0x12, 0xf4, 0xc1);
		public static readonly Guid CODECAPI_GUID_AVEncWMAVoice = new Guid(0x13ed18cb, 0x50e8, 0x4276, 0xa2, 0x88, 0xa6, 0xaa, 0x22, 0x83, 0x82, 0xd9);
		public static readonly Guid CODECAPI_GUID_AVEncDolbyDigitalPro = new Guid(0xf5be76cc, 0x0ff8, 0x40eb, 0x9c, 0xb1, 0xbb, 0xa9, 0x40, 0x04, 0xd4, 0x4f);
		public static readonly Guid CODECAPI_GUID_AVEncDolbyDigitalConsumer = new Guid(0xc1a7bf6c, 0x0059, 0x4bfa, 0x94, 0xef, 0xef, 0x74, 0x7a, 0x76, 0x8d, 0x52);
		public static readonly Guid CODECAPI_GUID_AVEncDolbyDigitalPlus = new Guid(0x698d1b80, 0xf7dd, 0x415c, 0x97, 0x1c, 0x42, 0x49, 0x2a, 0x20, 0x56, 0xc6);
		public static readonly Guid CODECAPI_GUID_AVEncDTSHD = new Guid(0x2052e630, 0x469d, 0x4bfb, 0x80, 0xca, 0x1d, 0x65, 0x6e, 0x7e, 0x91, 0x8f);
		public static readonly Guid CODECAPI_GUID_AVEncDTS = new Guid(0x45fbcaa2, 0x5e6e, 0x4ab0, 0x88, 0x93, 0x59, 0x03, 0xbe, 0xe9, 0x3a, 0xcf);
		public static readonly Guid CODECAPI_GUID_AVEncMLP = new Guid(0x05f73e29, 0xf0d1, 0x431e, 0xa4, 0x1c, 0xa4, 0x74, 0x32, 0xec, 0x5a, 0x66);
		public static readonly Guid CODECAPI_GUID_AVEncPCM = new Guid(0x844be7f4, 0x26cf, 0x4779, 0xb3, 0x86, 0xcc, 0x05, 0xd1, 0x87, 0x99, 0x0c);
		public static readonly Guid CODECAPI_GUID_AVEncSDDS = new Guid(0x1dc1b82f, 0x11c8, 0x4c71, 0xb7, 0xb6, 0xee, 0x3e, 0xb9, 0xbc, 0x2b, 0x94);
		public static readonly Guid CODECAPI_AVEncCommonRateControlMode = new Guid(0x1c0608e9, 0x370c, 0x4710, 0x8a, 0x58, 0xcb, 0x61, 0x81, 0xc4, 0x24, 0x23);
		public static readonly Guid CODECAPI_AVEncCommonLowLatency = new Guid(0x9d3ecd55, 0x89e8, 0x490a, 0x97, 0x0a, 0x0c, 0x95, 0x48, 0xd5, 0xa5, 0x6e);
		public static readonly Guid CODECAPI_AVEncCommonMultipassMode = new Guid(0x22533d4c, 0x47e1, 0x41b5, 0x93, 0x52, 0xa2, 0xb7, 0x78, 0x0e, 0x7a, 0xc4);
		public static readonly Guid CODECAPI_AVEncCommonPassStart = new Guid(0x6a67739f, 0x4eb5, 0x4385, 0x99, 0x28, 0xf2, 0x76, 0xa9, 0x39, 0xef, 0x95);
		public static readonly Guid CODECAPI_AVEncCommonPassEnd = new Guid(0x0e3d01bc, 0xc85c, 0x467d, 0x8b, 0x60, 0xc4, 0x10, 0x12, 0xee, 0x3b, 0xf6);
		public static readonly Guid CODECAPI_AVEncCommonRealTime = new Guid(0x143a0ff6, 0xa131, 0x43da, 0xb8, 0x1e, 0x98, 0xfb, 0xb8, 0xec, 0x37, 0x8e);
		public static readonly Guid CODECAPI_AVEncCommonQuality = new Guid(0xfcbf57a3, 0x7ea5, 0x4b0c, 0x96, 0x44, 0x69, 0xb4, 0x0c, 0x39, 0xc3, 0x91);
		public static readonly Guid CODECAPI_AVEncCommonQualityVsSpeed = new Guid(0x98332df8, 0x03cd, 0x476b, 0x89, 0xfa, 0x3f, 0x9e, 0x44, 0x2d, 0xec, 0x9f);
		public static readonly Guid CODECAPI_AVEncCommonTranscodeEncodingProfile = new Guid(0x6947787C, 0xF508, 0x4EA9, 0xB1, 0xE9, 0xA1, 0xFE, 0x3A, 0x49, 0xFB, 0xC9);
		public static readonly Guid CODECAPI_AVEncCommonMeanBitRate = new Guid(0xf7222374, 0x2144, 0x4815, 0xb5, 0x50, 0xa3, 0x7f, 0x8e, 0x12, 0xee, 0x52);
		public static readonly Guid CODECAPI_AVEncCommonMeanBitRateInterval = new Guid(0xbfaa2f0c, 0xcb82, 0x4bc0, 0x84, 0x74, 0xf0, 0x6a, 0x8a, 0x0d, 0x02, 0x58);
		public static readonly Guid CODECAPI_AVEncCommonMaxBitRate = new Guid(0x9651eae4, 0x39b9, 0x4ebf, 0x85, 0xef, 0xd7, 0xf4, 0x44, 0xec, 0x74, 0x65);
		public static readonly Guid CODECAPI_AVEncCommonMinBitRate = new Guid(0x101405b2, 0x2083, 0x4034, 0xa8, 0x06, 0xef, 0xbe, 0xdd, 0xd7, 0xc9, 0xff);
		public static readonly Guid CODECAPI_AVEncCommonBufferSize = new Guid(0x0db96574, 0xb6a4, 0x4c8b, 0x81, 0x06, 0x37, 0x73, 0xde, 0x03, 0x10, 0xcd);
		public static readonly Guid CODECAPI_AVEncCommonBufferInLevel = new Guid(0xd9c5c8db, 0xfc74, 0x4064, 0x94, 0xe9, 0xcd, 0x19, 0xf9, 0x47, 0xed, 0x45);
		public static readonly Guid CODECAPI_AVEncCommonBufferOutLevel = new Guid(0xccae7f49, 0xd0bc, 0x4e3d, 0xa5, 0x7e, 0xfb, 0x57, 0x40, 0x14, 0x00, 0x69);
		public static readonly Guid CODECAPI_AVEncCommonStreamEndHandling = new Guid(0x6aad30af, 0x6ba8, 0x4ccc, 0x8f, 0xca, 0x18, 0xd1, 0x9b, 0xea, 0xeb, 0x1c);
		public static readonly Guid CODECAPI_AVEncStatCommonCompletedPasses = new Guid(0x3e5de533, 0x9df7, 0x438c, 0x85, 0x4f, 0x9f, 0x7d, 0xd3, 0x68, 0x3d, 0x34);
		public static readonly Guid CODECAPI_AVEncVideoOutputFrameRate = new Guid(0xea85e7c3, 0x9567, 0x4d99, 0x87, 0xc4, 0x02, 0xc1, 0xc2, 0x78, 0xca, 0x7c);
		public static readonly Guid CODECAPI_AVEncVideoOutputFrameRateConversion = new Guid(0x8c068bf4, 0x369a, 0x4ba3, 0x82, 0xfd, 0xb2, 0x51, 0x8f, 0xb3, 0x39, 0x6e);
		public static readonly Guid CODECAPI_AVEncVideoPixelAspectRatio = new Guid(0x3cdc718f, 0xb3e9, 0x4eb6, 0xa5, 0x7f, 0xcf, 0x1f, 0x1b, 0x32, 0x1b, 0x87);
		public static readonly Guid CODECAPI_AVEncVideoForceSourceScanType = new Guid(0x1ef2065f, 0x058a, 0x4765, 0xa4, 0xfc, 0x8a, 0x86, 0x4c, 0x10, 0x30, 0x12);
		public static readonly Guid CODECAPI_AVEncVideoNoOfFieldsToEncode = new Guid(0x61e4bbe2, 0x4ee0, 0x40e7, 0x80, 0xab, 0x51, 0xdd, 0xee, 0xbe, 0x62, 0x91);
		public static readonly Guid CODECAPI_AVEncVideoNoOfFieldsToSkip = new Guid(0xa97e1240, 0x1427, 0x4c16, 0xa7, 0xf7, 0x3d, 0xcf, 0xd8, 0xba, 0x4c, 0xc5);
		public static readonly Guid CODECAPI_AVEncVideoEncodeDimension = new Guid(0x1074df28, 0x7e0f, 0x47a4, 0xa4, 0x53, 0xcd, 0xd7, 0x38, 0x70, 0xf5, 0xce);
		public static readonly Guid CODECAPI_AVEncVideoEncodeOffsetOrigin = new Guid(0x6bc098fe, 0xa71a, 0x4454, 0x85, 0x2e, 0x4d, 0x2d, 0xde, 0xb2, 0xcd, 0x24);
		public static readonly Guid CODECAPI_AVEncVideoDisplayDimension = new Guid(0xde053668, 0xf4ec, 0x47a9, 0x86, 0xd0, 0x83, 0x67, 0x70, 0xf0, 0xc1, 0xd5);
		public static readonly Guid CODECAPI_AVEncVideoOutputScanType = new Guid(0x460b5576, 0x842e, 0x49ab, 0xa6, 0x2d, 0xb3, 0x6f, 0x73, 0x12, 0xc9, 0xdb);
		public static readonly Guid CODECAPI_AVEncVideoInverseTelecineEnable = new Guid(0x2ea9098b, 0xe76d, 0x4ccd, 0xa0, 0x30, 0xd3, 0xb8, 0x89, 0xc1, 0xb6, 0x4c);
		public static readonly Guid CODECAPI_AVEncVideoInverseTelecineThreshold = new Guid(0x40247d84, 0xe895, 0x497f, 0xb4, 0x4c, 0xb7, 0x45, 0x60, 0xac, 0xfe, 0x27);
		public static readonly Guid CODECAPI_AVEncVideoSourceFilmContent = new Guid(0x1791c64b, 0xccfc, 0x4827, 0xa0, 0xed, 0x25, 0x57, 0x79, 0x3b, 0x2b, 0x1c);
		public static readonly Guid CODECAPI_AVEncVideoSourceIsBW = new Guid(0x42ffc49b, 0x1812, 0x4fdc, 0x8d, 0x24, 0x70, 0x54, 0xc5, 0x21, 0xe6, 0xeb);
		public static readonly Guid CODECAPI_AVEncVideoFieldSwap = new Guid(0xfefd7569, 0x4e0a, 0x49f2, 0x9f, 0x2b, 0x36, 0x0e, 0xa4, 0x8c, 0x19, 0xa2);
		public static readonly Guid CODECAPI_AVEncVideoInputChromaResolution = new Guid(0xbb0cec33, 0x16f1, 0x47b0, 0x8a, 0x88, 0x37, 0x81, 0x5b, 0xee, 0x17, 0x39);
		public static readonly Guid CODECAPI_AVEncVideoOutputChromaResolution = new Guid(0x6097b4c9, 0x7c1d, 0x4e64, 0xbf, 0xcc, 0x9e, 0x97, 0x65, 0x31, 0x8a, 0xe7);
		public static readonly Guid CODECAPI_AVEncVideoInputChromaSubsampling = new Guid(0xa8e73a39, 0x4435, 0x4ec3, 0xa6, 0xea, 0x98, 0x30, 0x0f, 0x4b, 0x36, 0xf7);
		public static readonly Guid CODECAPI_AVEncVideoOutputChromaSubsampling = new Guid(0xfa561c6c, 0x7d17, 0x44f0, 0x83, 0xc9, 0x32, 0xed, 0x12, 0xe9, 0x63, 0x43);
		public static readonly Guid CODECAPI_AVEncVideoInputColorPrimaries = new Guid(0xc24d783f, 0x7ce6, 0x4278, 0x90, 0xab, 0x28, 0xa4, 0xf1, 0xe5, 0xf8, 0x6c);
		public static readonly Guid CODECAPI_AVEncVideoOutputColorPrimaries = new Guid(0xbe95907c, 0x9d04, 0x4921, 0x89, 0x85, 0xa6, 0xd6, 0xd8, 0x7d, 0x1a, 0x6c);
		public static readonly Guid CODECAPI_AVEncVideoInputColorTransferFunction = new Guid(0x8c056111, 0xa9c3, 0x4b08, 0xa0, 0xa0, 0xce, 0x13, 0xf8, 0xa2, 0x7c, 0x75);
		public static readonly Guid CODECAPI_AVEncVideoOutputColorTransferFunction = new Guid(0x4a7f884a, 0xea11, 0x460d, 0xbf, 0x57, 0xb8, 0x8b, 0xc7, 0x59, 0x00, 0xde);
		public static readonly Guid CODECAPI_AVEncVideoInputColorTransferMatrix = new Guid(0x52ed68b9, 0x72d5, 0x4089, 0x95, 0x8d, 0xf5, 0x40, 0x5d, 0x55, 0x08, 0x1c);
		public static readonly Guid CODECAPI_AVEncVideoOutputColorTransferMatrix = new Guid(0xa9b90444, 0xaf40, 0x4310, 0x8f, 0xbe, 0xed, 0x6d, 0x93, 0x3f, 0x89, 0x2b);
		public static readonly Guid CODECAPI_AVEncVideoInputColorLighting = new Guid(0x46a99549, 0x0015, 0x4a45, 0x9c, 0x30, 0x1d, 0x5c, 0xfa, 0x25, 0x83, 0x16);
		public static readonly Guid CODECAPI_AVEncVideoOutputColorLighting = new Guid(0x0e5aaac6, 0xace6, 0x4c5c, 0x99, 0x8e, 0x1a, 0x8c, 0x9c, 0x6c, 0x0f, 0x89);
		public static readonly Guid CODECAPI_AVEncVideoInputColorNominalRange = new Guid(0x16cf25c6, 0xa2a6, 0x48e9, 0xae, 0x80, 0x21, 0xae, 0xc4, 0x1d, 0x42, 0x7e);
		public static readonly Guid CODECAPI_AVEncVideoOutputColorNominalRange = new Guid(0x972835ed, 0x87b5, 0x4e95, 0x95, 0x00, 0xc7, 0x39, 0x58, 0x56, 0x6e, 0x54);
		public static readonly Guid CODECAPI_AVEncInputVideoSystem = new Guid(0xbede146d, 0xb616, 0x4dc7, 0x92, 0xb2, 0xf5, 0xd9, 0xfa, 0x92, 0x98, 0xf7);
		public static readonly Guid CODECAPI_AVEncVideoHeaderDropFrame = new Guid(0x6ed9e124, 0x7925, 0x43fe, 0x97, 0x1b, 0xe0, 0x19, 0xf6, 0x22, 0x22, 0xb4);
		public static readonly Guid CODECAPI_AVEncVideoHeaderHours = new Guid(0x2acc7702, 0xe2da, 0x4158, 0xbf, 0x9b, 0x88, 0x88, 0x01, 0x29, 0xd7, 0x40);
		public static readonly Guid CODECAPI_AVEncVideoHeaderMinutes = new Guid(0xdc1a99ce, 0x0307, 0x408b, 0x88, 0x0b, 0xb8, 0x34, 0x8e, 0xe8, 0xca, 0x7f);
		public static readonly Guid CODECAPI_AVEncVideoHeaderSeconds = new Guid(0x4a2e1a05, 0xa780, 0x4f58, 0x81, 0x20, 0x9a, 0x44, 0x9d, 0x69, 0x65, 0x6b);
		public static readonly Guid CODECAPI_AVEncVideoHeaderFrames = new Guid(0xafd5f567, 0x5c1b, 0x4adc, 0xbd, 0xaf, 0x73, 0x56, 0x10, 0x38, 0x14, 0x36);
		public static readonly Guid CODECAPI_AVEncVideoDefaultUpperFieldDominant = new Guid(0x810167c4, 0x0bc1, 0x47ca, 0x8f, 0xc2, 0x57, 0x05, 0x5a, 0x14, 0x74, 0xa5);
		public static readonly Guid CODECAPI_AVEncVideoCBRMotionTradeoff = new Guid(0x0d49451e, 0x18d5, 0x4367, 0xa4, 0xef, 0x32, 0x40, 0xdf, 0x16, 0x93, 0xc4);
		public static readonly Guid CODECAPI_AVEncVideoCodedVideoAccessUnitSize = new Guid(0xb4b10c15, 0x14a7, 0x4ce8, 0xb1, 0x73, 0xdc, 0x90, 0xa0, 0xb4, 0xfc, 0xdb);
		public static readonly Guid CODECAPI_AVEncVideoMaxKeyframeDistance = new Guid(0x2987123a, 0xba93, 0x4704, 0xb4, 0x89, 0xec, 0x1e, 0x5f, 0x25, 0x29, 0x2c);
		public static readonly Guid CODECAPI_AVEncH264CABACEnable = new Guid(0xee6cad62, 0xd305, 0x4248, 0xa5, 0xe, 0xe1, 0xb2, 0x55, 0xf7, 0xca, 0xf8);
		public static readonly Guid CODECAPI_AVEncVideoContentType = new Guid(0x66117aca, 0xeb77, 0x459d, 0x93, 0xc, 0xa4, 0x8d, 0x9d, 0x6, 0x83, 0xfc);
		public static readonly Guid CODECAPI_AVEncNumWorkerThreads = new Guid(0xb0c8bf60, 0x16f7, 0x4951, 0xa3, 0xb, 0x1d, 0xb1, 0x60, 0x92, 0x93, 0xd6);
		public static readonly Guid CODECAPI_AVEncVideoEncodeQP = new Guid(0x2cb5696b, 0x23fb, 0x4ce1, 0xa0, 0xf9, 0xef, 0x5b, 0x90, 0xfd, 0x55, 0xca);
		public static readonly Guid CODECAPI_AVEncVideoMinQP = new Guid(0x0ee22c6a, 0xa37c, 0x4568, 0xb5, 0xf1, 0x9d, 0x4c, 0x2b, 0x3a, 0xb8, 0x86);
		public static readonly Guid CODECAPI_AVEncVideoForceKeyFrame = new Guid(0x398c1b98, 0x8353, 0x475a, 0x9e, 0xf2, 0x8f, 0x26, 0x5d, 0x26, 0x3, 0x45);
		public static readonly Guid CODECAPI_AVEncH264SPSID = new Guid(0x50f38f51, 0x2b79, 0x40e3, 0xb3, 0x9c, 0x7e, 0x9f, 0xa0, 0x77, 0x5, 0x1);
		public static readonly Guid CODECAPI_AVEncH264PPSID = new Guid(0xbfe29ec2, 0x56c, 0x4d68, 0xa3, 0x8d, 0xae, 0x59, 0x44, 0xc8, 0x58, 0x2e);
		public static readonly Guid CODECAPI_AVEncAdaptiveMode = new Guid(0x4419b185, 0xda1f, 0x4f53, 0xbc, 0x76, 0x9, 0x7d, 0xc, 0x1e, 0xfb, 0x1e);
		public static readonly Guid CODECAPI_AVEncVideoTemporalLayerCount = new Guid(0x19caebff, 0xb74d, 0x4cfd, 0x8c, 0x27, 0xc2, 0xf9, 0xd9, 0x7d, 0x5f, 0x52);
		public static readonly Guid CODECAPI_AVEncVideoUsage = new Guid(0x1f636849, 0x5dc1, 0x49f1, 0xb1, 0xd8, 0xce, 0x3c, 0xf6, 0x2e, 0xa3, 0x85);
		public static readonly Guid CODECAPI_AVEncVideoSelectLayer = new Guid(0xeb1084f5, 0x6aaa, 0x4914, 0xbb, 0x2f, 0x61, 0x47, 0x22, 0x7f, 0x12, 0xe7);
		public static readonly Guid CODECAPI_AVEncVideoRateControlParams = new Guid(0x87d43767, 0x7645, 0x44ec, 0xb4, 0x38, 0xd3, 0x32, 0x2f, 0xbc, 0xa2, 0x9f);
		public static readonly Guid CODECAPI_AVEncVideoSupportedControls = new Guid(0xd3f40fdd, 0x77b9, 0x473d, 0x81, 0x96, 0x6, 0x12, 0x59, 0xe6, 0x9c, 0xff);
		public static readonly Guid CODECAPI_AVEncVideoEncodeFrameTypeQP = new Guid(0xaa70b610, 0xe03f, 0x450c, 0xad, 0x07, 0x07, 0x31, 0x4e, 0x63, 0x9c, 0xe7);
		public static readonly Guid CODECAPI_AVEncSliceControlMode = new Guid(0xe9e782ef, 0x5f18, 0x44c9, 0xa9, 0x0b, 0xe9, 0xc3, 0xc2, 0xc1, 0x7b, 0x0b);
		public static readonly Guid CODECAPI_AVEncSliceControlSize = new Guid(0x92f51df3, 0x07a5, 0x4172, 0xae, 0xfe, 0xc6, 0x9c, 0xa3, 0xb6, 0x0e, 0x35);
		public static readonly Guid CODECAPI_AVEncVideoMaxNumRefFrame = new Guid(0x964829ed, 0x94f9, 0x43b4, 0xb7, 0x4d, 0xef, 0x40, 0x94, 0x4b, 0x69, 0xa0);
		public static readonly Guid CODECAPI_AVEncVideoMeanAbsoluteDifference = new Guid(0xe5c0c10f, 0x81a4, 0x422d, 0x8c, 0x3f, 0xb4, 0x74, 0xa4, 0x58, 0x13, 0x36);
		public static readonly Guid CODECAPI_AVEncVideoMaxQP = new Guid(0x3daf6f66, 0xa6a7, 0x45e0, 0xa8, 0xe5, 0xf2, 0x74, 0x3f, 0x46, 0xa3, 0xa2);
		public static readonly Guid CODECAPI_AVEncVideoLTRBufferControl = new Guid(0xa4a0e93d, 0x4cbc, 0x444c, 0x89, 0xf4, 0x82, 0x6d, 0x31, 0x0e, 0x92, 0xa7);
		public static readonly Guid CODECAPI_AVEncVideoMarkLTRFrame = new Guid(0xe42f4748, 0xa06d, 0x4ef9, 0x8c, 0xea, 0x3d, 0x05, 0xfd, 0xe3, 0xbd, 0x3b);
		public static readonly Guid CODECAPI_AVEncVideoUseLTRFrame = new Guid(0x00752db8, 0x55f7, 0x4f80, 0x89, 0x5b, 0x27, 0x63, 0x91, 0x95, 0xf2, 0xad);
		public static readonly Guid CODECAPI_AVEncVideoROIEnabled = new Guid(0xd74f7f18, 0x44dd, 0x4b85, 0xab, 0xa3, 0x5, 0xd9, 0xf4, 0x2a, 0x82, 0x80);
		public static readonly Guid CODECAPI_AVScenarioInfo = new Guid(0xb28a6e64, 0x3ff9, 0x446a, 0x8a, 0x4b, 0x0d, 0x7a, 0x53, 0x41, 0x32, 0x36);
		public static readonly Guid CODECAPI_AVEncMPVGOPSizeMin = new Guid(0x7155cf20, 0xd440, 0x4852, 0xad, 0x0f, 0x9c, 0x4a, 0xbf, 0xe3, 0x7a, 0x6a);
		public static readonly Guid CODECAPI_AVEncMPVGOPSizeMax = new Guid(0xfe7de4c4, 0x1936, 0x4fe2, 0xbd, 0xf7, 0x1f, 0x18, 0xca, 0x1d, 0x00, 0x1f);
		public static readonly Guid CODECAPI_AVEncVideoMaxCTBSize = new Guid(0x822363ff, 0xcec8, 0x43e5, 0x92, 0xfd, 0xe0, 0x97, 0x48, 0x84, 0x85, 0xe9);
		public static readonly Guid CODECAPI_AVEncVideoCTBSize = new Guid(0xd47db8b2, 0xe73b, 0x4cb9, 0x8c, 0x3e, 0xbd, 0x87, 0x7d, 0x06, 0xd7, 0x7b);
		public static readonly Guid CODECAPI_VideoEncoderDisplayContentType = new Guid(0x79b90b27, 0xf4b1, 0x42dc, 0x9d, 0xd7, 0xcd, 0xaf, 0x81, 0x35, 0xc4, 0x00);
		public static readonly Guid CODECAPI_AVEncEnableVideoProcessing = new Guid(0x006f4bf6, 0x0ea3, 0x4d42, 0x87, 0x02, 0xb5, 0xd8, 0xbe, 0x0f, 0x7a, 0x92);
		public static readonly Guid CODECAPI_AVEncVideoGradualIntraRefresh = new Guid(0x8f347dee, 0xcb0d, 0x49ba, 0xb4, 0x62, 0xdb, 0x69, 0x27, 0xee, 0x21, 0x01);
		public static readonly Guid CODECAPI_GetOPMContext = new Guid(0x2f036c05, 0x4c14, 0x4689, 0x88, 0x39, 0x29, 0x4c, 0x6d, 0x73, 0xe0, 0x53);
		public static readonly Guid CODECAPI_SetHDCPManagerContext = new Guid(0x6d2d1fc8, 0x3dc9, 0x47eb, 0xa1, 0xa2, 0x47, 0x1c, 0x80, 0xcd, 0x60, 0xd0);
		public static readonly Guid CODECAPI_AVEncVideoMaxTemporalLayers = new Guid(0x9c668cfe, 0x08e1, 0x424a, 0x93, 0x4e, 0xb7, 0x64, 0xb0, 0x64, 0x80, 0x2a);
		public static readonly Guid CODECAPI_AVEncVideoNumGOPsPerIDR = new Guid(0x83bc5bdb, 0x5b89, 0x4521, 0x8f, 0x66, 0x33, 0x15, 0x1c, 0x37, 0x31, 0x76);
		public static readonly Guid CODECAPI_AVEncCommonAllowFrameDrops = new Guid(0xd8477dcb, 0x9598, 0x48e3, 0x8d, 0x0c, 0x75, 0x2b, 0xf2, 0x06, 0x09, 0x3e);
		public static readonly Guid CODECAPI_AVEncVideoIntraLayerPrediction = new Guid(0xd3af46b8, 0xbf47, 0x44bb, 0xa2, 0x83, 0x69, 0xf0, 0xb0, 0x22, 0x8f, 0xf9);
		public static readonly Guid CODECAPI_AVEncVideoInstantTemporalUpSwitching = new Guid(0xa3308307, 0x0d96, 0x4ba4, 0xb1, 0xf0, 0xb9, 0x1a, 0x5e, 0x49, 0xdf, 0x10);
		public static readonly Guid CODECAPI_AVEncLowPowerEncoder = new Guid(0xb668d582, 0x8bad, 0x4f6a, 0x91, 0x41, 0x37, 0x5a, 0x95, 0x35, 0x8b, 0x6d);
		public static readonly Guid CODECAPI_AVEnableInLoopDeblockFilter = new Guid(0xd2e8e399, 0x0623, 0x4bf3, 0x92, 0xa8, 0x4d, 0x18, 0x18, 0x52, 0x9d, 0xed);
		public static readonly Guid CODECAPI_AVEncMuxOutputStreamType = new Guid(0xcedd9e8f, 0x34d3, 0x44db, 0xa1, 0xd8, 0xf8, 0x15, 0x20, 0x25, 0x4f, 0x3e);
		public static readonly Guid CODECAPI_AVEncStatVideoOutputFrameRate = new Guid(0xbe747849, 0x9ab4, 0x4a63, 0x98, 0xfe, 0xf1, 0x43, 0xf0, 0x4f, 0x8e, 0xe9);
		public static readonly Guid CODECAPI_AVEncStatVideoCodedFrames = new Guid(0xd47f8d61, 0x6f5a, 0x4a26, 0xbb, 0x9f, 0xcd, 0x95, 0x18, 0x46, 0x2b, 0xcd);
		public static readonly Guid CODECAPI_AVEncStatVideoTotalFrames = new Guid(0xfdaa9916, 0x119a, 0x4222, 0x9a, 0xd6, 0x3f, 0x7c, 0xab, 0x99, 0xcc, 0x8b);
		public static readonly Guid CODECAPI_AVEncAudioIntervalToEncode = new Guid(0x866e4b4d, 0x725a, 0x467c, 0xbb, 0x01, 0xb4, 0x96, 0xb2, 0x3b, 0x25, 0xf9);
		public static readonly Guid CODECAPI_AVEncAudioIntervalToSkip = new Guid(0x88c15f94, 0xc38c, 0x4796, 0xa9, 0xe8, 0x96, 0xe9, 0x67, 0x98, 0x3f, 0x26);
		public static readonly Guid CODECAPI_AVEncAudioDualMono = new Guid(0x3648126b, 0xa3e8, 0x4329, 0x9b, 0x3a, 0x5c, 0xe5, 0x66, 0xa4, 0x3b, 0xd3);
		public static readonly Guid CODECAPI_AVEncAudioMeanBitRate = new Guid(0x921295bb, 0x4fca, 0x4679, 0xaa, 0xb8, 0x9e, 0x2a, 0x1d, 0x75, 0x33, 0x84);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel0 = new Guid(0xbc5d0b60, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel1 = new Guid(0xbc5d0b61, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel2 = new Guid(0xbc5d0b62, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel3 = new Guid(0xbc5d0b63, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel4 = new Guid(0xbc5d0b64, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel5 = new Guid(0xbc5d0b65, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel6 = new Guid(0xbc5d0b66, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel7 = new Guid(0xbc5d0b67, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel8 = new Guid(0xbc5d0b68, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel9 = new Guid(0xbc5d0b69, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel10 = new Guid(0xbc5d0b6a, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel11 = new Guid(0xbc5d0b6b, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel12 = new Guid(0xbc5d0b6c, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel13 = new Guid(0xbc5d0b6d, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel14 = new Guid(0xbc5d0b6e, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioMapDestChannel15 = new Guid(0xbc5d0b6f, 0xdf6a, 0x4e16, 0x98, 0x03, 0xb8, 0x20, 0x07, 0xa3, 0x0c, 0x8d);
		public static readonly Guid CODECAPI_AVEncAudioInputContent = new Guid(0x3e226c2b, 0x60b9, 0x4a39, 0xb0, 0x0b, 0xa7, 0xb4, 0x0f, 0x70, 0xd5, 0x66);
		public static readonly Guid CODECAPI_AVEncStatAudioPeakPCMValue = new Guid(0xdce7fd34, 0xdc00, 0x4c16, 0x82, 0x1b, 0x35, 0xd9, 0xeb, 0x00, 0xfb, 0x1a);
		public static readonly Guid CODECAPI_AVEncStatAudioAveragePCMValue = new Guid(0x979272f8, 0xd17f, 0x4e32, 0xbb, 0x73, 0x4e, 0x73, 0x1c, 0x68, 0xba, 0x2d);
		public static readonly Guid CODECAPI_AVEncStatAudioAverageBPS = new Guid(0xca6724db, 0x7059, 0x4351, 0x8b, 0x43, 0xf8, 0x21, 0x98, 0x82, 0x6a, 0x14);
		public static readonly Guid CODECAPI_AVEncStatAverageBPS = new Guid(0xca6724db, 0x7059, 0x4351, 0x8b, 0x43, 0xf8, 0x21, 0x98, 0x82, 0x6a, 0x14);
		public static readonly Guid CODECAPI_AVEncStatHardwareProcessorUtilitization = new Guid(0x995dc027, 0xcb95, 0x49e6, 0xb9, 0x1b, 0x59, 0x67, 0x75, 0x3c, 0xdc, 0xb8);
		public static readonly Guid CODECAPI_AVEncStatHardwareBandwidthUtilitization = new Guid(0x0124ba9b, 0xdc41, 0x4826, 0xb4, 0x5f, 0x18, 0xac, 0x01, 0xb3, 0xd5, 0xa8);
		public static readonly Guid CODECAPI_AVEncMPVGOPSize = new Guid(0x95f31b26, 0x95a4, 0x41aa, 0x93, 0x03, 0x24, 0x6a, 0x7f, 0xc6, 0xee, 0xf1);
		public static readonly Guid CODECAPI_AVEncMPVGOPOpen = new Guid(0xb1d5d4a6, 0x3300, 0x49b1, 0xae, 0x61, 0xa0, 0x99, 0x37, 0xab, 0x0e, 0x49);
		public static readonly Guid CODECAPI_AVEncMPVDefaultBPictureCount = new Guid(0x8d390aac, 0xdc5c, 0x4200, 0xb5, 0x7f, 0x81, 0x4d, 0x04, 0xba, 0xba, 0xb2);
		public static readonly Guid CODECAPI_AVEncMPVProfile = new Guid(0xdabb534a, 0x1d99, 0x4284, 0x97, 0x5a, 0xd9, 0x0e, 0x22, 0x39, 0xba, 0xa1);
		public static readonly Guid CODECAPI_AVEncMPVLevel = new Guid(0x6ee40c40, 0xa60c, 0x41ef, 0x8f, 0x50, 0x37, 0xc2, 0x24, 0x9e, 0x2c, 0xb3);
		public static readonly Guid CODECAPI_AVEncMPVFrameFieldMode = new Guid(0xacb5de96, 0x7b93, 0x4c2f, 0x88, 0x25, 0xb0, 0x29, 0x5f, 0xa9, 0x3b, 0xf4);
		public static readonly Guid CODECAPI_AVEncMPVAddSeqEndCode = new Guid(0xa823178f, 0x57df, 0x4c7a, 0xb8, 0xfd, 0xe5, 0xec, 0x88, 0x87, 0x70, 0x8d);
		public static readonly Guid CODECAPI_AVEncMPVGOPSInSeq = new Guid(0x993410d4, 0x2691, 0x4192, 0x99, 0x78, 0x98, 0xdc, 0x26, 0x03, 0x66, 0x9f);
		public static readonly Guid CODECAPI_AVEncMPVUseConcealmentMotionVectors = new Guid(0xec770cf3, 0x6908, 0x4b4b, 0xaa, 0x30, 0x7f, 0xb9, 0x86, 0x21, 0x4f, 0xea);
		public static readonly Guid CODECAPI_AVEncMPVSceneDetection = new Guid(0x552799f1, 0xdb4c, 0x405b, 0x8a, 0x3a, 0xc9, 0x3f, 0x2d, 0x06, 0x74, 0xdc);
		public static readonly Guid CODECAPI_AVEncMPVGenerateHeaderSeqExt = new Guid(0xd5e78611, 0x082d, 0x4e6b, 0x98, 0xaf, 0x0f, 0x51, 0xab, 0x13, 0x92, 0x22);
		public static readonly Guid CODECAPI_AVEncMPVGenerateHeaderSeqDispExt = new Guid(0x6437aa6f, 0x5a3c, 0x4de9, 0x8a, 0x16, 0x53, 0xd9, 0xc4, 0xad, 0x32, 0x6f);
		public static readonly Guid CODECAPI_AVEncMPVGenerateHeaderPicExt = new Guid(0x1b8464ab, 0x944f, 0x45f0, 0xb7, 0x4e, 0x3a, 0x58, 0xda, 0xd1, 0x1f, 0x37);
		public static readonly Guid CODECAPI_AVEncMPVGenerateHeaderPicDispExt = new Guid(0xc6412f84, 0xc03f, 0x4f40, 0xa0, 0x0c, 0x42, 0x93, 0xdf, 0x83, 0x95, 0xbb);
		public static readonly Guid CODECAPI_AVEncMPVGenerateHeaderSeqScaleExt = new Guid(0x0722d62f, 0xdd59, 0x4a86, 0x9c, 0xd5, 0x64, 0x4f, 0x8e, 0x26, 0x53, 0xd8);
		public static readonly Guid CODECAPI_AVEncMPVScanPattern = new Guid(0x7f8a478e, 0x7bbb, 0x4ae2, 0xb2, 0xfc, 0x96, 0xd1, 0x7f, 0xc4, 0xa2, 0xd6);
		public static readonly Guid CODECAPI_AVEncMPVIntraDCPrecision = new Guid(0xa0116151, 0xcbc8, 0x4af3, 0x97, 0xdc, 0xd0, 0x0c, 0xce, 0xb8, 0x2d, 0x79);
		public static readonly Guid CODECAPI_AVEncMPVQScaleType = new Guid(0x2b79ebb7, 0xf484, 0x4af7, 0xbb, 0x58, 0xa2, 0xa1, 0x88, 0xc5, 0xcb, 0xbe);
		public static readonly Guid CODECAPI_AVEncMPVIntraVLCTable = new Guid(0xa2b83ff5, 0x1a99, 0x405a, 0xaf, 0x95, 0xc5, 0x99, 0x7d, 0x55, 0x8d, 0x3a);
		public static readonly Guid CODECAPI_AVEncMPVQuantMatrixIntra = new Guid(0x9bea04f3, 0x6621, 0x442c, 0x8b, 0xa1, 0x3a, 0xc3, 0x78, 0x97, 0x96, 0x98);
		public static readonly Guid CODECAPI_AVEncMPVQuantMatrixNonIntra = new Guid(0x87f441d8, 0x0997, 0x4beb, 0xa0, 0x8e, 0x85, 0x73, 0xd4, 0x09, 0xcf, 0x75);
		public static readonly Guid CODECAPI_AVEncMPVQuantMatrixChromaIntra = new Guid(0x9eb9ecd4, 0x018d, 0x4ffd, 0x8f, 0x2d, 0x39, 0xe4, 0x9f, 0x07, 0xb1, 0x7a);
		public static readonly Guid CODECAPI_AVEncMPVQuantMatrixChromaNonIntra = new Guid(0x1415b6b1, 0x362a, 0x4338, 0xba, 0x9a, 0x1e, 0xf5, 0x87, 0x03, 0xc0, 0x5b);
		public static readonly Guid CODECAPI_AVEncMPALayer = new Guid(0x9d377230, 0xf91b, 0x453d, 0x9c, 0xe0, 0x78, 0x44, 0x54, 0x14, 0xc2, 0x2d);
		public static readonly Guid CODECAPI_AVEncMPACodingMode = new Guid(0xb16ade03, 0x4b93, 0x43d7, 0xa5, 0x50, 0x90, 0xb4, 0xfe, 0x22, 0x45, 0x37);
		public static readonly Guid CODECAPI_AVEncDDService = new Guid(0xd2e1bec7, 0x5172, 0x4d2a, 0xa5, 0x0e, 0x2f, 0x3b, 0x82, 0xb1, 0xdd, 0xf8);
		public static readonly Guid CODECAPI_AVEncDDDialogNormalization = new Guid(0xd7055acf, 0xf125, 0x437d, 0xa7, 0x04, 0x79, 0xc7, 0x9f, 0x04, 0x04, 0xa8);
		public static readonly Guid CODECAPI_AVEncDDCentreDownMixLevel = new Guid(0xe285072c, 0xc958, 0x4a81, 0xaf, 0xd2, 0xe5, 0xe0, 0xda, 0xf1, 0xb1, 0x48);
		public static readonly Guid CODECAPI_AVEncDDSurroundDownMixLevel = new Guid(0x7b20d6e5, 0x0bcf, 0x4273, 0xa4, 0x87, 0x50, 0x6b, 0x04, 0x79, 0x97, 0xe9);
		public static readonly Guid CODECAPI_AVEncDDProductionInfoExists = new Guid(0xb0b7fe5f, 0xb6ab, 0x4f40, 0x96, 0x4d, 0x8d, 0x91, 0xf1, 0x7c, 0x19, 0xe8);
		public static readonly Guid CODECAPI_AVEncDDProductionRoomType = new Guid(0xdad7ad60, 0x23d8, 0x4ab7, 0xa2, 0x84, 0x55, 0x69, 0x86, 0xd8, 0xa6, 0xfe);
		public static readonly Guid CODECAPI_AVEncDDProductionMixLevel = new Guid(0x301d103a, 0xcbf9, 0x4776, 0x88, 0x99, 0x7c, 0x15, 0xb4, 0x61, 0xab, 0x26);
		public static readonly Guid CODECAPI_AVEncDDCopyright = new Guid(0x8694f076, 0xcd75, 0x481d, 0xa5, 0xc6, 0xa9, 0x04, 0xdc, 0xc8, 0x28, 0xf0);
		public static readonly Guid CODECAPI_AVEncDDOriginalBitstream = new Guid(0x966ae800, 0x5bd3, 0x4ff9, 0x95, 0xb9, 0xd3, 0x05, 0x66, 0x27, 0x38, 0x56);
		public static readonly Guid CODECAPI_AVEncDDDigitalDeemphasis = new Guid(0xe024a2c2, 0x947c, 0x45ac, 0x87, 0xd8, 0xf1, 0x03, 0x0c, 0x5c, 0x00, 0x82);
		public static readonly Guid CODECAPI_AVEncDDDCHighPassFilter = new Guid(0x9565239f, 0x861c, 0x4ac8, 0xbf, 0xda, 0xe0, 0x0c, 0xb4, 0xdb, 0x85, 0x48);
		public static readonly Guid CODECAPI_AVEncDDChannelBWLowPassFilter = new Guid(0xe197821d, 0xd2e7, 0x43e2, 0xad, 0x2c, 0x00, 0x58, 0x2f, 0x51, 0x85, 0x45);
		public static readonly Guid CODECAPI_AVEncDDLFELowPassFilter = new Guid(0xd3b80f6f, 0x9d15, 0x45e5, 0x91, 0xbe, 0x01, 0x9c, 0x3f, 0xab, 0x1f, 0x01);
		public static readonly Guid CODECAPI_AVEncDDSurround90DegreeePhaseShift = new Guid(0x25ecec9d, 0x3553, 0x42c0, 0xbb, 0x56, 0xd2, 0x57, 0x92, 0x10, 0x4f, 0x80);
		public static readonly Guid CODECAPI_AVEncDDSurround3dBAttenuation = new Guid(0x4d43b99d, 0x31e2, 0x48b9, 0xbf, 0x2e, 0x5c, 0xbf, 0x1a, 0x57, 0x27, 0x84);
		public static readonly Guid CODECAPI_AVEncDDDynamicRangeCompressionControl = new Guid(0xcfc2ff6d, 0x79b8, 0x4b8d, 0xa8, 0xaa, 0xa0, 0xc9, 0xbd, 0x1c, 0x29, 0x40);
		public static readonly Guid CODECAPI_AVEncDDRFPreEmphasisFilter = new Guid(0x21af44c0, 0x244e, 0x4f3d, 0xa2, 0xcc, 0x3d, 0x30, 0x68, 0xb2, 0xe7, 0x3f);
		public static readonly Guid CODECAPI_AVEncDDSurroundExMode = new Guid(0x91607cee, 0xdbdd, 0x4eb6, 0xbc, 0xa2, 0xaa, 0xdf, 0xaf, 0xa3, 0xdd, 0x68);
		public static readonly Guid CODECAPI_AVEncDDPreferredStereoDownMixMode = new Guid(0x7f4e6b31, 0x9185, 0x403d, 0xb0, 0xa2, 0x76, 0x37, 0x43, 0xe6, 0xf0, 0x63);
		public static readonly Guid CODECAPI_AVEncDDLtRtCenterMixLvl_x10 = new Guid(0xdca128a2, 0x491f, 0x4600, 0xb2, 0xda, 0x76, 0xe3, 0x34, 0x4b, 0x41, 0x97);
		public static readonly Guid CODECAPI_AVEncDDLtRtSurroundMixLvl_x10 = new Guid(0x212246c7, 0x3d2c, 0x4dfa, 0xbc, 0x21, 0x65, 0x2a, 0x90, 0x98, 0x69, 0x0d);
		public static readonly Guid CODECAPI_AVEncDDLoRoCenterMixLvl_x10 = new Guid(0x1cfba222, 0x25b3, 0x4bf4, 0x9b, 0xfd, 0xe7, 0x11, 0x12, 0x67, 0x85, 0x8c);
		public static readonly Guid CODECAPI_AVEncDDLoRoSurroundMixLvl_x10 = new Guid(0xe725cff6, 0xeb56, 0x40c7, 0x84, 0x50, 0x2b, 0x93, 0x67, 0xe9, 0x15, 0x55);
		public static readonly Guid CODECAPI_AVEncDDAtoDConverterType = new Guid(0x719f9612, 0x81a1, 0x47e0, 0x9a, 0x05, 0xd9, 0x4a, 0xd5, 0xfc, 0xa9, 0x48);
		public static readonly Guid CODECAPI_AVEncDDHeadphoneMode = new Guid(0x4052dbec, 0x52f5, 0x42f5, 0x9b, 0x00, 0xd1, 0x34, 0xb1, 0x34, 0x1b, 0x9d);
		public static readonly Guid CODECAPI_AVEncWMVKeyFrameDistance = new Guid(0x5569055e, 0xe268, 0x4771, 0xb8, 0x3e, 0x95, 0x55, 0xea, 0x28, 0xae, 0xd3);
		public static readonly Guid CODECAPI_AVEncWMVInterlacedEncoding = new Guid(0xe3d00f8a, 0xc6f5, 0x4e14, 0xa5, 0x88, 0x0e, 0xc8, 0x7a, 0x72, 0x6f, 0x9b);
		public static readonly Guid CODECAPI_AVEncWMVDecoderComplexity = new Guid(0xf32c0dab, 0xf3cb, 0x4217, 0xb7, 0x9f, 0x87, 0x62, 0x76, 0x8b, 0x5f, 0x67);
		public static readonly Guid CODECAPI_AVEncWMVKeyFrameBufferLevelMarker = new Guid(0x51ff1115, 0x33ac, 0x426c, 0xa1, 0xb1, 0x09, 0x32, 0x1b, 0xdf, 0x96, 0xb4);
		public static readonly Guid CODECAPI_AVEncWMVProduceDummyFrames = new Guid(0xd669d001, 0x183c, 0x42e3, 0xa3, 0xca, 0x2f, 0x45, 0x86, 0xd2, 0x39, 0x6c);
		public static readonly Guid CODECAPI_AVEncStatWMVCBAvg = new Guid(0x6aa6229f, 0xd602, 0x4b9d, 0xb6, 0x8c, 0xc1, 0xad, 0x78, 0x88, 0x4b, 0xef);
		public static readonly Guid CODECAPI_AVEncStatWMVCBMax = new Guid(0xe976bef8, 0x00fe, 0x44b4, 0xb6, 0x25, 0x8f, 0x23, 0x8b, 0xc0, 0x34, 0x99);
		public static readonly Guid CODECAPI_AVEncStatWMVDecoderComplexityProfile = new Guid(0x89e69fc3, 0x0f9b, 0x436c, 0x97, 0x4a, 0xdf, 0x82, 0x12, 0x27, 0xc9, 0x0d);
		public static readonly Guid CODECAPI_AVEncStatMPVSkippedEmptyFrames = new Guid(0x32195fd3, 0x590d, 0x4812, 0xa7, 0xed, 0x6d, 0x63, 0x9a, 0x1f, 0x97, 0x11);
		public static readonly Guid CODECAPI_AVEncMP12PktzSTDBuffer = new Guid(0x0b751bd0, 0x819e, 0x478c, 0x94, 0x35, 0x75, 0x20, 0x89, 0x26, 0xb3, 0x77);
		public static readonly Guid CODECAPI_AVEncMP12PktzStreamID = new Guid(0xc834d038, 0xf5e8, 0x4408, 0x9b, 0x60, 0x88, 0xf3, 0x64, 0x93, 0xfe, 0xdf);
		public static readonly Guid CODECAPI_AVEncMP12PktzInitialPTS = new Guid(0x2a4f2065, 0x9a63, 0x4d20, 0xae, 0x22, 0x0a, 0x1b, 0xc8, 0x96, 0xa3, 0x15);
		public static readonly Guid CODECAPI_AVEncMP12PktzPacketSize = new Guid(0xab71347a, 0x1332, 0x4dde, 0xa0, 0xe5, 0xcc, 0xf7, 0xda, 0x8a, 0x0f, 0x22);
		public static readonly Guid CODECAPI_AVEncMP12PktzCopyright = new Guid(0xc8f4b0c1, 0x094c, 0x43c7, 0x8e, 0x68, 0xa5, 0x95, 0x40, 0x5a, 0x6e, 0xf8);
		public static readonly Guid CODECAPI_AVEncMP12PktzOriginal = new Guid(0x6b178416, 0x31b9, 0x4964, 0x94, 0xcb, 0x6b, 0xff, 0x86, 0x6c, 0xdf, 0x83);
		public static readonly Guid CODECAPI_AVEncMP12MuxPacketOverhead = new Guid(0xe40bd720, 0x3955, 0x4453, 0xac, 0xf9, 0xb7, 0x91, 0x32, 0xa3, 0x8f, 0xa0);
		public static readonly Guid CODECAPI_AVEncMP12MuxNumStreams = new Guid(0xf7164a41, 0xdced, 0x4659, 0xa8, 0xf2, 0xfb, 0x69, 0x3f, 0x2a, 0x4c, 0xd0);
		public static readonly Guid CODECAPI_AVEncMP12MuxEarliestPTS = new Guid(0x157232b6, 0xf809, 0x474e, 0x94, 0x64, 0xa7, 0xf9, 0x30, 0x14, 0xa8, 0x17);
		public static readonly Guid CODECAPI_AVEncMP12MuxLargestPacketSize = new Guid(0x35ceb711, 0xf461, 0x4b92, 0xa4, 0xef, 0x17, 0xb6, 0x84, 0x1e, 0xd2, 0x54);
		public static readonly Guid CODECAPI_AVEncMP12MuxInitialSCR = new Guid(0x3433ad21, 0x1b91, 0x4a0b, 0xb1, 0x90, 0x2b, 0x77, 0x06, 0x3b, 0x63, 0xa4);
		public static readonly Guid CODECAPI_AVEncMP12MuxMuxRate = new Guid(0xee047c72, 0x4bdb, 0x4a9d, 0x8e, 0x21, 0x41, 0x92, 0x6c, 0x82, 0x3d, 0xa7);
		public static readonly Guid CODECAPI_AVEncMP12MuxPackSize = new Guid(0xf916053a, 0x1ce8, 0x4faf, 0xaa, 0x0b, 0xba, 0x31, 0xc8, 0x00, 0x34, 0xb8);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysSTDBufferBound = new Guid(0x35746903, 0xb545, 0x43e7, 0xbb, 0x35, 0xc5, 0xe0, 0xa7, 0xd5, 0x09, 0x3c);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysRateBound = new Guid(0x05f0428a, 0xee30, 0x489d, 0xae, 0x28, 0x20, 0x5c, 0x72, 0x44, 0x67, 0x10);
		public static readonly Guid CODECAPI_AVEncMP12MuxTargetPacketizer = new Guid(0xd862212a, 0x2015, 0x45dd, 0x9a, 0x32, 0x1b, 0x3a, 0xa8, 0x82, 0x05, 0xa0);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysFixed = new Guid(0xcefb987e, 0x894f, 0x452e, 0x8f, 0x89, 0xa4, 0xef, 0x8c, 0xec, 0x06, 0x3a);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysCSPS = new Guid(0x7952ff45, 0x9c0d, 0x4822, 0xbc, 0x82, 0x8a, 0xd7, 0x72, 0xe0, 0x29, 0x93);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysVideoLock = new Guid(0xb8296408, 0x2430, 0x4d37, 0xa2, 0xa1, 0x95, 0xb3, 0xe4, 0x35, 0xa9, 0x1d);
		public static readonly Guid CODECAPI_AVEncMP12MuxSysAudioLock = new Guid(0x0fbb5752, 0x1d43, 0x47bf, 0xbd, 0x79, 0xf2, 0x29, 0x3d, 0x8c, 0xe3, 0x37);
		public static readonly Guid CODECAPI_AVEncMP12MuxDVDNavPacks = new Guid(0xc7607ced, 0x8cf1, 0x4a99, 0x83, 0xa1, 0xee, 0x54, 0x61, 0xbe, 0x35, 0x74);
		public static readonly Guid CODECAPI_AVEncMPACopyright = new Guid(0xa6ae762a, 0xd0a9, 0x4454, 0xb8, 0xef, 0xf2, 0xdb, 0xee, 0xfd, 0xd3, 0xbd);
		public static readonly Guid CODECAPI_AVEncMPAOriginalBitstream = new Guid(0x3cfb7855, 0x9cc9, 0x47ff, 0xb8, 0x29, 0xb3, 0x67, 0x86, 0xc9, 0x23, 0x46);
		public static readonly Guid CODECAPI_AVEncMPAEnableRedundancyProtection = new Guid(0x5e54b09e, 0xb2e7, 0x4973, 0xa8, 0x9b, 0x0b, 0x36, 0x50, 0xa3, 0xbe, 0xda);
		public static readonly Guid CODECAPI_AVEncMPAPrivateUserBit = new Guid(0xafa505ce, 0xc1e3, 0x4e3d, 0x85, 0x1b, 0x61, 0xb7, 0x00, 0xe5, 0xe6, 0xcc);
		public static readonly Guid CODECAPI_AVEncMPAEmphasisType = new Guid(0x2d59fcda, 0xbf4e, 0x4ed6, 0xb5, 0xdf, 0x5b, 0x03, 0xb3, 0x6b, 0x0a, 0x1f);
		public static readonly Guid CODECAPI_AVDecCommonMeanBitRate = new Guid(0x59488217, 0x007a, 0x4f7a, 0x8e, 0x41, 0x5c, 0x48, 0xb1, 0xea, 0xc5, 0xc6);
		public static readonly Guid CODECAPI_AVDecCommonMeanBitRateInterval = new Guid(0x0ee437c6, 0x38a7, 0x4c5c, 0x94, 0x4c, 0x68, 0xab, 0x42, 0x11, 0x6b, 0x85);
		public static readonly Guid CODECAPI_AVDecCommonInputFormat = new Guid(0xe5005239, 0xbd89, 0x4be3, 0x9c, 0x0f, 0x5d, 0xde, 0x31, 0x79, 0x88, 0xcc);
		public static readonly Guid CODECAPI_AVDecCommonOutputFormat = new Guid(0x3c790028, 0xc0ce, 0x4256, 0xb1, 0xa2, 0x1b, 0x0f, 0xc8, 0xb1, 0xdc, 0xdc);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_PCM_Stereo_MatrixEncoded = new Guid(0x696e1d30, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_PCM = new Guid(0x696e1d31, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_SPDIF_PCM = new Guid(0x696e1d32, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_SPDIF_Bitstream = new Guid(0x696e1d33, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_PCM_Headphones = new Guid(0x696e1d34, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_GUID_AVDecAudioOutputFormat_PCM_Stereo_Auto = new Guid(0x696e1d35, 0x548f, 0x4036, 0x82, 0x5f, 0x70, 0x26, 0xc6, 0x00, 0x11, 0xbd);
		public static readonly Guid CODECAPI_AVDecVideoImageSize = new Guid(0x5ee5747c, 0x6801, 0x4cab, 0xaa, 0xf1, 0x62, 0x48, 0xfa, 0x84, 0x1b, 0xa4);
		public static readonly Guid CODECAPI_AVDecVideoInputScanType = new Guid(0x38477e1f, 0x0ea7, 0x42cd, 0x8c, 0xd1, 0x13, 0x0c, 0xed, 0x57, 0xc5, 0x80);
		public static readonly Guid CODECAPI_AVDecVideoPixelAspectRatio = new Guid(0xb0cf8245, 0xf32d, 0x41df, 0xb0, 0x2c, 0x87, 0xbd, 0x30, 0x4d, 0x12, 0xab);
		public static readonly Guid CODECAPI_AVDecVideoAcceleration_MPEG2 = new Guid(0xf7db8a2e, 0x4f48, 0x4ee8, 0xae, 0x31, 0x8b, 0x6e, 0xbe, 0x55, 0x8a, 0xe2);
		public static readonly Guid CODECAPI_AVDecVideoAcceleration_H264 = new Guid(0xf7db8a2f, 0x4f48, 0x4ee8, 0xae, 0x31, 0x8b, 0x6e, 0xbe, 0x55, 0x8a, 0xe2);
		public static readonly Guid CODECAPI_AVDecVideoAcceleration_VC1 = new Guid(0xf7db8a30, 0x4f48, 0x4ee8, 0xae, 0x31, 0x8b, 0x6e, 0xbe, 0x55, 0x8a, 0xe2);
		public static readonly Guid CODECAPI_AVDecVideoProcDeinterlaceCSC = new Guid(0xf7db8a31, 0x4f48, 0x4ee8, 0xae, 0x31, 0x8b, 0x6e, 0xbe, 0x55, 0x8a, 0xe2);
		public static readonly Guid CODECAPI_AVDecVideoThumbnailGenerationMode = new Guid(0x2efd8eee, 0x1150, 0x4328, 0x9c, 0xf5, 0x66, 0xdc, 0xe9, 0x33, 0xfc, 0xf4);
		public static readonly Guid CODECAPI_AVDecVideoDropPicWithMissingRef = new Guid(0xf8226383, 0x14c2, 0x4567, 0x97, 0x34, 0x50, 0x4, 0xe9, 0x6f, 0xf8, 0x87);
		public static readonly Guid CODECAPI_AVDecVideoSoftwareDeinterlaceMode = new Guid(0x0c08d1ce, 0x9ced, 0x4540, 0xba, 0xe3, 0xce, 0xb3, 0x80, 0x14, 0x11, 0x09);
		public static readonly Guid CODECAPI_AVDecVideoFastDecodeMode = new Guid(0x6b529f7d, 0xd3b1, 0x49c6, 0xa9, 0x99, 0x9e, 0xc6, 0x91, 0x1b, 0xed, 0xbf);
		public static readonly Guid CODECAPI_AVLowLatencyMode = new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee);
		public static readonly Guid CODECAPI_AVDecVideoH264ErrorConcealment = new Guid(0xececace8, 0x3436, 0x462c, 0x92, 0x94, 0xcd, 0x7b, 0xac, 0xd7, 0x58, 0xa9);
		public static readonly Guid CODECAPI_AVDecVideoMPEG2ErrorConcealment = new Guid(0x9d2bfe18, 0x728d, 0x48d2, 0xb3, 0x58, 0xbc, 0x7e, 0x43, 0x6c, 0x66, 0x74);
		public static readonly Guid CODECAPI_AVDecVideoCodecType = new Guid(0x434528e5, 0x21f0, 0x46b6, 0xb6, 0x2c, 0x9b, 0x1b, 0x6b, 0x65, 0x8c, 0xd1);
		public static readonly Guid CODECAPI_AVDecVideoDXVAMode = new Guid(0xf758f09e, 0x7337, 0x4ae7, 0x83, 0x87, 0x73, 0xdc, 0x2d, 0x54, 0xe6, 0x7d);
		public static readonly Guid CODECAPI_AVDecVideoDXVABusEncryption = new Guid(0x42153c8b, 0xfd0b, 0x4765, 0xa4, 0x62, 0xdd, 0xd9, 0xe8, 0xbc, 0xc3, 0x88);
		public static readonly Guid CODECAPI_AVDecVideoSWPowerLevel = new Guid(0xfb5d2347, 0x4dd8, 0x4509, 0xae, 0xd0, 0xdb, 0x5f, 0xa9, 0xaa, 0x93, 0xf4);
		public static readonly Guid CODECAPI_AVDecVideoMaxCodedWidth = new Guid(0x5ae557b8, 0x77af, 0x41f5, 0x9f, 0xa6, 0x4d, 0xb2, 0xfe, 0x1d, 0x4b, 0xca);
		public static readonly Guid CODECAPI_AVDecVideoMaxCodedHeight = new Guid(0x7262a16a, 0xd2dc, 0x4e75, 0x9b, 0xa8, 0x65, 0xc0, 0xc6, 0xd3, 0x2b, 0x13);
		public static readonly Guid CODECAPI_AVDecNumWorkerThreads = new Guid(0x9561c3e8, 0xea9e, 0x4435, 0x9b, 0x1e, 0xa9, 0x3e, 0x69, 0x18, 0x94, 0xd8);
		public static readonly Guid CODECAPI_AVDecSoftwareDynamicFormatChange = new Guid(0x862e2f0a, 0x507b, 0x47ff, 0xaf, 0x47, 0x1, 0xe2, 0x62, 0x42, 0x98, 0xb7);
		public static readonly Guid CODECAPI_AVDecDisableVideoPostProcessing = new Guid(0xf8749193, 0x667a, 0x4f2c, 0xa9, 0xe8, 0x5d, 0x4a, 0xf9, 0x24, 0xf0, 0x8f);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputWMA = new Guid(0xc95e8dcf, 0x4058, 0x4204, 0x8c, 0x42, 0xcb, 0x24, 0xd9, 0x1e, 0x4b, 0x9b);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputWMAPro = new Guid(0x0128b7c7, 0xda72, 0x4fe3, 0xbe, 0xf8, 0x5c, 0x52, 0xe3, 0x55, 0x77, 0x04);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputDolby = new Guid(0x8e4228a0, 0xf000, 0x4e0b, 0x8f, 0x54, 0xab, 0x8d, 0x24, 0xad, 0x61, 0xa2);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputDTS = new Guid(0x600bc0ca, 0x6a1f, 0x4e91, 0xb2, 0x41, 0x1b, 0xbe, 0xb1, 0xcb, 0x19, 0xe0);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputPCM = new Guid(0xf2421da5, 0xbbb4, 0x4cd5, 0xa9, 0x96, 0x93, 0x3c, 0x6b, 0x5d, 0x13, 0x47);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputMPEG = new Guid(0x91106f36, 0x02c5, 0x4f75, 0x97, 0x19, 0x3b, 0x7a, 0xbf, 0x75, 0xe1, 0xf6);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputAAC = new Guid(0x97df7828, 0xb94a, 0x47e2, 0xa4, 0xbc, 0x51, 0x19, 0x4d, 0xb2, 0x2a, 0x4d);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputHEAAC = new Guid(0x16efb4aa, 0x330e, 0x4f5c, 0x98, 0xa8, 0xcf, 0x6a, 0xc5, 0x5c, 0xbe, 0x60);
		public static readonly Guid CODECAPI_GUID_AVDecAudioInputDolbyDigitalPlus = new Guid(0x0803e185, 0x8f5d, 0x47f5, 0x99, 0x08, 0x19, 0xa5, 0xbb, 0xc9, 0xfe, 0x34);
		public static readonly Guid CODECAPI_AVDecAACDownmixMode = new Guid(0x01274475, 0xf6bb, 0x4017, 0xb0, 0x84, 0x81, 0xa7, 0x63, 0xc9, 0x42, 0xd4);
		public static readonly Guid CODECAPI_AVDecHEAACDynamicRangeControl = new Guid(0x287c8abe, 0x69a4, 0x4d39, 0x80, 0x80, 0xd3, 0xd9, 0x71, 0x21, 0x78, 0xa0);
		public static readonly Guid CODECAPI_AVDecAudioDualMono = new Guid(0x4a52cda8, 0x30f8, 0x4216, 0xbe, 0x0f, 0xba, 0x0b, 0x20, 0x25, 0x92, 0x1d);
		public static readonly Guid CODECAPI_AVDecAudioDualMonoReproMode = new Guid(0xa5106186, 0xcc94, 0x4bc9, 0x8c, 0xd9, 0xaa, 0x2f, 0x61, 0xf6, 0x80, 0x7e);
		public static readonly Guid CODECAPI_AVAudioChannelCount = new Guid(0x1d3583c4, 0x1583, 0x474e, 0xb7, 0x1a, 0x5e, 0xe4, 0x63, 0xc1, 0x98, 0xe4);
		public static readonly Guid CODECAPI_AVAudioChannelConfig = new Guid(0x17f89cb3, 0xc38d, 0x4368, 0x9e, 0xde, 0x63, 0xb9, 0x4d, 0x17, 0x7f, 0x9f);
		public static readonly Guid CODECAPI_AVAudioSampleRate = new Guid(0x971d2723, 0x1acb, 0x42e7, 0x85, 0x5c, 0x52, 0x0a, 0x4b, 0x70, 0xa5, 0xf2);
		public static readonly Guid CODECAPI_AVDDSurroundMode = new Guid(0x99f2f386, 0x98d1, 0x4452, 0xa1, 0x63, 0xab, 0xc7, 0x8a, 0x6e, 0xb7, 0x70);
		public static readonly Guid CODECAPI_AVDecDDOperationalMode = new Guid(0xd6d6c6d1, 0x064e, 0x4fdd, 0xa4, 0x0e, 0x3e, 0xcb, 0xfc, 0xb7, 0xeb, 0xd0);
		public static readonly Guid CODECAPI_AVDecDDMatrixDecodingMode = new Guid(0xddc811a5, 0x04ed, 0x4bf3, 0xa0, 0xca, 0xd0, 0x04, 0x49, 0xf9, 0x35, 0x5f);
		public static readonly Guid CODECAPI_AVDecDDDynamicRangeScaleHigh = new Guid(0x50196c21, 0x1f33, 0x4af5, 0xb2, 0x96, 0x11, 0x42, 0x6d, 0x6c, 0x87, 0x89);
		public static readonly Guid CODECAPI_AVDecDDDynamicRangeScaleLow = new Guid(0x044e62e4, 0x11a5, 0x42d5, 0xa3, 0xb2, 0x3b, 0xb2, 0xc7, 0xc2, 0xd7, 0xcf);
		public static readonly Guid CODECAPI_AVDecDDStereoDownMixMode = new Guid(0x6ce4122c, 0x3ee9, 0x4182, 0xb4, 0xae, 0xc1, 0x0f, 0xc0, 0x88, 0x64, 0x9d);
		public static readonly Guid CODECAPI_AVDSPLoudnessEqualization = new Guid(0x8afd1a15, 0x1812, 0x4cbf, 0x93, 0x19, 0x43, 0x3a, 0x5b, 0x2a, 0x3b, 0x27);
		public static readonly Guid CODECAPI_AVDSPSpeakerFill = new Guid(0x5612bca1, 0x56da, 0x4582, 0x8d, 0xa1, 0xca, 0x80, 0x90, 0xf9, 0x27, 0x68);
		public static readonly Guid CODECAPI_AVPriorityControl = new Guid(0x54ba3dc8, 0xbdde, 0x4329, 0xb1, 0x87, 0x20, 0x18, 0xbc, 0x5c, 0x2b, 0xa1);
		public static readonly Guid CODECAPI_AVRealtimeControl = new Guid(0x6f440632, 0xc4ad, 0x4bf7, 0x9e, 0x52, 0x45, 0x69, 0x42, 0xb4, 0x54, 0xb0);
		public static readonly Guid CODECAPI_AVEncMaxFrameRate = new Guid(0xb98e1b31, 0x19fa, 0x4d4f, 0x99, 0x31, 0xd6, 0xa5, 0xb8, 0xaa, 0xb9, 0x3c);

		// Misc W8 attributes
		public static readonly Guid MF_ENABLE_3DVIDEO_OUTPUT = new Guid(0xbdad7bca, 0xe5f, 0x4b10, 0xab, 0x16, 0x26, 0xde, 0x38, 0x1b, 0x62, 0x93);
		public static readonly Guid MF_SA_D3D11_BINDFLAGS = new Guid(0xeacf97ad, 0x065c, 0x4408, 0xbe, 0xe3, 0xfd, 0xcb, 0xfd, 0x12, 0x8b, 0xe2);
		public static readonly Guid MF_SA_D3D11_USAGE = new Guid(0xe85fe442, 0x2ca3, 0x486e, 0xa9, 0xc7, 0x10, 0x9d, 0xda, 0x60, 0x98, 0x80);
		public static readonly Guid MF_SA_D3D11_AWARE = new Guid(0x206b4fc8, 0xfcf9, 0x4c51, 0xaf, 0xe3, 0x97, 0x64, 0x36, 0x9e, 0x33, 0xa0);
		public static readonly Guid MF_SA_D3D11_SHARED = new Guid(0x7b8f32c3, 0x6d96, 0x4b89, 0x92, 0x3, 0xdd, 0x38, 0xb6, 0x14, 0x14, 0xf3);
		public static readonly Guid MF_SA_D3D11_SHARED_WITHOUT_MUTEX = new Guid(0x39dbd44d, 0x2e44, 0x4931, 0xa4, 0xc8, 0x35, 0x2d, 0x3d, 0xc4, 0x21, 0x15);
		public static readonly Guid MF_SA_BUFFERS_PER_SAMPLE = new Guid(0x873c5171, 0x1e3d, 0x4e25, 0x98, 0x8d, 0xb4, 0x33, 0xce, 0x04, 0x19, 0x83);
		public static readonly Guid MFT_DECODER_EXPOSE_OUTPUT_TYPES_IN_NATIVE_ORDER = new Guid(0xef80833f, 0xf8fa, 0x44d9, 0x80, 0xd8, 0x41, 0xed, 0x62, 0x32, 0x67, 0xc);
		public static readonly Guid MFT_DECODER_FINAL_VIDEO_RESOLUTION_HINT = new Guid(0xdc2f8496, 0x15c4, 0x407a, 0xb6, 0xf0, 0x1b, 0x66, 0xab, 0x5f, 0xbf, 0x53);
		public static readonly Guid MFT_ENUM_HARDWARE_VENDOR_ID_Attribute = new Guid(0x3aecb0cc, 0x35b, 0x4bcc, 0x81, 0x85, 0x2b, 0x8d, 0x55, 0x1e, 0xf3, 0xaf);
		public static readonly Guid MF_WVC1_PROG_SINGLE_SLICE_CONTENT = new Guid(0x67EC2559, 0x0F2F, 0x4420, 0xA4, 0xDD, 0x2F, 0x8E, 0xE7, 0xA5, 0x73, 0x8B);
		public static readonly Guid MF_PROGRESSIVE_CODING_CONTENT = new Guid(0x8F020EEA, 0x1508, 0x471F, 0x9D, 0xA6, 0x50, 0x7D, 0x7C, 0xFA, 0x40, 0xDB);
		public static readonly Guid MF_NALU_LENGTH_SET = new Guid(0xA7911D53, 0x12A4, 0x4965, 0xAE, 0x70, 0x6E, 0xAD, 0xD6, 0xFF, 0x05, 0x51);
		public static readonly Guid MF_NALU_LENGTH_INFORMATION = new Guid(0x19124E7C, 0xAD4B, 0x465F, 0xBB, 0x18, 0x20, 0x18, 0x62, 0x87, 0xB6, 0xAF);
		public static readonly Guid MF_USER_DATA_PAYLOAD = new Guid(0xd1d4985d, 0xdc92, 0x457a, 0xb3, 0xa0, 0x65, 0x1a, 0x33, 0xa3, 0x10, 0x47);
		public static readonly Guid MF_MPEG4SINK_SPSPPS_PASSTHROUGH = new Guid(0x5601a134, 0x2005, 0x4ad2, 0xb3, 0x7d, 0x22, 0xa6, 0xc5, 0x54, 0xde, 0xb2);
		public static readonly Guid MF_MPEG4SINK_MOOV_BEFORE_MDAT = new Guid(0xf672e3ac, 0xe1e6, 0x4f10, 0xb5, 0xec, 0x5f, 0x3b, 0x30, 0x82, 0x88, 0x16);
		public static readonly Guid MF_STREAM_SINK_SUPPORTS_HW_CONNECTION = new Guid(0x9b465cbf, 0x597, 0x4f9e, 0x9f, 0x3c, 0xb9, 0x7e, 0xee, 0xf9, 0x3, 0x59);
		public static readonly Guid MF_STREAM_SINK_SUPPORTS_ROTATION = new Guid(0xb3e96280, 0xbd05, 0x41a5, 0x97, 0xad, 0x8a, 0x7f, 0xee, 0x24, 0xb9, 0x12);
		public static readonly Guid MF_DISABLE_LOCALLY_REGISTERED_PLUGINS = new Guid(0x66b16da9, 0xadd4, 0x47e0, 0xa1, 0x6b, 0x5a, 0xf1, 0xfb, 0x48, 0x36, 0x34);
		public static readonly Guid MF_LOCAL_PLUGIN_CONTROL_POLICY = new Guid(0xd91b0085, 0xc86d, 0x4f81, 0x88, 0x22, 0x8c, 0x68, 0xe1, 0xd7, 0xfa, 0x04);
		public static readonly Guid MF_TOPONODE_WORKQUEUE_MMCSS_PRIORITY = new Guid(0x5001f840, 0x2816, 0x48f4, 0x93, 0x64, 0xad, 0x1e, 0xf6, 0x61, 0xa1, 0x23);
		public static readonly Guid MF_TOPONODE_WORKQUEUE_ITEM_PRIORITY = new Guid(0xa1ff99be, 0x5e97, 0x4a53, 0xb4, 0x94, 0x56, 0x8c, 0x64, 0x2c, 0x0f, 0xf3);
		public static readonly Guid MF_AUDIO_RENDERER_ATTRIBUTE_STREAM_CATEGORY = new Guid(0xa9770471, 0x92ec, 0x4df4, 0x94, 0xfe, 0x81, 0xc3, 0x6f, 0xc, 0x3a, 0x7a);
		public static readonly Guid MFPROTECTION_PROTECTED_SURFACE = new Guid(0x4f5d9566, 0xe742, 0x4a25, 0x8d, 0x1f, 0xd2, 0x87, 0xb5, 0xfa, 0x0a, 0xde);
		public static readonly Guid MFPROTECTION_DISABLE_SCREEN_SCRAPE = new Guid(0xa21179a4, 0xb7cd, 0x40d8, 0x96, 0x14, 0x8e, 0xf2, 0x37, 0x1b, 0xa7, 0x8d);
		public static readonly Guid MFPROTECTION_VIDEO_FRAMES = new Guid(0x36a59cbc, 0x7401, 0x4a8c, 0xbc, 0x20, 0x46, 0xa7, 0xc9, 0xe5, 0x97, 0xf0);
		public static readonly Guid MFPROTECTIONATTRIBUTE_BEST_EFFORT = new Guid(0xc8e06331, 0x75f0, 0x4ec1, 0x8e, 0x77, 0x17, 0x57, 0x8f, 0x77, 0x3b, 0x46);
		public static readonly Guid MFPROTECTIONATTRIBUTE_FAIL_OVER = new Guid(0x8536abc5, 0x38f1, 0x4151, 0x9c, 0xce, 0xf5, 0x5d, 0x94, 0x12, 0x29, 0xac);
		public static readonly Guid MFPROTECTION_GRAPHICS_TRANSFER_AES_ENCRYPTION = new Guid(0xc873de64, 0xd8a5, 0x49e6, 0x88, 0xbb, 0xfb, 0x96, 0x3f, 0xd3, 0xd4, 0xce);
		public static readonly Guid MF_XVP_DISABLE_FRC = new Guid(0x2c0afa19, 0x7a97, 0x4d5a, 0x9e, 0xe8, 0x16, 0xd4, 0xfc, 0x51, 0x8d, 0x8c);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK = new Guid(0x98d24b5e, 0x5930, 0x4614, 0xb5, 0xa1, 0xf6, 0x0, 0xf9, 0x35, 0x5a, 0x78);
		public static readonly Guid MF_DEVICESTREAM_IMAGE_STREAM = new Guid(0xa7ffb865, 0xe7b2, 0x43b0, 0x9f, 0x6f, 0x9a, 0xf2, 0xa0, 0xe5, 0xf, 0xc0);
		public static readonly Guid MF_DEVICESTREAM_INDEPENDENT_IMAGE_STREAM = new Guid(0x3eeec7e, 0xd605, 0x4576, 0x8b, 0x29, 0x65, 0x80, 0xb4, 0x90, 0xd7, 0xd3);
		public static readonly Guid MF_DEVICESTREAM_STREAM_ID = new Guid(0x11bd5120, 0xd124, 0x446b, 0x88, 0xe6, 0x17, 0x6, 0x2, 0x57, 0xff, 0xf9);
		public static readonly Guid MF_DEVICESTREAM_STREAM_CATEGORY = new Guid(0x2939e7b8, 0xa62e, 0x4579, 0xb6, 0x74, 0xd4, 0x7, 0x3d, 0xfa, 0xbb, 0xba);
		public static readonly Guid MF_DEVICESTREAM_TRANSFORM_STREAM_ID = new Guid(0xe63937b7, 0xdaaf, 0x4d49, 0x81, 0x5f, 0xd8, 0x26, 0xf8, 0xad, 0x31, 0xe7);
		public static readonly Guid MF_DEVICESTREAM_EXTENSION_PLUGIN_CLSID = new Guid(0x048e6558, 0x60c4, 0x4173, 0xbd, 0x5b, 0x6a, 0x3c, 0xa2, 0x89, 0x6a, 0xee);
		public static readonly Guid MF_DEVICESTREAM_EXTENSION_PLUGIN_CONNECTION_POINT = new Guid(0x37f9375c, 0xe664, 0x4ea4, 0xaa, 0xe4, 0xcb, 0x6d, 0x1d, 0xac, 0xa1, 0xf4);
		public static readonly Guid MF_DEVICESTREAM_TAKEPHOTO_TRIGGER = new Guid(0x1d180e34, 0x538c, 0x4fbb, 0xa7, 0x5a, 0x85, 0x9a, 0xf7, 0xd2, 0x61, 0xa6);
		public static readonly Guid MF_DEVICESTREAM_MAX_FRAME_BUFFERS = new Guid(0x1684cebe, 0x3175, 0x4985, 0x88, 0x2c, 0x0e, 0xfd, 0x3e, 0x8a, 0xc1, 0x1e);

		// Windows X attributes
		public static readonly Guid MF_CAPTURE_METADATA_FRAME_RAWSTREAM = new Guid(0x9252077B, 0x2680, 0x49B9, 0xAE, 0x02, 0xB1, 0x90, 0x75, 0x97, 0x3B, 0x70);
		public static readonly Guid MF_CAPTURE_METADATA_FOCUSSTATE = new Guid(0xa87ee154, 0x997f, 0x465d, 0xb9, 0x1f, 0x29, 0xd5, 0x3b, 0x98, 0x2b, 0x88);
		public static readonly Guid MF_CAPTURE_METADATA_REQUESTED_FRAME_SETTING_ID = new Guid(0xbb3716d9, 0x8a61, 0x47a4, 0x81, 0x97, 0x45, 0x9c, 0x7f, 0xf1, 0x74, 0xd5);
		public static readonly Guid MF_CAPTURE_METADATA_EXPOSURE_TIME = new Guid(0x16b9ae99, 0xcd84, 0x4063, 0x87, 0x9d, 0xa2, 0x8c, 0x76, 0x33, 0x72, 0x9e);
		public static readonly Guid MF_CAPTURE_METADATA_EXPOSURE_COMPENSATION = new Guid(0xd198aa75, 0x4b62, 0x4345, 0xab, 0xf3, 0x3c, 0x31, 0xfa, 0x12, 0xc2, 0x99);
		public static readonly Guid MF_CAPTURE_METADATA_ISO_SPEED = new Guid(0xe528a68f, 0xb2e3, 0x44fe, 0x8b, 0x65, 0x7, 0xbf, 0x4b, 0x5a, 0x13, 0xff);
		public static readonly Guid MF_CAPTURE_METADATA_LENS_POSITION = new Guid(0xb5fc8e86, 0x11d1, 0x4e70, 0x81, 0x9b, 0x72, 0x3a, 0x89, 0xfa, 0x45, 0x20);
		public static readonly Guid MF_CAPTURE_METADATA_SCENE_MODE = new Guid(0x9cc3b54d, 0x5ed3, 0x4bae, 0xb3, 0x88, 0x76, 0x70, 0xae, 0xf5, 0x9e, 0x13);
		public static readonly Guid MF_CAPTURE_METADATA_FLASH = new Guid(0x4a51520b, 0xfb36, 0x446c, 0x9d, 0xf2, 0x68, 0x17, 0x1b, 0x9a, 0x3, 0x89);
		public static readonly Guid MF_CAPTURE_METADATA_FLASH_POWER = new Guid(0x9c0e0d49, 0x205, 0x491a, 0xbc, 0x9d, 0x2d, 0x6e, 0x1f, 0x4d, 0x56, 0x84);
		public static readonly Guid MF_CAPTURE_METADATA_WHITEBALANCE = new Guid(0xc736fd77, 0xfb9, 0x4e2e, 0x97, 0xa2, 0xfc, 0xd4, 0x90, 0x73, 0x9e, 0xe9);
		public static readonly Guid MF_CAPTURE_METADATA_ZOOMFACTOR = new Guid(0xe50b0b81, 0xe501, 0x42c2, 0xab, 0xf2, 0x85, 0x7e, 0xcb, 0x13, 0xfa, 0x5c);
		public static readonly Guid MF_CAPTURE_METADATA_FACEROIS = new Guid(0x864f25a6, 0x349f, 0x46b1, 0xa3, 0xe, 0x54, 0xcc, 0x22, 0x92, 0x8a, 0x47);
		public static readonly Guid MF_CAPTURE_METADATA_FACEROITIMESTAMPS = new Guid(0xe94d50cc, 0x3da0, 0x44d4, 0xbb, 0x34, 0x83, 0x19, 0x8a, 0x74, 0x18, 0x68);
		public static readonly Guid MF_CAPTURE_METADATA_FACEROICHARACTERIZATIONS = new Guid(0xb927a1a8, 0x18ef, 0x46d3, 0xb3, 0xaf, 0x69, 0x37, 0x2f, 0x94, 0xd9, 0xb2);
		public static readonly Guid MF_CAPTURE_METADATA_ISO_GAINS = new Guid(0x5802ac9, 0xe1d, 0x41c7, 0xa8, 0xc8, 0x7e, 0x73, 0x69, 0xf8, 0x4e, 0x1e);
		public static readonly Guid MF_CAPTURE_METADATA_SENSORFRAMERATE = new Guid(0xdb51357e, 0x9d3d, 0x4962, 0xb0, 0x6d, 0x7, 0xce, 0x65, 0xd, 0x9a, 0xa);
		public static readonly Guid MF_CAPTURE_METADATA_WHITEBALANCE_GAINS = new Guid(0xe7570c8f, 0x2dcb, 0x4c7c, 0xaa, 0xce, 0x22, 0xec, 0xe7, 0xcc, 0xe6, 0x47);
		public static readonly Guid MF_CAPTURE_METADATA_HISTOGRAM = new Guid(0x85358432, 0x2ef6, 0x4ba9, 0xa3, 0xfb, 0x6, 0xd8, 0x29, 0x74, 0xb8, 0x95);

		public static readonly Guid MF_SINK_VIDEO_PTS = new Guid(0x2162bde7, 0x421e, 0x4b90, 0x9b, 0x33, 0xe5, 0x8f, 0xbf, 0x1d, 0x58, 0xb6);
		public static readonly Guid MF_SINK_VIDEO_NATIVE_WIDTH = new Guid(0xe6d6a707, 0x1505, 0x4747, 0x9b, 0x10, 0x72, 0xd2, 0xd1, 0x58, 0xcb, 0x3a);
		public static readonly Guid MF_SINK_VIDEO_NATIVE_HEIGHT = new Guid(0xf0ca6705, 0x490c, 0x43e8, 0x94, 0x1c, 0xc0, 0xb3, 0x20, 0x6b, 0x9a, 0x65);
		public static readonly Guid MF_SINK_VIDEO_DISPLAY_ASPECT_RATIO_NUMERATOR = new Guid(0xd0f33b22, 0xb78a, 0x4879, 0xb4, 0x55, 0xf0, 0x3e, 0xf3, 0xfa, 0x82, 0xcd);
		public static readonly Guid MF_SINK_VIDEO_DISPLAY_ASPECT_RATIO_DENOMINATOR = new Guid(0x6ea1eb97, 0x1fe0, 0x4f10, 0xa6, 0xe4, 0x1f, 0x4f, 0x66, 0x15, 0x64, 0xe0);
		public static readonly Guid MF_BD_MVC_PLANE_OFFSET_METADATA = new Guid(0x62a654e4, 0xb76c, 0x4901, 0x98, 0x23, 0x2c, 0xb6, 0x15, 0xd4, 0x73, 0x18);
		public static readonly Guid MF_LUMA_KEY_ENABLE = new Guid(0x7369820f, 0x76de, 0x43ca, 0x92, 0x84, 0x47, 0xb8, 0xf3, 0x7e, 0x06, 0x49);
		public static readonly Guid MF_LUMA_KEY_LOWER = new Guid(0x93d7b8d5, 0x0b81, 0x4715, 0xae, 0xa0, 0x87, 0x25, 0x87, 0x16, 0x21, 0xe9);
		public static readonly Guid MF_LUMA_KEY_UPPER = new Guid(0xd09f39bb, 0x4602, 0x4c31, 0xa7, 0x06, 0xa1, 0x21, 0x71, 0xa5, 0x11, 0x0a);
		public static readonly Guid MF_USER_EXTENDED_ATTRIBUTES = new Guid(0xc02abac6, 0xfeb2, 0x4541, 0x92, 0x2f, 0x92, 0x0b, 0x43, 0x70, 0x27, 0x22);
		public static readonly Guid MF_INDEPENDENT_STILL_IMAGE = new Guid(0xea12af41, 0x0710, 0x42c9, 0xa1, 0x27, 0xda, 0xa3, 0xe7, 0x84, 0x83, 0xa5);
		public static readonly Guid MF_MEDIA_PROTECTION_MANAGER_PROPERTIES = new Guid(0x38BD81A9, 0xACEA, 0x4C73, 0x89, 0xB2, 0x55, 0x32, 0xC0, 0xAE, 0xCA, 0x79);
		public static readonly Guid MFPROTECTION_HARDWARE = new Guid(0x4ee7f0c1, 0x9ed7, 0x424f, 0xb6, 0xbe, 0x99, 0x6b, 0x33, 0x52, 0x88, 0x56);
		public static readonly Guid MFPROTECTION_HDCP_WITH_TYPE_ENFORCEMENT = new Guid(0xa4a585e8, 0xed60, 0x442d, 0x81, 0x4d, 0xdb, 0x4d, 0x42, 0x20, 0xa0, 0x6d);
		public static readonly Guid MF_XVP_CALLER_ALLOCATES_OUTPUT = new Guid(0x4a2cabc, 0xcab, 0x40b1, 0xa1, 0xb9, 0x75, 0xbc, 0x36, 0x58, 0xf0, 0x0);
		public static readonly Guid MF_DEVICEMFT_EXTENSION_PLUGIN_CLSID = new Guid(0x844dbae, 0x34fa, 0x48a0, 0xa7, 0x83, 0x8e, 0x69, 0x6f, 0xb1, 0xc9, 0xa8);
		public static readonly Guid MF_DEVICEMFT_CONNECTED_FILTER_KSCONTROL = new Guid(0x6a2c4fa6, 0xd179, 0x41cd, 0x95, 0x23, 0x82, 0x23, 0x71, 0xea, 0x40, 0xe5);
		public static readonly Guid MF_DEVICEMFT_CONNECTED_PIN_KSCONTROL = new Guid(0xe63310f7, 0xb244, 0x4ef8, 0x9a, 0x7d, 0x24, 0xc7, 0x4e, 0x32, 0xeb, 0xd0);
		public static readonly Guid MF_DEVICE_THERMAL_STATE_CHANGED = new Guid(0x70ccd0af, 0xfc9f, 0x4deb, 0xa8, 0x75, 0x9f, 0xec, 0xd1, 0x6c, 0x5b, 0xd4);
		public static readonly Guid MF_ACCESS_CONTROLLED_MEDIASOURCE_SERVICE = new Guid(0x14a5031, 0x2f05, 0x4c6a, 0x9f, 0x9c, 0x7d, 0xd, 0xc4, 0xed, 0xa5, 0xf4);
		public static readonly Guid MF_CONTENT_DECRYPTOR_SERVICE = new Guid(0x68a72927, 0xfc7b, 0x44ee, 0x85, 0xf4, 0x7c, 0x51, 0xbd, 0x55, 0xa6, 0x59);
		public static readonly Guid MF_CONTENT_PROTECTION_DEVICE_SERVICE = new Guid(0xff58436f, 0x76a0, 0x41fe, 0xb5, 0x66, 0x10, 0xcc, 0x53, 0x96, 0x2e, 0xdd);
		public static readonly Guid MF_SD_AUDIO_ENCODER_DELAY = new Guid(0x8e85422c, 0x73de, 0x403f, 0x9a, 0x35, 0x55, 0x0a, 0xd6, 0xe8, 0xb9, 0x51);
		public static readonly Guid MF_SD_AUDIO_ENCODER_PADDING = new Guid(0x529c7f2c, 0xac4b, 0x4e3f, 0xbf, 0xc3, 0x09, 0x02, 0x19, 0x49, 0x82, 0xcb);

		public static readonly Guid MFT_END_STREAMING_AWARE = new Guid(0x70fbc845, 0xb07e, 0x4089, 0xb0, 0x64, 0x39, 0x9d, 0xc6, 0x11, 0xf, 0x29);
		public static readonly Guid MF_SA_D3D11_ALLOW_DYNAMIC_YUV_TEXTURE = new Guid(0xce06d49f, 0x613, 0x4b9d, 0x86, 0xa6, 0xd8, 0xc4, 0xf9, 0xc1, 0x0, 0x75);
		public static readonly Guid MFT_DECODER_QUALITY_MANAGEMENT_CUSTOM_CONTROL = new Guid(0xa24e30d7, 0xde25, 0x4558, 0xbb, 0xfb, 0x71, 0x7, 0xa, 0x2d, 0x33, 0x2e);
		public static readonly Guid MFT_DECODER_QUALITY_MANAGEMENT_RECOVERY_WITHOUT_ARTIFACTS = new Guid(0xd8980deb, 0xa48, 0x425f, 0x86, 0x23, 0x61, 0x1d, 0xb4, 0x1d, 0x38, 0x10);

		public static readonly Guid MF_SOURCE_READER_D3D11_BIND_FLAGS = new Guid(0x33f3197b, 0xf73a, 0x4e14, 0x8d, 0x85, 0xe, 0x4c, 0x43, 0x68, 0x78, 0x8d);
		public static readonly Guid MF_MEDIASINK_AUTOFINALIZE_SUPPORTED = new Guid(0x48c131be, 0x135a, 0x41cb, 0x82, 0x90, 0x3, 0x65, 0x25, 0x9, 0xc9, 0x99);
		public static readonly Guid MF_MEDIASINK_ENABLE_AUTOFINALIZE = new Guid(0x34014265, 0xcb7e, 0x4cde, 0xac, 0x7c, 0xef, 0xfd, 0x3b, 0x3c, 0x25, 0x30);
		public static readonly Guid MF_READWRITE_ENABLE_AUTOFINALIZE = new Guid(0xdd7ca129, 0x8cd1, 0x4dc5, 0x9d, 0xde, 0xce, 0x16, 0x86, 0x75, 0xde, 0x61);

		public static readonly Guid MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE_IE_EDGE = new Guid(0xa6f3e465, 0x3aca, 0x442c, 0xa3, 0xf0, 0xad, 0x6d, 0xda, 0xd8, 0x39, 0xae);
		public static readonly Guid MF_MEDIA_ENGINE_TELEMETRY_APPLICATION_ID = new Guid(0x1e7b273b, 0xa7e4, 0x402a, 0x8f, 0x51, 0xc4, 0x8e, 0x88, 0xa2, 0xca, 0xbc);
		public static readonly Guid MF_MEDIA_ENGINE_TIMEDTEXT = new Guid(0x805ea411, 0x92e0, 0x4e59, 0x9b, 0x6e, 0x5c, 0x7d, 0x79, 0x15, 0xe6, 0x4f);
		public static readonly Guid MF_MEDIA_ENGINE_CONTINUE_ON_CODEC_ERROR = new Guid(0xdbcdb7f9, 0x48e4, 0x4295, 0xb7, 0x0d, 0xd5, 0x18, 0x23, 0x4e, 0xeb, 0x38);

		public static readonly Guid MF_CAPTURE_ENGINE_CAMERA_STREAM_BLOCKED = new Guid(0xA4209417, 0x8D39, 0x46F3, 0xB7, 0x59, 0x59, 0x12, 0x52, 0x8F, 0x42, 0x07);
		public static readonly Guid MF_CAPTURE_ENGINE_CAMERA_STREAM_UNBLOCKED = new Guid(0x9BE9EEF0, 0xCDAF, 0x4717, 0x85, 0x64, 0x83, 0x4A, 0xAE, 0x66, 0x41, 0x5C);
		public static readonly Guid MF_CAPTURE_ENGINE_ENABLE_CAMERA_STREAMSTATE_NOTIFICATION = new Guid(0x4C808E9D, 0xAAED, 0x4713, 0x90, 0xFB, 0xCB, 0x24, 0x06, 0x4A, 0xB8, 0xDA);
		public static readonly Guid MF_CAPTURE_ENGINE_SELECTEDCAMERAPROFILE = new Guid(0x03160B7E, 0x1C6F, 0x4DB2, 0xAD, 0x56, 0xA7, 0xC4, 0x30, 0xF8, 0x23, 0x92);
		public static readonly Guid MF_CAPTURE_ENGINE_SELECTEDCAMERAPROFILE_INDEX = new Guid(0x3CE88613, 0x2214, 0x46C3, 0xB4, 0x17, 0x82, 0xF8, 0xA3, 0x13, 0xC9, 0xC3);

		public static readonly Guid EVRConfig_AllowBatching = new Guid(0xe447df0a, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_AllowDropToBob = new Guid(0xe447df02, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_AllowDropToHalfInterlace = new Guid(0xe447df06, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_AllowDropToThrottle = new Guid(0xe447df04, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_AllowScaling = new Guid(0xe447df08, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_ForceBatching = new Guid(0xe447df09, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_ForceBob = new Guid(0xe447df01, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_ForceHalfInterlace = new Guid(0xe447df05, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_ForceScaling = new Guid(0xe447df07, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid EVRConfig_ForceThrottle = new Guid(0xe447df03, 0x10ca, 0x4d17, 0xb1, 0x7e, 0x6a, 0x84, 0x0f, 0x8a, 0x3a, 0x4c);
		public static readonly Guid MF_ASFPROFILE_MAXPACKETSIZE = new Guid(0x22587627, 0x47de, 0x4168, 0x87, 0xf5, 0xb5, 0xaa, 0x9b, 0x12, 0xa8, 0xf0);
		public static readonly Guid MF_ASFPROFILE_MINPACKETSIZE = new Guid(0x22587626, 0x47de, 0x4168, 0x87, 0xf5, 0xb5, 0xaa, 0x9b, 0x12, 0xa8, 0xf0);
		public static readonly Guid MF_CAPTURE_ENGINE_D3D_MANAGER = new Guid(0x76e25e7b, 0xd595, 0x4283, 0x96, 0x2c, 0xc5, 0x94, 0xaf, 0xd7, 0x8d, 0xdf);
		public static readonly Guid MF_CAPTURE_ENGINE_DECODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x2b8ad2e8, 0x7acb, 0x4321, 0xa6, 0x06, 0x32, 0x5c, 0x42, 0x49, 0xf4, 0xfc);
		public static readonly Guid MF_CAPTURE_ENGINE_DISABLE_DXVA = new Guid(0xf9818862, 0x179d, 0x433f, 0xa3, 0x2f, 0x74, 0xcb, 0xcf, 0x74, 0x46, 0x6d);
		public static readonly Guid MF_CAPTURE_ENGINE_DISABLE_HARDWARE_TRANSFORMS = new Guid(0xb7c42a6b, 0x3207, 0x4495, 0xb4, 0xe7, 0x81, 0xf9, 0xc3, 0x5d, 0x59, 0x91);
		public static readonly Guid MF_CAPTURE_ENGINE_ENCODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x54c63a00, 0x78d5, 0x422f, 0xaa, 0x3e, 0x5e, 0x99, 0xac, 0x64, 0x92, 0x69);
		public static readonly Guid MF_CAPTURE_ENGINE_EVENT_GENERATOR_GUID = new Guid(0xabfa8ad5, 0xfc6d, 0x4911, 0x87, 0xe0, 0x96, 0x19, 0x45, 0xf8, 0xf7, 0xce);
		public static readonly Guid MF_CAPTURE_ENGINE_EVENT_STREAM_INDEX = new Guid(0x82697f44, 0xb1cf, 0x42eb, 0x97, 0x53, 0xf8, 0x6d, 0x64, 0x9c, 0x88, 0x65);
		public static readonly Guid MF_CAPTURE_ENGINE_MEDIASOURCE_CONFIG = new Guid(0xbc6989d2, 0x0fc1, 0x46e1, 0xa7, 0x4f, 0xef, 0xd3, 0x6b, 0xc7, 0x88, 0xde);
		public static readonly Guid MF_CAPTURE_ENGINE_OUTPUT_MEDIA_TYPE_SET = new Guid(0xcaaad994, 0x83ec, 0x45e9, 0xa3, 0x0a, 0x1f, 0x20, 0xaa, 0xdb, 0x98, 0x31);
		public static readonly Guid MF_CAPTURE_ENGINE_RECORD_SINK_AUDIO_MAX_PROCESSED_SAMPLES = new Guid(0x9896e12a, 0xf707, 0x4500, 0xb6, 0xbd, 0xdb, 0x8e, 0xb8, 0x10, 0xb5, 0xf);
		public static readonly Guid MF_CAPTURE_ENGINE_RECORD_SINK_AUDIO_MAX_UNPROCESSED_SAMPLES = new Guid(0x1cddb141, 0xa7f4, 0x4d58, 0x98, 0x96, 0x4d, 0x15, 0xa5, 0x3c, 0x4e, 0xfe);
		public static readonly Guid MF_CAPTURE_ENGINE_RECORD_SINK_VIDEO_MAX_PROCESSED_SAMPLES = new Guid(0xe7b4a49e, 0x382c, 0x4aef, 0xa9, 0x46, 0xae, 0xd5, 0x49, 0xb, 0x71, 0x11);
		public static readonly Guid MF_CAPTURE_ENGINE_RECORD_SINK_VIDEO_MAX_UNPROCESSED_SAMPLES = new Guid(0xb467f705, 0x7913, 0x4894, 0x9d, 0x42, 0xa2, 0x15, 0xfe, 0xa2, 0x3d, 0xa9);
		public static readonly Guid MF_CAPTURE_ENGINE_USE_AUDIO_DEVICE_ONLY = new Guid(0x1c8077da, 0x8466, 0x4dc4, 0x8b, 0x8e, 0x27, 0x6b, 0x3f, 0x85, 0x92, 0x3b);
		public static readonly Guid MF_CAPTURE_ENGINE_USE_VIDEO_DEVICE_ONLY = new Guid(0x7e025171, 0xcf32, 0x4f2e, 0x8f, 0x19, 0x41, 0x5, 0x77, 0xb7, 0x3a, 0x66);
		public static readonly Guid MF_SOURCE_STREAM_SUPPORTS_HW_CONNECTION = new Guid(0xa38253aa, 0x6314, 0x42fd, 0xa3, 0xce, 0xbb, 0x27, 0xb6, 0x85, 0x99, 0x46);
		public static readonly Guid MF_VIDEODSP_MODE = new Guid(0x16d720f0, 0x768c, 0x11de, 0x8a, 0x39, 0x08, 0x00, 0x20, 0x0c, 0x9a, 0x66);
		public static readonly Guid MFASFSPLITTER_PACKET_BOUNDARY = new Guid(0xfe584a05, 0xe8d6, 0x42e3, 0xb1, 0x76, 0xf1, 0x21, 0x17, 0x5, 0xfb, 0x6f);
		public static readonly Guid MFSampleExtension_DeviceReferenceSystemTime = new Guid(0x6523775a, 0xba2d, 0x405f, 0xb2, 0xc5, 0x01, 0xff, 0x88, 0xe2, 0xe8, 0xf6);
		public static readonly Guid MFSampleExtension_VideoDSPMode = new Guid(0xc12d55cb, 0xd7d9, 0x476d, 0x81, 0xf3, 0x69, 0x11, 0x7f, 0x16, 0x3e, 0xa0);

		public static readonly Guid MF_MEDIA_ENGINE_CALLBACK = new Guid(0xc60381b8, 0x83a4, 0x41f8, 0xa3, 0xd0, 0xde, 0x05, 0x07, 0x68, 0x49, 0xa9);
		public static readonly Guid MF_MEDIA_ENGINE_DXGI_MANAGER = new Guid(0x065702da, 0x1094, 0x486d, 0x86, 0x17, 0xee, 0x7c, 0xc4, 0xee, 0x46, 0x48);
		public static readonly Guid MF_MEDIA_ENGINE_EXTENSION = new Guid(0x3109fd46, 0x060d, 0x4b62, 0x8d, 0xcf, 0xfa, 0xff, 0x81, 0x13, 0x18, 0xd2);
		public static readonly Guid MF_MEDIA_ENGINE_PLAYBACK_HWND = new Guid(0xd988879b, 0x67c9, 0x4d92, 0xba, 0xa7, 0x6e, 0xad, 0xd4, 0x46, 0x03, 0x9d);
		public static readonly Guid MF_MEDIA_ENGINE_OPM_HWND = new Guid(0xa0be8ee7, 0x0572, 0x4f2c, 0xa8, 0x01, 0x2a, 0x15, 0x1b, 0xd3, 0xe7, 0x26);
		public static readonly Guid MF_MEDIA_ENGINE_PLAYBACK_VISUAL = new Guid(0x6debd26f, 0x6ab9, 0x4d7e, 0xb0, 0xee, 0xc6, 0x1a, 0x73, 0xff, 0xad, 0x15);
		public static readonly Guid MF_MEDIA_ENGINE_COREWINDOW = new Guid(0xfccae4dc, 0x0b7f, 0x41c2, 0x9f, 0x96, 0x46, 0x59, 0x94, 0x8a, 0xcd, 0xdc);
		public static readonly Guid MF_MEDIA_ENGINE_VIDEO_OUTPUT_FORMAT = new Guid(0x5066893c, 0x8cf9, 0x42bc, 0x8b, 0x8a, 0x47, 0x22, 0x12, 0xe5, 0x27, 0x26);
		public static readonly Guid MF_MEDIA_ENGINE_CONTENT_PROTECTION_FLAGS = new Guid(0xe0350223, 0x5aaf, 0x4d76, 0xa7, 0xc3, 0x06, 0xde, 0x70, 0x89, 0x4d, 0xb4);
		public static readonly Guid MF_MEDIA_ENGINE_CONTENT_PROTECTION_MANAGER = new Guid(0xfdd6dfaa, 0xbd85, 0x4af3, 0x9e, 0x0f, 0xa0, 0x1d, 0x53, 0x9d, 0x87, 0x6a);
		public static readonly Guid MF_MEDIA_ENGINE_AUDIO_ENDPOINT_ROLE = new Guid(0xd2cb93d1, 0x116a, 0x44f2, 0x93, 0x85, 0xf7, 0xd0, 0xfd, 0xa2, 0xfb, 0x46);
		public static readonly Guid MF_MEDIA_ENGINE_AUDIO_CATEGORY = new Guid(0xc8d4c51d, 0x350e, 0x41f2, 0xba, 0x46, 0xfa, 0xeb, 0xbb, 0x08, 0x57, 0xf6);
		public static readonly Guid MF_MEDIA_ENGINE_STREAM_CONTAINS_ALPHA_CHANNEL = new Guid(0x5cbfaf44, 0xd2b2, 0x4cfb, 0x80, 0xa7, 0xd4, 0x29, 0xc7, 0x4c, 0x78, 0x9d);
		public static readonly Guid MF_MEDIA_ENGINE_BROWSER_COMPATIBILITY_MODE = new Guid(0x4e0212e2, 0xe18f, 0x41e1, 0x95, 0xe5, 0xc0, 0xe7, 0xe9, 0x23, 0x5b, 0xc3);
		public static readonly Guid MF_MEDIA_ENGINE_SOURCE_RESOLVER_CONFIG_STORE = new Guid(0x0ac0c497, 0xb3c4, 0x48c9, 0x9c, 0xde, 0xbb, 0x8c, 0xa2, 0x44, 0x2c, 0xa3);
		public static readonly Guid MF_MEDIA_ENGINE_NEEDKEY_CALLBACK = new Guid(0x7ea80843, 0xb6e4, 0x432c, 0x8e, 0xa4, 0x78, 0x48, 0xff, 0xe4, 0x22, 0x0e);
		public static readonly Guid MF_MEDIA_ENGINE_TRACK_ID = new Guid(0x65bea312, 0x4043, 0x4815, 0x8e, 0xab, 0x44, 0xdc, 0xe2, 0xef, 0x8f, 0x2a);

		public static readonly Guid MFPROTECTION_ACP = new Guid(0xc3fd11c6, 0xf8b7, 0x4d20, 0xb0, 0x08, 0x1d, 0xb1, 0x7d, 0x61, 0xf2, 0xda);
		public static readonly Guid MFPROTECTION_CGMSA = new Guid(0xE57E69E9, 0x226B, 0x4d31, 0xB4, 0xE3, 0xD3, 0xDB, 0x00, 0x87, 0x36, 0xDD);
		public static readonly Guid MFPROTECTION_CONSTRICTAUDIO = new Guid(0xffc99b44, 0xdf48, 0x4e16, 0x8e, 0x66, 0x09, 0x68, 0x92, 0xc1, 0x57, 0x8a);
		public static readonly Guid MFPROTECTION_CONSTRICTVIDEO = new Guid(0x193370ce, 0xc5e4, 0x4c3a, 0x8a, 0x66, 0x69, 0x59, 0xb4, 0xda, 0x44, 0x42);
		public static readonly Guid MFPROTECTION_CONSTRICTVIDEO_NOOPM = new Guid(0xa580e8cd, 0xc247, 0x4957, 0xb9, 0x83, 0x3c, 0x2e, 0xeb, 0xd1, 0xff, 0x59);
		public static readonly Guid MFPROTECTION_DISABLE = new Guid(0x8cc6d81b, 0xfec6, 0x4d8f, 0x96, 0x4b, 0xcf, 0xba, 0x0b, 0x0d, 0xad, 0x0d);
		public static readonly Guid MFPROTECTION_FFT = new Guid(0x462a56b2, 0x2866, 0x4bb6, 0x98, 0x0d, 0x6d, 0x8d, 0x9e, 0xdb, 0x1a, 0x8c);
		public static readonly Guid MFPROTECTION_HDCP = new Guid(0xAE7CC03D, 0xC828, 0x4021, 0xac, 0xb7, 0xd5, 0x78, 0xd2, 0x7a, 0xaf, 0x13);
		public static readonly Guid MFPROTECTION_TRUSTEDAUDIODRIVERS = new Guid(0x65bdf3d2, 0x0168, 0x4816, 0xa5, 0x33, 0x55, 0xd4, 0x7b, 0x02, 0x71, 0x01);
		public static readonly Guid MFPROTECTION_WMDRMOTA = new Guid(0xa267a6a1, 0x362e, 0x47d0, 0x88, 0x05, 0x46, 0x28, 0x59, 0x8a, 0x23, 0xe4);
		public static readonly Guid MFSampleExtension_Encryption_SampleID = new Guid(0x6698b84e, 0x0afa, 0x4330, 0xae, 0xb2, 0x1c, 0x0a, 0x98, 0xd7, 0xa4, 0x4d);
		public static readonly Guid MFSampleExtension_Encryption_SubSampleMappingSplit = new Guid(0xfe0254b9, 0x2aa5, 0x4edc, 0x99, 0xf7, 0x17, 0xe8, 0x9d, 0xbf, 0x91, 0x74);
		public static readonly Guid MFSampleExtension_PacketCrossOffsets = new Guid(0x2789671d, 0x389f, 0x40bb, 0x90, 0xd9, 0xc2, 0x82, 0xf7, 0x7f, 0x9a, 0xbd);
		public static readonly Guid MFSampleExtension_Content_KeyID = new Guid(0xc6c7f5b0, 0xacca, 0x415b, 0x87, 0xd9, 0x10, 0x44, 0x14, 0x69, 0xef, 0xc6);
		public static readonly Guid MF_MSE_ACTIVELIST_CALLBACK = new Guid(0x949bda0f, 0x4549, 0x46d5, 0xad, 0x7f, 0xb8, 0x46, 0xe1, 0xab, 0x16, 0x52);
		public static readonly Guid MF_MSE_BUFFERLIST_CALLBACK = new Guid(0x42e669b0, 0xd60e, 0x4afb, 0xa8, 0x5b, 0xd8, 0xe5, 0xfe, 0x6b, 0xda, 0xb5);
		public static readonly Guid MF_MSE_CALLBACK = new Guid(0x9063a7c0, 0x42c5, 0x4ffd, 0xa8, 0xa8, 0x6f, 0xcf, 0x9e, 0xa3, 0xd0, 0x0c);
		public static readonly Guid MF_MT_VIDEO_3D = new Guid(0xcb5e88cf, 0x7b5b, 0x476b, 0x85, 0xaa, 0x1c, 0xa5, 0xae, 0x18, 0x75, 0x55);
	}

	public static class MF_MEDIA_SHARING_ENGINE
	{
		public static readonly Guid DEVICE_NAME = new Guid(0x771e05d1, 0x862f, 0x4299, 0x95, 0xac, 0xae, 0x81, 0xfd, 0x14, 0xf3, 0xe7);
		public static readonly Guid DEVICE = new Guid(0xb461c58a, 0x7a08, 0x4b98, 0x99, 0xa8, 0x70, 0xfd, 0x5f, 0x3b, 0xad, 0xfd);
		public static readonly Guid INITIAL_SEEK_TIME = new Guid(0x6f3497f5, 0xd528, 0x4a4f, 0x8d, 0xd7, 0xdb, 0x36, 0x65, 0x7e, 0xc4, 0xc9);
		public static readonly Guid MF_SHUTDOWN_RENDERER_ON_ENGINE_SHUTDOWN = new Guid(0xc112d94d, 0x6b9c, 0x48f8, 0xb6, 0xf9, 0x79, 0x50, 0xff, 0x9a, 0xb7, 0x1e);
		public static readonly Guid MF_PREFERRED_SOURCE_URI = new Guid(0x5fc85488, 0x436a, 0x4db8, 0x90, 0xaf, 0x4d, 0xb4, 0x2, 0xae, 0x5c, 0x57);
		public static readonly Guid MF_SHARING_ENGINE_SHAREDRENDERER = new Guid(0xefa446a0, 0x73e7, 0x404e, 0x8a, 0xe2, 0xfe, 0xf6, 0x0a, 0xf5, 0xa3, 0x2b);
		public static readonly Guid MF_SHARING_ENGINE_CALLBACK = new Guid(0x57dc1e95, 0xd252, 0x43fa, 0x9b, 0xbc, 0x18, 0x0, 0x70, 0xee, 0xfe, 0x6d);
	}

	public static class MF_CAPTURE_ENGINE
	{
		public static readonly Guid INITIALIZED = new Guid(0x219992bc, 0xcf92, 0x4531, 0xa1, 0xae, 0x96, 0xe1, 0xe8, 0x86, 0xc8, 0xf1);
		public static readonly Guid PREVIEW_STARTED = new Guid(0xa416df21, 0xf9d3, 0x4a74, 0x99, 0x1b, 0xb8, 0x17, 0x29, 0x89, 0x52, 0xc4);
		public static readonly Guid PREVIEW_STOPPED = new Guid(0x13d5143c, 0x1edd, 0x4e50, 0xa2, 0xef, 0x35, 0x0a, 0x47, 0x67, 0x80, 0x60);
		public static readonly Guid RECORD_STARTED = new Guid(0xac2b027b, 0xddf9, 0x48a0, 0x89, 0xbe, 0x38, 0xab, 0x35, 0xef, 0x45, 0xc0);
		public static readonly Guid RECORD_STOPPED = new Guid(0x55e5200a, 0xf98f, 0x4c0d, 0xa9, 0xec, 0x9e, 0xb2, 0x5e, 0xd3, 0xd7, 0x73);
		public static readonly Guid PHOTO_TAKEN = new Guid(0x3c50c445, 0x7304, 0x48eb, 0x86, 0x5d, 0xbb, 0xa1, 0x9b, 0xa3, 0xaf, 0x5c);
		public static readonly Guid ERROR = new Guid(0x46b89fc6, 0x33cc, 0x4399, 0x9d, 0xad, 0x78, 0x4d, 0xe7, 0x7d, 0x58, 0x7c);
		public static readonly Guid EFFECT_ADDED = new Guid(0xaa8dc7b5, 0xa048, 0x4e13, 0x8e, 0xbe, 0xf2, 0x3c, 0x46, 0xc8, 0x30, 0xc1);
		public static readonly Guid EFFECT_REMOVED = new Guid(0xc6e8db07, 0xfb09, 0x4a48, 0x89, 0xc6, 0xbf, 0x92, 0xa0, 0x42, 0x22, 0xc9);
		public static readonly Guid ALL_EFFECTS_REMOVED = new Guid(0xfded7521, 0x8ed8, 0x431a, 0xa9, 0x6b, 0xf3, 0xe2, 0x56, 0x5e, 0x98, 0x1c);
		public static readonly Guid D3D_MANAGER = new Guid(0x76e25e7b, 0xd595, 0x4283, 0x96, 0x2c, 0xc5, 0x94, 0xaf, 0xd7, 0x8d, 0xdf);
		public static readonly Guid RECORD_SINK_VIDEO_MAX_UNPROCESSED_SAMPLES = new Guid(0xb467f705, 0x7913, 0x4894, 0x9d, 0x42, 0xa2, 0x15, 0xfe, 0xa2, 0x3d, 0xa9);
		public static readonly Guid RECORD_SINK_AUDIO_MAX_UNPROCESSED_SAMPLES = new Guid(0x1cddb141, 0xa7f4, 0x4d58, 0x98, 0x96, 0x4d, 0x15, 0xa5, 0x3c, 0x4e, 0xfe);
		// value changed by MS - public static readonly Guid RECORD_SINK_VIDEO_MAX_PROCESSED_SAMPLES = new Guid(0xe7b4a49e, 0x382c, 0x4aef, 0xa9, 0x46, 0xae, 0xd5, 0x49, 0xb, 0x71, 0x11);
		public static readonly Guid RECORD_SINK_VIDEO_MAX_PROCESSED_SAMPLES = new Guid(0x9896e12a, 0xf707, 0x4500, 0xb6, 0xbd, 0xdb, 0x8e, 0xb8, 0x10, 0xb5, 0xf);
		public static readonly Guid RECORD_SINK_AUDIO_MAX_PROCESSED_SAMPLES = new Guid(0xe7b4a49e, 0x382c, 0x4aef, 0xa9, 0x46, 0xae, 0xd5, 0x49, 0xb, 0x71, 0x11);
		public static readonly Guid USE_AUDIO_DEVICE_ONLY = new Guid(0x1c8077da, 0x8466, 0x4dc4, 0x8b, 0x8e, 0x27, 0x6b, 0x3f, 0x85, 0x92, 0x3b);
		public static readonly Guid USE_VIDEO_DEVICE_ONLY = new Guid(0x7e025171, 0xcf32, 0x4f2e, 0x8f, 0x19, 0x41, 0x5, 0x77, 0xb7, 0x3a, 0x66);
		public static readonly Guid DISABLE_HARDWARE_TRANSFORMS = new Guid(0xb7c42a6b, 0x3207, 0x4495, 0xb4, 0xe7, 0x81, 0xf9, 0xc3, 0x5d, 0x59, 0x91);
		public static readonly Guid DISABLE_DXVA = new Guid(0xf9818862, 0x179d, 0x433f, 0xa3, 0x2f, 0x74, 0xcb, 0xcf, 0x74, 0x46, 0x6d);
		public static readonly Guid MEDIASOURCE_CONFIG = new Guid(0xbc6989d2, 0x0fc1, 0x46e1, 0xa7, 0x4f, 0xef, 0xd3, 0x6b, 0xc7, 0x88, 0xde);
		public static readonly Guid DECODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x2b8ad2e8, 0x7acb, 0x4321, 0xa6, 0x06, 0x32, 0x5c, 0x42, 0x49, 0xf4, 0xfc);
		public static readonly Guid ENCODER_MFT_FIELDOFUSE_UNLOCK_Attribute = new Guid(0x54c63a00, 0x78d5, 0x422f, 0xaa, 0x3e, 0x5e, 0x99, 0xac, 0x64, 0x92, 0x69);
		// removed by MS - public static readonly Guid DISABLE_LOW_LATENCY = new Guid(0xdab5a16d, 0x1f0f, 0x44da, 0xad, 0x48, 0x82, 0x27, 0x31, 0x89, 0x43, 0xb8);
		public static readonly Guid EVENT_GENERATOR_GUID = new Guid(0xabfa8ad5, 0xfc6d, 0x4911, 0x87, 0xe0, 0x96, 0x19, 0x45, 0xf8, 0xf7, 0xce);
		public static readonly Guid EVENT_STREAM_INDEX = new Guid(0x82697f44, 0xb1cf, 0x42eb, 0x97, 0x53, 0xf8, 0x6d, 0x64, 0x9c, 0x88, 0x65);

		public static readonly Guid SOURCE_CURRENT_DEVICE_MEDIA_TYPE_SET = new Guid(0xe7e75e4c, 0x039c, 0x4410, 0x81, 0x5b, 0x87, 0x41, 0x30, 0x7b, 0x63, 0xaa);
		public static readonly Guid SINK_PREPARED = new Guid(0x7BFCE257, 0x12B1, 0x4409, 0x8C, 0x34, 0xD4, 0x45, 0xDA, 0xAB, 0x75, 0x78);
		public static readonly Guid OUTPUT_MEDIA_TYPE_SET = new Guid(0xcaaad994, 0x83ec, 0x45e9, 0xa3, 0x0a, 0x1f, 0x20, 0xaa, 0xdb, 0x98, 0x31);
	}

	public static class MFTranscodeContainerType
	{
		public static readonly Guid ASF = new Guid(0x430f6f6e, 0xb6bf, 0x4fc1, 0xa0, 0xbd, 0x9e, 0xe4, 0x6e, 0xee, 0x2a, 0xfb);
		public static readonly Guid MPEG4 = new Guid(0xdc6cd05d, 0xb9d0, 0x40ef, 0xbd, 0x35, 0xfa, 0x62, 0x2c, 0x1a, 0xb2, 0x8a);
		public static readonly Guid MP3 = new Guid(0xe438b912, 0x83f1, 0x4de6, 0x9e, 0x3a, 0x9f, 0xfb, 0xc6, 0xdd, 0x24, 0xd1);
		public static readonly Guid x3GP = new Guid(0x34c50167, 0x4472, 0x4f34, 0x9e, 0xa0, 0xc4, 0x9f, 0xba, 0xcf, 0x03, 0x7d);
		public static readonly Guid AC3 = new Guid(0x6d8d91c3, 0x8c91, 0x4ed1, 0x87, 0x42, 0x8c, 0x34, 0x7d, 0x5b, 0x44, 0xd0);
		public static readonly Guid ADTS = new Guid(0x132fd27d, 0x0f02, 0x43de, 0xa3, 0x01, 0x38, 0xfb, 0xbb, 0xb3, 0x83, 0x4e);
		public static readonly Guid MPEG2 = new Guid(0xbfc2dbf9, 0x7bb4, 0x4f8f, 0xaf, 0xde, 0xe1, 0x12, 0xc4, 0x4b, 0xa8, 0x82);
		public static readonly Guid WAVE = new Guid(0x64c3453c, 0x0f26, 0x4741, 0xbe, 0x63, 0x87, 0xbd, 0xf8, 0xbb, 0x93, 0x5b);
		public static readonly Guid AVI = new Guid(0x7edfe8af, 0x402f, 0x4d76, 0xa3, 0x3c, 0x61, 0x9f, 0xd1, 0x57, 0xd0, 0xf1);
		public static readonly Guid FMPEG4 = new Guid(0x9ba876f1, 0x419f, 0x4b77, 0xa1, 0xe0, 0x35, 0x95, 0x9d, 0x9d, 0x40, 0x4);
		public static readonly Guid FLAC = new Guid(0x31344aa3, 0x05a9, 0x42b5, 0x90, 0x1b, 0x8e, 0x9d, 0x42, 0x57, 0xf7, 0x5e);
		public static readonly Guid AMR = new Guid(0x25d5ad3, 0x621a, 0x475b, 0x96, 0x4d, 0x66, 0xb1, 0xc8, 0x24, 0xf0, 0x79);
	}

	public static class MFConnector
	{
		public static readonly Guid MFCONNECTOR_AGP = new Guid(0xac3aef60, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_COMPONENT = new Guid(0x57cd596b, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_COMPOSITE = new Guid(0x57cd596a, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_D_JPN = new Guid(0x57cd5970, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_DISPLAYPORT_EMBEDDED = new Guid(0x57cd5973, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_DISPLAYPORT_EXTERNAL = new Guid(0x57cd5972, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_DVI = new Guid(0x57cd596c, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_HDMI = new Guid(0x57cd596d, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_LVDS = new Guid(0x57cd596e, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_PCI = new Guid(0xac3aef5d, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_PCI_Express = new Guid(0xac3aef5f, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_PCIX = new Guid(0xac3aef5e, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_SPDIF = new Guid(0xb94a712, 0xad3e, 0x4cee, 0x83, 0xce, 0xce, 0x32, 0xe3, 0xdb, 0x65, 0x22);
		public static readonly Guid MFCONNECTOR_SVIDEO = new Guid(0x57cd5969, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_UDI_EMBEDDED = new Guid(0x57cd5975, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_MIRACAST = new Guid(0x57cd5977, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_UDI_EXTERNAL = new Guid(0x57cd5974, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_UNKNOWN = new Guid(0xac3aef5c, 0xce43, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFCONNECTOR_VGA = new Guid(0x57cd5968, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
	}

	public static class MFTransformCategory
	{
		// {d6c02d4b-6833-45b4-971a-05a4b04bab91}   MFT_CATEGORY_VIDEO_DECODER
		public static readonly Guid MFT_CATEGORY_VIDEO_DECODER = new Guid(0xd6c02d4b, 0x6833, 0x45b4, 0x97, 0x1a, 0x05, 0xa4, 0xb0, 0x4b, 0xab, 0x91);

		// {f79eac7d-e545-4387-bdee-d647d7bde42a}   MFT_CATEGORY_VIDEO_ENCODER
		public static readonly Guid MFT_CATEGORY_VIDEO_ENCODER = new Guid(0xf79eac7d, 0xe545, 0x4387, 0xbd, 0xee, 0xd6, 0x47, 0xd7, 0xbd, 0xe4, 0x2a);

		// {12e17c21-532c-4a6e-8a1c-40825a736397}   MFT_CATEGORY_VIDEO_EFFECT
		public static readonly Guid MFT_CATEGORY_VIDEO_EFFECT = new Guid(0x12e17c21, 0x532c, 0x4a6e, 0x8a, 0x1c, 0x40, 0x82, 0x5a, 0x73, 0x63, 0x97);

		// {059c561e-05ae-4b61-b69d-55b61ee54a7b}   MFT_CATEGORY_MULTIPLEXER
		public static readonly Guid MFT_CATEGORY_MULTIPLEXER = new Guid(0x059c561e, 0x05ae, 0x4b61, 0xb6, 0x9d, 0x55, 0xb6, 0x1e, 0xe5, 0x4a, 0x7b);

		// {a8700a7a-939b-44c5-99d7-76226b23b3f1}   MFT_CATEGORY_DEMULTIPLEXER
		public static readonly Guid MFT_CATEGORY_DEMULTIPLEXER = new Guid(0xa8700a7a, 0x939b, 0x44c5, 0x99, 0xd7, 0x76, 0x22, 0x6b, 0x23, 0xb3, 0xf1);

		// {9ea73fb4-ef7a-4559-8d5d-719d8f0426c7}   MFT_CATEGORY_AUDIO_DECODER
		public static readonly Guid MFT_CATEGORY_AUDIO_DECODER = new Guid(0x9ea73fb4, 0xef7a, 0x4559, 0x8d, 0x5d, 0x71, 0x9d, 0x8f, 0x04, 0x26, 0xc7);

		// {91c64bd0-f91e-4d8c-9276-db248279d975}   MFT_CATEGORY_AUDIO_ENCODER
		public static readonly Guid MFT_CATEGORY_AUDIO_ENCODER = new Guid(0x91c64bd0, 0xf91e, 0x4d8c, 0x92, 0x76, 0xdb, 0x24, 0x82, 0x79, 0xd9, 0x75);

		// {11064c48-3648-4ed0-932e-05ce8ac811b7}   MFT_CATEGORY_AUDIO_EFFECT
		public static readonly Guid MFT_CATEGORY_AUDIO_EFFECT = new Guid(0x11064c48, 0x3648, 0x4ed0, 0x93, 0x2e, 0x05, 0xce, 0x8a, 0xc8, 0x11, 0xb7);

		// {302EA3FC-AA5F-47f9-9F7A-C2188BB163021}   MFT_CATEGORY_VIDEO_PROCESSOR
		public static readonly Guid MFT_CATEGORY_VIDEO_PROCESSOR = new Guid(0x302ea3fc, 0xaa5f, 0x47f9, 0x9f, 0x7a, 0xc2, 0x18, 0x8b, 0xb1, 0x63, 0x2);

		// {90175d57-b7ea-4901-aeb3-933a8747756f}   MFT_CATEGORY_OTHER
		public static readonly Guid MFT_CATEGORY_OTHER = new Guid(0x90175d57, 0xb7ea, 0x4901, 0xae, 0xb3, 0x93, 0x3a, 0x87, 0x47, 0x75, 0x6f);

	}

	public static class MFEnabletype
	{
		public static readonly Guid MFENABLETYPE_MF_RebootRequired = new Guid(0x6d4d3d4b, 0x0ece, 0x4652, 0x8b, 0x3a, 0xf2, 0xd2, 0x42, 0x60, 0xd8, 0x87);
		public static readonly Guid MFENABLETYPE_MF_UpdateRevocationInformation = new Guid(0xe558b0b5, 0xb3c4, 0x44a0, 0x92, 0x4c, 0x50, 0xd1, 0x78, 0x93, 0x23, 0x85);
		public static readonly Guid MFENABLETYPE_MF_UpdateUntrustedComponent = new Guid(0x9879f3d6, 0xcee2, 0x48e6, 0xb5, 0x73, 0x97, 0x67, 0xab, 0x17, 0x2f, 0x16);
		public static readonly Guid MFENABLETYPE_WMDRMV1_LicenseAcquisition = new Guid(0x4ff6eeaf, 0xb43, 0x4797, 0x9b, 0x85, 0xab, 0xf3, 0x18, 0x15, 0xe7, 0xb0);
		public static readonly Guid MFENABLETYPE_WMDRMV7_Individualization = new Guid(0xacd2c84a, 0xb303, 0x4f65, 0xbc, 0x2c, 0x2c, 0x84, 0x8d, 0x1, 0xa9, 0x89);
		public static readonly Guid MFENABLETYPE_WMDRMV7_LicenseAcquisition = new Guid(0x3306df, 0x4a06, 0x4884, 0xa0, 0x97, 0xef, 0x6d, 0x22, 0xec, 0x84, 0xa3);
	}

	public static class MFRepresentation
	{
		/// <summary> FORMAT_DvInfo </summary>
		public static readonly Guid DvInfo = new Guid(0x05589f84, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
		/// <summary> FORMAT_MFVideoFormat </summary>
		public static readonly Guid MFVideoFormat = new Guid(0xaed4ab2d, 0x7326, 0x43cb, 0x94, 0x64, 0xc8, 0x79, 0xca, 0xb9, 0xc4, 0x3d);
		/// <summary> FORMAT_MPEG2Video </summary>
		public static readonly Guid MPEG2Video = new Guid(0xe06d80e3, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x05f, 0x6c, 0xbb, 0xea);
		/// <summary> FORMAT_MPEGStreams </summary>
		public static readonly Guid MPEGStreams = new Guid(0x05589f83, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
		/// <summary> FORMAT_MPEGVideo </summary>
		public static readonly Guid MPEGVideo = new Guid(0x05589f82, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
		/// <summary> FORMAT_VideoInfo </summary>
		public static readonly Guid VideoInfo = new Guid(0x05589f80, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
		/// <summary> FORMAT_VideoInfo2 </summary>
		public static readonly Guid VideoInfo2 = new Guid(0xf72a76A0, 0xeb0a, 0x11d0, 0xac, 0xe4, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);
		/// <summary> FORMAT_WaveFormatEx </summary>
		public static readonly Guid WaveFormatEx = new Guid(0x05589f81, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);
		/// <summary> AM_MEDIA_TYPE_REPRESENTATION </summary>
		public static readonly Guid AMMediaType = new Guid(0xe2e42ad2, 0x132c, 0x491e, 0xa2, 0x68, 0x3c, 0x7c, 0x2d, 0xca, 0x18, 0x1f);
	}

	public static class MFProperties
	{
		// Media Foundation Properties
		public static readonly Guid MFNETSOURCE_ACCELERATEDSTREAMINGDURATION = new Guid(0x3cb1f277, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_AUTORECONNECTLIMIT = new Guid(0x3cb1f27a, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_AUTORECONNECTPROGRESS = new Guid(0x3cb1f282, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_BROWSERUSERAGENT = new Guid(0x3cb1f28b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_BROWSERWEBPAGE = new Guid(0x3cb1f28c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_BUFFERINGTIME = new Guid(0x3cb1f276, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_CACHEENABLED = new Guid(0x3cb1f279, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_CONNECTIONBANDWIDTH = new Guid(0x3cb1f278, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_CREDENTIAL_MANAGER = new Guid(0x3cb1f280, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_DRMNET_LICENSE_REPRESENTATION = new Guid(0x47eae1bd, 0xbdfe, 0x42e2, 0x82, 0xf3, 0x54, 0xa4, 0x8c, 0x17, 0x96, 0x2d);
		public static readonly Guid MFNETSOURCE_ENABLE_DOWNLOAD = new Guid(0x3cb1f29d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_ENABLE_HTTP = new Guid(0x3cb1f299, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_ENABLE_RTSP = new Guid(0x3cb1f298, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_ENABLE_STREAMING = new Guid(0x3cb1f29c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_ENABLE_TCP = new Guid(0x3cb1f295, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_ENABLE_UDP = new Guid(0x3cb1f294, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_HOSTEXE = new Guid(0x3cb1f28f, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_HOSTVERSION = new Guid(0x3cb1f291, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_LOGURL = new Guid(0x3cb1f293, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_MAXBUFFERTIMEMS = new Guid(0x408b24e6, 0x4038, 0x4401, 0xb5, 0xb2, 0xfe, 0x70, 0x1a, 0x9e, 0xbf, 0x10);
		public static readonly Guid MFNETSOURCE_MAXUDPACCELERATEDSTREAMINGDURATION = new Guid(0x4aab2879, 0xbbe1, 0x4994, 0x9f, 0xf0, 0x54, 0x95, 0xbd, 0x25, 0x1, 0x29);
		public static readonly Guid MFNETSOURCE_PLAYERID = new Guid(0x3cb1f28e, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PLAYERUSERAGENT = new Guid(0x3cb1f292, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PLAYERVERSION = new Guid(0x3cb1f28d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PPBANDWIDTH = new Guid(0x3cb1f281, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROTOCOL = new Guid(0x3cb1f27d, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYBYPASSFORLOCAL = new Guid(0x3cb1f286, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYEXCEPTIONLIST = new Guid(0x3cb1f285, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYHOSTNAME = new Guid(0x3cb1f284, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYINFO = new Guid(0x3cb1f29b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYLOCATORFACTORY = new Guid(0x3cb1f283, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYPORT = new Guid(0x3cb1f288, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYRERUNAUTODETECTION = new Guid(0x3cb1f289, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_PROXYSETTINGS = new Guid(0x3cb1f287, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_RESENDSENABLED = new Guid(0x3cb1f27b, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_STATISTICS = new Guid(0x3cb1f274, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_THINNINGENABLED = new Guid(0x3cb1f27c, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_TRANSPORT = new Guid(0x3cb1f27e, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_UDP_PORT_RANGE = new Guid(0x3cb1f29a, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);

		public static readonly Guid MFNETSOURCE_SSLCERTIFICATE_MANAGER = new Guid(0x55e6cb27, 0xe69b, 0x4267, 0x94, 0x0c, 0x2d, 0x7e, 0xc5, 0xbb, 0x8a, 0x0f);

		public static readonly Guid MFNETSOURCE_PREVIEWMODEENABLED = new Guid(0x3cb1f27f, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_CLIENTGUID = new Guid(0x60a2c4a6, 0xf197, 0x4c14, 0xa5, 0xbf, 0x88, 0x83, 0xd, 0x24, 0x58, 0xaf);
		public static readonly Guid MFNETSOURCE_ENABLE_MSB = new Guid(0x3cb1f296, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MFNETSOURCE_STREAM_LANGUAGE = new Guid(0x9ab44318, 0xf7cd, 0x4f2d, 0x8d, 0x6d, 0xfa, 0x35, 0xb4, 0x92, 0xce, 0xcb);
		public static readonly Guid MFNETSOURCE_LOGPARAMS = new Guid(0x64936ae8, 0x9418, 0x453a, 0x8c, 0xda, 0x3e, 0xa, 0x66, 0x8b, 0x35, 0x3b);

		public static readonly Guid MFNETSOURCE_ENABLE_PRIVATEMODE = new Guid(0x824779d8, 0xf18b, 0x4405, 0x8c, 0xf1, 0x46, 0x4f, 0xb5, 0xaa, 0x8f, 0x71);
		public static readonly Guid MFNETSOURCE_PEERMANAGER = new Guid(0x48b29adb, 0xfebf, 0x45ee, 0xa9, 0xbf, 0xef, 0xb8, 0x1c, 0x49, 0x2e, 0xfc);
		public static readonly Guid MFNETSOURCE_FRIENDLYNAME = new Guid(0x5b2a7757, 0xbc6b, 0x447e, 0xaa, 0x06, 0x0d, 0xda, 0x1c, 0x64, 0x6e, 0x2f);
		public static readonly Guid MFNETSOURCE_RESOURCE_FILTER = new Guid(0x815d0ff6, 0x265a, 0x4477, 0x9e, 0x46, 0x7b, 0x80, 0xad, 0x80, 0xb5, 0xfb);
	}

	public static class MFServices
	{
		public static readonly Guid MF_TIMECODE_SERVICE = new Guid(0xa0d502a7, 0x0eb3, 0x4885, 0xb1, 0xb9, 0x9f, 0xeb, 0x0d, 0x08, 0x34, 0x54);
		public static readonly Guid MF_PROPERTY_HANDLER_SERVICE = new Guid(0xa3face02, 0x32b8, 0x41dd, 0x90, 0xe7, 0x5f, 0xef, 0x7c, 0x89, 0x91, 0xb5);
		public static readonly Guid MF_METADATA_PROVIDER_SERVICE = new Guid(0xdb214084, 0x58a4, 0x4d2e, 0xb8, 0x4f, 0x6f, 0x75, 0x5b, 0x2f, 0x7a, 0xd);
		public static readonly Guid MF_PMP_SERVER_CONTEXT = new Guid(0x2f00c910, 0xd2cf, 0x4278, 0x8b, 0x6a, 0xd0, 0x77, 0xfa, 0xc3, 0xa2, 0x5f);
		public static readonly Guid MF_QUALITY_SERVICES = new Guid(0xb7e2be11, 0x2f96, 0x4640, 0xb5, 0x2c, 0x28, 0x23, 0x65, 0xbd, 0xf1, 0x6c);
		public static readonly Guid MF_RATE_CONTROL_SERVICE = new Guid(0x866fa297, 0xb802, 0x4bf8, 0x9d, 0xc9, 0x5e, 0x3b, 0x6a, 0x9f, 0x53, 0xc9);
		public static readonly Guid MF_REMOTE_PROXY = new Guid(0x2f00c90e, 0xd2cf, 0x4278, 0x8b, 0x6a, 0xd0, 0x77, 0xfa, 0xc3, 0xa2, 0x5f);
		public static readonly Guid MF_SAMI_SERVICE = new Guid(0x49a89ae7, 0xb4d9, 0x4ef2, 0xaa, 0x5c, 0xf6, 0x5a, 0x3e, 0x5, 0xae, 0x4e);
		public static readonly Guid MF_SOURCE_PRESENTATION_PROVIDER_SERVICE = new Guid(0xe002aadc, 0xf4af, 0x4ee5, 0x98, 0x47, 0x05, 0x3e, 0xdf, 0x84, 0x04, 0x26);
		public static readonly Guid MF_TOPONODE_ATTRIBUTE_EDITOR_SERVICE = new Guid(0x65656e1a, 0x077f, 0x4472, 0x83, 0xef, 0x31, 0x6f, 0x11, 0xd5, 0x08, 0x7a);
		public static readonly Guid MF_WORKQUEUE_SERVICES = new Guid(0x8e37d489, 0x41e0, 0x413a, 0x90, 0x68, 0x28, 0x7c, 0x88, 0x6d, 0x8d, 0xda);
		public static readonly Guid MFNET_SAVEJOB_SERVICE = new Guid(0xb85a587f, 0x3d02, 0x4e52, 0x95, 0x65, 0x55, 0xd3, 0xec, 0x1e, 0x7f, 0xf7);
		public static readonly Guid MFNETSOURCE_STATISTICS_SERVICE = new Guid(0x3cb1f275, 0x0505, 0x4c5d, 0xae, 0x71, 0x0a, 0x55, 0x63, 0x44, 0xef, 0xa1);
		public static readonly Guid MR_AUDIO_POLICY_SERVICE = new Guid(0x911fd737, 0x6775, 0x4ab0, 0xa6, 0x14, 0x29, 0x78, 0x62, 0xfd, 0xac, 0x88);
		public static readonly Guid MR_POLICY_VOLUME_SERVICE = new Guid(0x1abaa2ac, 0x9d3b, 0x47c6, 0xab, 0x48, 0xc5, 0x95, 0x6, 0xde, 0x78, 0x4d);
		public static readonly Guid MR_STREAM_VOLUME_SERVICE = new Guid(0xf8b5fa2f, 0x32ef, 0x46f5, 0xb1, 0x72, 0x13, 0x21, 0x21, 0x2f, 0xb2, 0xc4);
		public static readonly Guid MR_VIDEO_RENDER_SERVICE = new Guid(0x1092a86c, 0xab1a, 0x459a, 0xa3, 0x36, 0x83, 0x1f, 0xbc, 0x4d, 0x11, 0xff);
		public static readonly Guid MR_VIDEO_MIXER_SERVICE = new Guid(0x73cd2fc, 0x6cf4, 0x40b7, 0x88, 0x59, 0xe8, 0x95, 0x52, 0xc8, 0x41, 0xf8);
		public static readonly Guid MR_VIDEO_ACCELERATION_SERVICE = new Guid(0xefef5175, 0x5c7d, 0x4ce2, 0xbb, 0xbd, 0x34, 0xff, 0x8b, 0xca, 0x65, 0x54);
		public static readonly Guid MR_BUFFER_SERVICE = new Guid(0xa562248c, 0x9ac6, 0x4ffc, 0x9f, 0xba, 0x3a, 0xf8, 0xf8, 0xad, 0x1a, 0x4d);
		public static readonly Guid MF_PMP_SERVICE = new Guid(0x2F00C90C, 0xD2CF, 0x4278, 0x8B, 0x6A, 0xD0, 0x77, 0xFA, 0xC3, 0xA2, 0x5F);
		public static readonly Guid MF_LOCAL_MFT_REGISTRATION_SERVICE = new Guid(0xddf5cf9c, 0x4506, 0x45aa, 0xab, 0xf0, 0x6d, 0x5d, 0x94, 0xdd, 0x1b, 0x4a);
		public static readonly Guid MF_BYTESTREAM_SERVICE = new Guid(0xab025e2b, 0x16d9, 0x4180, 0xa1, 0x27, 0xba, 0x6c, 0x70, 0x15, 0x61, 0x61);
		public static readonly Guid MF_WRAPPED_BUFFER_SERVICE = new Guid(0xab544072, 0xc269, 0x4ebc, 0xa5, 0x52, 0x1c, 0x3b, 0x32, 0xbe, 0xd5, 0xca);
		public static readonly Guid MF_WRAPPED_SAMPLE_SERVICE = new Guid(0x31f52bf2, 0xd03e, 0x4048, 0x80, 0xd0, 0x9c, 0x10, 0x46, 0xd8, 0x7c, 0x61);
		public static readonly Guid MF_MEDIASOURCE_SERVICE = new Guid(0xf09992f7, 0x9fba, 0x4c4a, 0xa3, 0x7f, 0x8c, 0x47, 0xb4, 0xe1, 0xdf, 0xe7);
		public static readonly Guid GUID_PlayToService = new Guid(0xf6a8ff9d, 0x9e14, 0x41c9, 0xbf, 0x0f, 0x12, 0x0a, 0x2b, 0x3c, 0xe1, 0x20);
		public static readonly Guid GUID_NativeDeviceService = new Guid(0xef71e53c, 0x52f4, 0x43c5, 0xb8, 0x6a, 0xad, 0x6c, 0xb2, 0x16, 0xa6, 0x1e);
		public static readonly Guid MF_WRAPPED_OBJECT = new Guid(0x2b182c4c, 0xd6ac, 0x49f4, 0x89, 0x15, 0xf7, 0x18, 0x87, 0xdb, 0x70, 0xcd);
		public static readonly Guid MF_SCRUBBING_SERVICE = new Guid(0xDD0AC3D8, 0x40E3, 0x4128, 0xAC, 0x48, 0xC0, 0xAD, 0xD0, 0x67, 0xB7, 0x14);
		public static readonly Guid MR_CAPTURE_POLICY_VOLUME_SERVICE = new Guid(0x24030acd, 0x107a, 0x4265, 0x97, 0x5c, 0x41, 0x4e, 0x33, 0xe6, 0x5f, 0x2a);
	}

	public static class MFPKEY
	{
		public static readonly PropertyKey CATEGORY = new PropertyKey(new Guid(0xc57a84c0, 0x1a80, 0x40a3, 0x97, 0xb5, 0x92, 0x72, 0xa4, 0x3, 0xc8, 0xae), 0x02);
		public static readonly PropertyKey CLSID = new PropertyKey(new Guid(0xc57a84c0, 0x1a80, 0x40a3, 0x97, 0xb5, 0x92, 0x72, 0xa4, 0x3, 0xc8, 0xae), 0x01);
		public static readonly PropertyKey EXATTRIBUTE_SUPPORTED = new PropertyKey(new Guid(0x456fe843, 0x3c87, 0x40c0, 0x94, 0x9d, 0x14, 0x9, 0xc9, 0x7d, 0xab, 0x2c), 0x01);
		public static readonly PropertyKey SourceOpenMonitor = new PropertyKey(new Guid(0x074d4637, 0xb5ae, 0x465d, 0xaf, 0x17, 0x1a, 0x53, 0x8d, 0x28, 0x59, 0xdd), 0x02);
		public static readonly PropertyKey ASFMediaSource_ApproxSeek = new PropertyKey(new Guid(0xb4cd270f, 0x244d, 0x4969, 0xbb, 0x92, 0x3f, 0x0f, 0xb8, 0x31, 0x6f, 0x10), 0x01);
		public static readonly PropertyKey MULTICHANNEL_CHANNEL_MASK = new PropertyKey(new Guid(0x58bdaf8c, 0x3224, 0x4692, 0x86, 0xd0, 0x44, 0xd6, 0x5c, 0x5b, 0xf8, 0x2b), 0x01);
		public static readonly PropertyKey ASFMediaSource_IterativeSeekIfNoIndex = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x01);
		public static readonly PropertyKey ASFMediaSource_IterativeSeek_Max_Count = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x02);
		public static readonly PropertyKey ASFMediaSource_IterativeSeek_Tolerance_In_MilliSecond = new PropertyKey(new Guid(0x170b65dc, 0x4a4e, 0x407a, 0xac, 0x22, 0x57, 0x7f, 0x50, 0xe4, 0xa3, 0x7c), 0x03);
		public static readonly PropertyKey Content_DLNA_Profile_ID = new PropertyKey(new Guid(0xcfa31b45, 0x525d, 0x4998, 0xbb, 0x44, 0x3f, 0x7d, 0x81, 0x54, 0x2f, 0xa4), 0x01);
		public static readonly PropertyKey MediaSource_DisableReadAhead = new PropertyKey(new Guid(0x26366c14, 0xc5bf, 0x4c76, 0x88, 0x7b, 0x9f, 0x17, 0x54, 0xdb, 0x5f, 0x9), 0x01);

		public static readonly PropertyKey MFP_PKEY_StreamIndex = new PropertyKey(new Guid(0xa7cf9740, 0xe8d9, 0x4a87, 0xbd, 0x8e, 0x29, 0x67, 0x0, 0x1f, 0xd3, 0xad), 0x00);
		public static readonly PropertyKey MFP_PKEY_StreamRenderingResults = new PropertyKey(new Guid(0xa7cf9740, 0xe8d9, 0x4a87, 0xbd, 0x8e, 0x29, 0x67, 0x0, 0x1f, 0xd3, 0xad), 0x01);

		public static readonly PropertyKey SBESourceMode = new PropertyKey(new Guid(0x3fae10bb, 0xf859, 0x4192, 0xb5, 0x62, 0x18, 0x68, 0xd3, 0xda, 0x3a, 0x02), 0x01);
		public static readonly PropertyKey PMP_Creation_Callback = new PropertyKey(new Guid(0x28bb4de2, 0x26a2, 0x4870, 0xb7, 0x20, 0xd2, 0x6b, 0xbe, 0xb1, 0x49, 0x42), 0x01);
		public static readonly PropertyKey HTTP_ByteStream_Enable_Urlmon = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x01);
		public static readonly PropertyKey HTTP_ByteStream_Urlmon_Bind_Flags = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x02);
		public static readonly PropertyKey HTTP_ByteStream_Urlmon_Security_Id = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x03);
		public static readonly PropertyKey HTTP_ByteStream_Urlmon_Window = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x04);
		public static readonly PropertyKey HTTP_ByteStream_Urlmon_Callback_QueryService = new PropertyKey(new Guid(0xeda8afdf, 0xc171, 0x417f, 0x8d, 0x17, 0x2e, 0x09, 0x18, 0x30, 0x32, 0x92), 0x05);
		public static readonly PropertyKey MediaProtectionSystemId = new PropertyKey(new Guid(0x636b271d, 0xddc7, 0x49e9, 0xa6, 0xc6, 0x47, 0x38, 0x59, 0x62, 0xe5, 0xbd), 0x01);
		public static readonly PropertyKey MediaProtectionSystemContext = new PropertyKey(new Guid(0x636b271d, 0xddc7, 0x49e9, 0xa6, 0xc6, 0x47, 0x38, 0x59, 0x62, 0xe5, 0xbd), 0x02);

		public static readonly PropertyKey MediaProtectionSystemIdMapping = new PropertyKey(new Guid(0x636b271d, 0xddc7, 0x49e9, 0xa6, 0xc6, 0x47, 0x38, 0x59, 0x62, 0xe5, 0xbd), 0x03);
		public static readonly PropertyKey MediaProtectionContainerGuid = new PropertyKey(new Guid(0x42af3d7c, 0xcf, 0x4a0f, 0x81, 0xf0, 0xad, 0xf5, 0x24, 0xa5, 0xa5, 0xb5), 0x1);
		public static readonly PropertyKey MediaProtectionSystemContextsPerTrack = new PropertyKey(new Guid(0x4454b092, 0xd3da, 0x49b0, 0x84, 0x52, 0x68, 0x50, 0xc7, 0xdb, 0x76, 0x4d), 0x03);
		public static readonly PropertyKey HTTP_ByteStream_Download_Mode = new PropertyKey(new Guid(0x817f11b7, 0xa982, 0x46ec, 0xa4, 0x49, 0xef, 0x58, 0xae, 0xd5, 0x3c, 0xa8), 0x01);

		public static readonly PropertyKey MFPKEY_HTTP_ByteStream_Caching_Mode = new PropertyKey(new Guid(0x86a2403e, 0xc78b, 0x44d7, 0x8b, 0xc8, 0xff, 0x72, 0x58, 0x11, 0x75, 0x08), 0x01);
		public static readonly PropertyKey MFPKEY_HTTP_ByteStream_Cache_Limit = new PropertyKey(new Guid(0x86a2403e, 0xc78b, 0x44d7, 0x8b, 0xc8, 0xff, 0x72, 0x58, 0x11, 0x75, 0x08), 0x02);
	}

	public static class CLSID
	{
		// Not attribs
		public static readonly Guid MFASFINDEXER_TYPE_TIMECODE = new Guid(0x49815231, 0x6bad, 0x44fd, 0x81, 0xa, 0x3f, 0x60, 0x98, 0x4e, 0xc7, 0xfd);

		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_GUID = new Guid(0x14dd9a1c, 0x7cff, 0x41be, 0xb1, 0xb9, 0xba, 0x1a, 0xc6, 0xec, 0xb5, 0x71);
		public static readonly Guid MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID = new Guid(0x8ac3587a, 0x4ae7, 0x42d8, 0x99, 0xe0, 0x0a, 0x60, 0x13, 0xee, 0xf9, 0x0f);

		public static readonly Guid MFCONNECTOR_SDI = new Guid(0x57cd5971, 0xce47, 0x11d9, 0x92, 0xdb, 0x00, 0x0b, 0xdb, 0x28, 0xff, 0x98);
		public static readonly Guid MFPROTECTIONATTRIBUTE_CONSTRICTVIDEO_IMAGESIZE = new Guid(0x8476fc, 0x4b58, 0x4d80, 0xa7, 0x90, 0xe7, 0x29, 0x76, 0x73, 0x16, 0x1d);
		public static readonly Guid MFPROTECTIONATTRIBUTE_HDCP_SRM = new Guid(0x6f302107, 0x3477, 0x4468, 0x8a, 0x8, 0xee, 0xf9, 0xdb, 0x10, 0xe2, 0xf);
		public static readonly Guid MFSampleExtension_DescrambleData = new Guid(0x43483be6, 0x4903, 0x4314, 0xb0, 0x32, 0x29, 0x51, 0x36, 0x59, 0x36, 0xfc);
		public static readonly Guid MFSampleExtension_SampleKeyID = new Guid(0x9ed713c8, 0x9b87, 0x4b26, 0x82, 0x97, 0xa9, 0x3b, 0x0c, 0x5a, 0x8a, 0xcc);
		public static readonly Guid MFSampleExtension_GenKeyFunc = new Guid(0x441ca1ee, 0x6b1f, 0x4501, 0x90, 0x3a, 0xde, 0x87, 0xdf, 0x42, 0xf6, 0xed);
		public static readonly Guid MFSampleExtension_GenKeyCtx = new Guid(0x188120cb, 0xd7da, 0x4b59, 0x9b, 0x3e, 0x92, 0x52, 0xfd, 0x37, 0x30, 0x1c);
		public static readonly Guid MFSampleExtension_Encryption_KeyID = new Guid(0x76376591, 0x795f, 0x4da1, 0x86, 0xed, 0x9d, 0x46, 0xec, 0xa1, 0x09, 0xa9);
		public static readonly Guid MF_SampleProtectionSalt = new Guid(0x5403deee, 0xb9ee, 0x438f, 0xaa, 0x83, 0x38, 0x4, 0x99, 0x7e, 0x56, 0x9d);

		// Generic

		public static readonly Guid CLSID_MFCaptureEngine = new Guid("efce38d3-8914-4674-a7df-ae1b3d654b8a");
		public static readonly Guid CLSID_MFSinkWriter = new Guid(0xa3bbfb17, 0x8273, 0x4e52, 0x9e, 0x0e, 0x97, 0x39, 0xdc, 0x88, 0x79, 0x90);
		public static readonly Guid CLSID_MFSourceReader = new Guid(0x1777133c, 0x0881, 0x411b, 0xa5, 0x77, 0xad, 0x54, 0x5f, 0x07, 0x14, 0xc4);
		public static readonly Guid CLSID_MFSourceResolver = new Guid(0x90eab60f, 0xe43a, 0x4188, 0xbc, 0xc4, 0xe4, 0x7f, 0xdf, 0x04, 0x86, 0x8c);
		public static readonly Guid CLSID_CreateMediaExtensionObject = new Guid(0xef65a54d, 0x0788, 0x45b8, 0x8b, 0x14, 0xbc, 0x0f, 0x6a, 0x6b, 0x51, 0x37);
		public static readonly Guid CLSID_MFImageSharingEngineClassFactory = new Guid(0xb22c3339, 0x87f3, 0x4059, 0xa0, 0xc5, 0x3, 0x7a, 0xa9, 0x70, 0x7e, 0xaf);

		public static readonly Guid MF_QUALITY_NOTIFY_PROCESSING_LATENCY = new Guid(0xf6b44af8, 0x604d, 0x46fe, 0xa9, 0x5d, 0x45, 0x47, 0x9b, 0x10, 0xc9, 0xbc);
		public static readonly Guid MF_QUALITY_NOTIFY_SAMPLE_LAG = new Guid(0x30d15206, 0xed2a, 0x4760, 0xbe, 0x17, 0xeb, 0x4a, 0x9f, 0x12, 0x29, 0x5c);
		public static readonly Guid MF_TIME_FORMAT_SEGMENT_OFFSET = new Guid(0xc8b8be77, 0x869c, 0x431d, 0x81, 0x2e, 0x16, 0x96, 0x93, 0xf6, 0x5a, 0x39);
		public static readonly Guid MF_TIME_FORMAT_ENTRY_RELATIVE = new Guid(0x4399f178, 0x46d3, 0x4504, 0xaf, 0xda, 0x20, 0xd3, 0x2e, 0x9b, 0xa3, 0x60);
		public static readonly Guid MFP_POSITIONTYPE_100NS = Guid.Empty;

		public static readonly Guid MFSubtitleFormat_TTML = new Guid(0x73e73992, 0x9a10, 0x4356, 0x95, 0x57, 0x71, 0x94, 0xe9, 0x1e, 0x3e, 0x54);
		public static readonly Guid MFSubtitleFormat_ATSC = new Guid(0x7fa7faa3, 0xfeae, 0x4e16, 0xae, 0xdf, 0x36, 0xb9, 0xac, 0xfb, 0xb0, 0x99);
		public static readonly Guid MFSubtitleFormat_WebVTT = new Guid(0xc886d215, 0xf485, 0x40bb, 0x8d, 0xb6, 0xfa, 0xdb, 0xc6, 0x19, 0xa4, 0x5d);
		public static readonly Guid MFSubtitleFormat_SRT = new Guid(0x5e467f2e, 0x77ca, 0x4ca5, 0x83, 0x91, 0xd1, 0x42, 0xed, 0x4b, 0x76, 0xc8);
		public static readonly Guid MFSubtitleFormat_SSA = new Guid(0x57176a1b, 0x1a9e, 0x4eea, 0xab, 0xef, 0xc6, 0x17, 0x60, 0x19, 0x8a, 0xc4);
		public static readonly Guid MFSubtitleFormat_CustomUserData = new Guid(0x1bb3d849, 0x6614, 0x4d80, 0x88, 0x82, 0xed, 0x24, 0xaa, 0x82, 0xda, 0x92);

		public static readonly Guid MF_MT_SECURE = new Guid(0xc5acc4fd, 0x0304, 0x4ecf, 0x80, 0x9f, 0x47, 0xbc, 0x97, 0xff, 0x63, 0xbd);
		public static readonly Guid MF_MT_VIDEO_NO_FRAME_ORDERING = new Guid(0x3f5b106f, 0x6bc2, 0x4ee3, 0xb7, 0xed, 0x89, 0x2, 0xc1, 0x8f, 0x53, 0x51);
		public static readonly Guid MF_MT_VIDEO_H264_NO_FMOASO = new Guid(0xed461cd6, 0xec9f, 0x416a, 0xa8, 0xa3, 0x26, 0xd7, 0xd3, 0x10, 0x18, 0xd7);
		public static readonly Guid MF_MT_AUDIO_FLAC_MAX_BLOCK_SIZE = new Guid(0x8b81adae, 0x4b5a, 0x4d40, 0x80, 0x22, 0xf3, 0x8d, 0x9, 0xca, 0x3c, 0x5c);
		public static readonly Guid MF_MT_OUTPUT_BUFFER_NUM = new Guid(0xa505d3ac, 0xf930, 0x436e, 0x8e, 0xde, 0x93, 0xa5, 0x09, 0xce, 0x23, 0xb2);
		public static readonly Guid MF_MT_MPEG2_ONE_FRAME_PER_PACKET = new Guid(0x91a49eb5, 0x1d20, 0x4b42, 0xac, 0xe8, 0x80, 0x42, 0x69, 0xbf, 0x95, 0xed);
		public static readonly Guid MF_MT_MPEG2_HDCP = new Guid(0x168f1b4a, 0x3e91, 0x450f, 0xae, 0xa7, 0xe4, 0xba, 0xea, 0xda, 0xe5, 0xba);
		public static readonly Guid MF_MT_IN_BAND_PARAMETER_SET = new Guid(0x75da5090, 0x910b, 0x4a03, 0x89, 0x6c, 0x7b, 0x89, 0x8f, 0xee, 0xa5, 0xaf);
		public static readonly Guid MF_DISABLE_FRAME_CORRUPTION_INFO = new Guid(0x7086e16c, 0x49c5, 0x4201, 0x88, 0x2a, 0x85, 0x38, 0xf3, 0x8c, 0xf1, 0x3a);
	}

	public static class MFSourceReader
	{
		public const int FIRST_VIDEO_STREAM = unchecked((int)0xfffffffc);
		public const int FIRST_AUDIO_STREAM = unchecked((int)0xfffffffd);
		public const int FIRST_ALL_STREAMS = unchecked((int)0xfffffffe);
	}

	public static class MFMediaType
	{
		/// <summary> From MFMediaType_Default </summary>
		public static readonly Guid Default = new Guid(0x81A412E6, 0x8103, 0x4B06, 0x85, 0x7F, 0x18, 0x62, 0x78, 0x10, 0x24, 0xAC);
		/// <summary> From MFMediaType_Audio </summary>
		public static readonly Guid Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
		/// <summary> From MFMediaType_Video </summary>
		public static readonly Guid Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
		/// <summary> From MFMediaType_Protected </summary>
		public static readonly Guid Protected = new Guid(0x7b4b6fe6, 0x9d04, 0x4494, 0xbe, 0x14, 0x7e, 0x0b, 0xd0, 0x76, 0xc8, 0xe4);
		/// <summary> From MFMediaType_SAMI </summary>
		public static readonly Guid SAMI = new Guid(0xe69669a0, 0x3dcd, 0x40cb, 0x9e, 0x2e, 0x37, 0x08, 0x38, 0x7c, 0x06, 0x16);
		/// <summary> From MFMediaType_Script </summary>
		public static readonly Guid Script = new Guid(0x72178C22, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
		/// <summary> From MFMediaType_Image </summary>
		public static readonly Guid Image = new Guid(0x72178C23, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
		/// <summary> From MFMediaType_HTML </summary>
		public static readonly Guid HTML = new Guid(0x72178C24, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
		/// <summary> From MFMediaType_Binary </summary>
		public static readonly Guid Binary = new Guid(0x72178C25, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
		/// <summary> From MFMediaType_FileTransfer </summary>
		public static readonly Guid FileTransfer = new Guid(0x72178C26, 0xE45B, 0x11D5, 0xBC, 0x2A, 0x00, 0xB0, 0xD0, 0xF3, 0xF4, 0xAB);
		/// <summary> From MFMediaType_Stream </summary>
		public static readonly Guid Stream = new Guid(0xe436eb83, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

		public static readonly Guid Base = new Guid(0x00000000, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid PCM = new Guid(0x00000001, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid Float = new Guid(0x0003, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid DTS = new Guid(0x0008, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid Dolby_AC3_SPDIF = new Guid(0x0092, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid DRM = new Guid(0x0009, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid WMAudioV8 = new Guid(0x0161, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid WMAudioV9 = new Guid(0x0162, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid WMAudio_Lossless = new Guid(0x0163, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid WMASPDIF = new Guid(0x0164, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid MSP1 = new Guid(0x000A, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid MP3 = new Guid(0x0055, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid MPEG = new Guid(0x0050, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid AAC = new Guid(0x1610, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71); // MFAudioFormat_AAC
		public static readonly Guid ADTS = new Guid(0x1600, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid AMR_NB = new Guid(0x7361, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid AMR_WB = new Guid(0x7362, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid AMR_WP = new Guid(0x7363, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

		// {00000000-767a-494d-b478-f29d25dc9037}       MFMPEG4Format_Base
		public static readonly Guid MFMPEG4Format = new Guid(0x00000000, 0x767a, 0x494d, 0xb4, 0x78, 0xf2, 0x9d, 0x25, 0xdc, 0x90, 0x37);

		public static readonly Guid RGB32 = new Guid(22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid ARGB32 = new Guid(21, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid RGB24 = new Guid(20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid RGB555 = new Guid(24, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid RGB565 = new Guid(23, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid RGB8 = new Guid(41, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid AI44 = new FourCC("AI44").ToMediaSubtype();
		public static readonly Guid AYUV = new FourCC("AYUV").ToMediaSubtype();
		public static readonly Guid YUY2 = new FourCC("YUY2").ToMediaSubtype();
		public static readonly Guid YVYU = new FourCC("YVYU").ToMediaSubtype();
		public static readonly Guid YVU9 = new FourCC("YVU9").ToMediaSubtype();
		public static readonly Guid UYVY = new FourCC("UYVY").ToMediaSubtype();
		public static readonly Guid NV11 = new FourCC("NV11").ToMediaSubtype();
		public static readonly Guid NV12 = new FourCC("NV12").ToMediaSubtype();
		public static readonly Guid YV12 = new FourCC("YV12").ToMediaSubtype();
		public static readonly Guid I420 = new FourCC("I420").ToMediaSubtype();
		public static readonly Guid IYUV = new FourCC("IYUV").ToMediaSubtype();
		public static readonly Guid Y210 = new FourCC("Y210").ToMediaSubtype();
		public static readonly Guid Y216 = new FourCC("Y216").ToMediaSubtype();
		public static readonly Guid Y410 = new FourCC("Y410").ToMediaSubtype();
		public static readonly Guid Y416 = new FourCC("Y416").ToMediaSubtype();
		public static readonly Guid Y41P = new FourCC("Y41P").ToMediaSubtype();
		public static readonly Guid Y41T = new FourCC("Y41T").ToMediaSubtype();
		public static readonly Guid Y42T = new FourCC("Y42T").ToMediaSubtype();
		public static readonly Guid P210 = new FourCC("P210").ToMediaSubtype();
		public static readonly Guid P216 = new FourCC("P216").ToMediaSubtype();
		public static readonly Guid P010 = new FourCC("P010").ToMediaSubtype();
		public static readonly Guid P016 = new FourCC("P016").ToMediaSubtype();
		public static readonly Guid v210 = new FourCC("v210").ToMediaSubtype();
		public static readonly Guid v216 = new FourCC("v216").ToMediaSubtype();
		public static readonly Guid v410 = new FourCC("v410").ToMediaSubtype();
		public static readonly Guid MP43 = new FourCC("MP43").ToMediaSubtype();
		public static readonly Guid MP4S = new FourCC("MP4S").ToMediaSubtype();
		public static readonly Guid M4S2 = new FourCC("M4S2").ToMediaSubtype();
		public static readonly Guid MP4V = new FourCC("MP4V").ToMediaSubtype();
		public static readonly Guid WMV1 = new FourCC("WMV1").ToMediaSubtype();
		public static readonly Guid WMV2 = new FourCC("WMV2").ToMediaSubtype();
		public static readonly Guid WMV3 = new FourCC("WMV3").ToMediaSubtype();
		public static readonly Guid WVC1 = new FourCC("WVC1").ToMediaSubtype();
		public static readonly Guid MSS1 = new FourCC("MSS1").ToMediaSubtype();
		public static readonly Guid MSS2 = new FourCC("MSS2").ToMediaSubtype();
		public static readonly Guid MPG1 = new FourCC("MPG1").ToMediaSubtype();
		public static readonly Guid DVSL = new FourCC("dvsl").ToMediaSubtype();
		public static readonly Guid DVSD = new FourCC("dvsd").ToMediaSubtype();
		public static readonly Guid DVHD = new FourCC("dvhd").ToMediaSubtype();
		public static readonly Guid DV25 = new FourCC("dv25").ToMediaSubtype();
		public static readonly Guid DV50 = new FourCC("dv50").ToMediaSubtype();
		public static readonly Guid DVH1 = new FourCC("dvh1").ToMediaSubtype();
		public static readonly Guid DVC = new FourCC("dvc ").ToMediaSubtype();
		public static readonly Guid H264 = new FourCC("H264").ToMediaSubtype();
		public static readonly Guid MJPG = new FourCC("MJPG").ToMediaSubtype();
		public static readonly Guid O420 = new FourCC("420O").ToMediaSubtype();
		public static readonly Guid HEVC = new FourCC("HEVC").ToMediaSubtype();
		public static readonly Guid HEVC_ES = new FourCC("HEVS").ToMediaSubtype();

		public static readonly Guid H265 = new FourCC("H265").ToMediaSubtype();
		public static readonly Guid VP80 = new FourCC("VP80").ToMediaSubtype();
		public static readonly Guid VP90 = new FourCC("VP90").ToMediaSubtype();

		public static readonly Guid FLAC = new FourCC("F1AC").ToMediaSubtype();
		public static readonly Guid ALAC = new FourCC("ALAC").ToMediaSubtype();

		public static readonly Guid MPEG2 = new Guid(0xe06d8026, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
		public static readonly Guid MFVideoFormat_H264_ES = new Guid(0x3f40f4f0, 0x5622, 0x4ff8, 0xb6, 0xd8, 0xa1, 0x7a, 0x58, 0x4b, 0xee, 0x5e);
		public static readonly Guid MFAudioFormat_Dolby_AC3 = new Guid(0xe06d802c, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x05f, 0x6c, 0xbb, 0xea);
		public static readonly Guid MFAudioFormat_Dolby_DDPlus = new Guid(0xa7fb87af, 0x2d02, 0x42fb, 0xa4, 0xd4, 0x5, 0xcd, 0x93, 0x84, 0x3b, 0xdd);
		// removed by MS - public static readonly Guid MFAudioFormat_QCELP = new Guid(0x5E7F6D41, 0xB115, 0x11D0, 0xBA, 0x91, 0x00, 0x80, 0x5F, 0xB4, 0xB9, 0x7E);

		public static readonly Guid MFAudioFormat_Vorbis = new Guid(0x8D2FD10B, 0x5841, 0x4a6b, 0x89, 0x05, 0x58, 0x8F, 0xEC, 0x1A, 0xDE, 0xD9);
		public static readonly Guid MFAudioFormat_LPCM = new Guid(0xe06d8032, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
		public static readonly Guid MFAudioFormat_PCM_HDCP = new Guid(0xa5e7ff01, 0x8411, 0x4acc, 0xa8, 0x65, 0x5f, 0x49, 0x41, 0x28, 0x8d, 0x80);
		public static readonly Guid MFAudioFormat_Dolby_AC3_HDCP = new Guid(0x97663a80, 0x8ffb, 0x4445, 0xa6, 0xba, 0x79, 0x2d, 0x90, 0x8f, 0x49, 0x7f);
		public static readonly Guid MFAudioFormat_AAC_HDCP = new Guid(0x419bce76, 0x8b72, 0x400f, 0xad, 0xeb, 0x84, 0xb5, 0x7d, 0x63, 0x48, 0x4d);
		public static readonly Guid MFAudioFormat_ADTS_HDCP = new Guid(0xda4963a3, 0x14d8, 0x4dcf, 0x92, 0xb7, 0x19, 0x3e, 0xb8, 0x43, 0x63, 0xdb);
		public static readonly Guid MFAudioFormat_Base_HDCP = new Guid(0x3884b5bc, 0xe277, 0x43fd, 0x98, 0x3d, 0x03, 0x8a, 0xa8, 0xd9, 0xb6, 0x05);
		public static readonly Guid MFVideoFormat_H264_HDCP = new Guid(0x5d0ce9dd, 0x9817, 0x49da, 0xbd, 0xfd, 0xf5, 0xf5, 0xb9, 0x8f, 0x18, 0xa6);
		public static readonly Guid MFVideoFormat_Base_HDCP = new Guid(0xeac3b9d5, 0xbd14, 0x4237, 0x8f, 0x1f, 0xba, 0xb4, 0x28, 0xe4, 0x93, 0x12);

		public static readonly Guid MPEG2Transport = new Guid(0xe06d8023, 0xdb46, 0x11cf, 0xb4, 0xd1, 0x00, 0x80, 0x5f, 0x6c, 0xbb, 0xea);
		public static readonly Guid MPEG2Program = new Guid(0x263067d1, 0xd330, 0x45dc, 0xb6, 0x69, 0x34, 0xd9, 0x86, 0xe4, 0xe3, 0xe1);

		public static readonly Guid V216_MS = new Guid(0x36313256, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid V410_MS = new Guid(0x30313456, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
	}

	public static class MFImageFormat
	{
		public static readonly Guid JPEG = new Guid(0x19e4a5aa, 0x5662, 0x4fc5, 0xa0, 0xc0, 0x17, 0x58, 0x02, 0x8e, 0x10, 0x57);
		public static readonly Guid RGB32 = new Guid(0x00000016, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
	}
	#endregion
}